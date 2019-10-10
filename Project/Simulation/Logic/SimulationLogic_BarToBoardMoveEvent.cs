using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using System;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BarToBoardMoveEvent(BarToBoardMoveEvent Event)
		{
			PlayerData player = SimulationUtilities.GetPlayer(board, Event.Color);
			if (player == null || player.MoveCount == 0)
				return;

			PointData[] possibleTargetPoints = Logic.GetPossibleBarToBoardMoves(board, Event.Color);
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

			--player.BarCheckerCount;
			++toPoint.CheckerCount;
			toPoint.Color = Event.Color;

			SimulationUtilities.ApplyMoveCount(player, board.TurnDice, SimulationUtilities.GetEndIndex(player.Color), toPoint.Index);

			mutations.Add(new BarToBoardMoveMutation(Event.To));
		}
	}
}