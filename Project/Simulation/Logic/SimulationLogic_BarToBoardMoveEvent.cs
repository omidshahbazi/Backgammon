using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BarToBoardMoveEvent(BarToBoardMoveEvent Event)
		{
			PointData[] possibleTargetPoints = Logic.GetPossibleBarToBoardMoves(board, Event.Color);
			if (possibleTargetPoints == null)
				return;

			PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);
			if (toPoint == null)
				return;

			PlayerData player = SimulationUtilities.GetPlayer(board, Event.Color);
			if (player == null || player.MoveCount == 0)
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
			//--player.MoveCount;
			player.MoveCount = Logic.GetTotalPossibleMoveCount(board, player.Color);
			++toPoint.CheckerCount;
			toPoint.Color = Event.Color;

			mutations.Add(new BarToBoardMoveMutation(Event.To));
		}
	}
}