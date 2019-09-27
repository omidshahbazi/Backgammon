using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BarToBoardMoveEvent(BarToBoardMoveEvent Event)
		{
			PointData[] possibleTargetPoints = Logic.GetPossibleBarToBoard(board, Event.Color);
			if (possibleTargetPoints == null)
				return;

			PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);
			if (toPoint == null)
				return;

			if (toPoint.CheckerCount == 1 && toPoint.Color != board.TurnColor)
			{
				PlayerData opponentPlayer = SimulationUtilities.GetPlayer(board, toPoint.Color);

				--toPoint.CheckerCount;
				++opponentPlayer.BarCheckerCount;

				mutations.Add(new BoardToBarMoveMutation(Event.To));
			}

			PlayerData player = SimulationUtilities.GetPlayer(board, Event.Color);

			--player.BarCheckerCount;
			++toPoint.CheckerCount;
			toPoint.Color = Event.Color;

			mutations.Add(new BarToBoardMoveMutation(Event.To));
		}
	}
}