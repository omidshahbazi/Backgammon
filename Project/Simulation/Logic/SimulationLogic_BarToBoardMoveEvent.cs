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
			PlayerData player = Utilities.GetPlayer(board, Event.Color);
			if (player == null || player.MoveCount == 0)
				return;

			MoveInfo[] possibleMoves = Logic.GetPossibleBarToBoardMoves(board);
			if (possibleMoves == null)
				return;

			MoveInfo moveInfo = Utilities.FindInToPoint(possibleMoves, Event.To);
			if (moveInfo == null)
				return;

			PointData toPoint = moveInfo.To;

			PlayerData opponentPlayer = Utilities.GetPlayer(board, toPoint.Color);
			if (opponentPlayer == null)
				return;

			if (!SimulationUtilities.ApplyMoveCount(board, player, Utilities.GetBarIndex(player.Color), toPoint.Index, false))
				return;

			if (toPoint.CheckerCount == 1 && toPoint.Color != board.TurnColor)
			{
				--toPoint.CheckerCount;
				++opponentPlayer.BarCheckerCount;

				mutations.Add(new BoardToBarMoveMutation(Event.To));
			}

			--player.BarCheckerCount;
			++toPoint.CheckerCount;
			toPoint.Color = Event.Color;

			SimulationUtilities.UpdateMoveCount(board, player);

			mutations.Add(new BarToBoardMoveMutation(Event.To));
		}
	}
}