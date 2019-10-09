using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using System;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BoardToBoardMoveEvent(BoardToBoardMoveEvent Event)
		{
			PointData fromPoint = Utilities.FindPoint(board, Event.From);
			if (fromPoint == null)
				return;

			PlayerData player = SimulationUtilities.GetPlayer(board, fromPoint.Color);
			if (player == null || player.MoveCount == 0)
				return;

			PointData[] possibleTargetPoints = Logic.GetPossibleBoardToBoardMoves(board, Event.From);
			if (possibleTargetPoints == null)
				return;

			PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);
			if (toPoint == null)
				return;

			if (toPoint.CheckerCount == 1 && toPoint.Color != board.TurnColor)
			{
				PlayerData opponentPlayer = SimulationUtilities.GetPlayer(board, toPoint.Color);
				if (opponentPlayer == null)
					return;

				--toPoint.CheckerCount;
				++opponentPlayer.BarCheckerCount;

				mutations.Add(new BoardToBarMoveMutation(Event.To));
			}

			--fromPoint.CheckerCount;
			++toPoint.CheckerCount;
			toPoint.Color = fromPoint.Color;

			//if (board.TurnDice.AreSame && Math.Abs(toPoint.Index - fromPoint.Index) == board.TurnDice.Dice1 * 2)
			//	player.MoveCount -= 2;
			//else
			//	--player.MoveCount;
			player.MoveCount = Logic.GetTotalPossibleMoveCount(board, player.Color);

			mutations.Add(new BoardToBoardMoveMutation(Event.From, Event.To));
		}
	}
}