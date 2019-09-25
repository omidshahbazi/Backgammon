using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		public void Handle_MoveEvent(MoveEvent Event)
		{
			PointData[] possibleTargetPoints = Logic.GetPossibleTargetPoints(board, Event.From);
			PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);

			if (toPoint == null)
				return;
		}
	}
}
