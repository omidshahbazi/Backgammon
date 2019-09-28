using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BearedOffEvent(BearedOffEvent Event)
		{
			PointData[] possibleTargetPoints = Logic.GetPossibleBearedOffs(board, Event.From);
			if (possibleTargetPoints == null)
				return;

			PointData fromPoint = Utilities.FindPoint(possibleTargetPoints, Event.From);
			if (fromPoint == null)
				return;

			PlayerData player = SimulationUtilities.GetPlayer(board, fromPoint.Color);
			if (player == null || player.MoveCount == 0)
				return;

			++player.BearedOffCheckersCount;
			--player.MoveCount;
			++fromPoint.CheckerCount;

			mutations.Add(new BearedOffMutation(Event.From));
		}
	}
}