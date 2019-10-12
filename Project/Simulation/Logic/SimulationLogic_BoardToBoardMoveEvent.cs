using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

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

			MoveInfo[] possibleMoves = Logic.GetPossibleBoardToBoardMoves(board, Event.From);
			if (possibleMoves == null)
				return;

			MoveInfo moveInfo = Utilities.FindInToPoint(possibleMoves, Event.To);
			if (moveInfo == null)
				return;

			PointData toPoint = moveInfo.To;

			if (!SimulationUtilities.ApplyMoveCount(board, player, fromPoint.Index, toPoint.Index, false))
				//if (!SimulationUtilities.ApplyMoveCount(board.TurnDice, player, fromPoint.Index, toPoint.Index, false))
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

			SimulationUtilities.UpdateMoveCount(board, player);

			mutations.Add(new BoardToBoardMoveMutation(Event.From, Event.To));
		}
	}
}