using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private ConfigData config;
		private BoardData board;
		private EventBase[] events;
		private MutationList mutations;

		public void Simulate(ConfigData Config, BoardData Board, EventBase[] Events, MutationList Mutations)
		{
			config = Config;
			board = Board;
			events = Events;
			mutations = Mutations;

			SimulateBoard();

			config = null;
			board = null;
			events = null;
			mutations = null;
		}

		private void SimulateBoard()
		{
			if (board.Points == null)
				return;

			ProcessEvents();

			for (int i = 0; i < board.Points.Length; ++i)
				SimulatePoint(board.Points[i]);
		}

		private void SimulatePoint(PointData Point)
		{
		}

		private void ProcessEvents()
		{
			if (events == null)
				return;

			for (int i = 0; i < events.Length; ++i)
			{
				switch (events[i].GetType())
				{
					case EventBase.Types.Move:
						{
							MoveEvent ev = (MoveEvent)events[i];

							//ObjectData newData = SceneUtilities.CreateObject(scene, ev.UnitID, ev.Level, ev.Position, config);

							//if (newData == null)
							//	continue;

							//newData.Forward = Number3.Right;

							//FollowerData follower = ObjectUtilities.GetFollower(newData);
							//if (follower != null)
							//{
							//	ControllableData controllable = BehaviorUtilities.FindControllable(scene);
							//	if (controllable == null)
							//		continue;

							//	newData.IsInParachuteState = (newData != controllable.Holder);

							//	Number3 landingPosition = FormationUtilities.GetPosition(controllable, follower, config);

							//	if (scene.Map.GetPositionState(landingPosition) == NodeStates.Blocked)
							//		landingPosition = scene.Map.GetNearestNode(landingPosition, 100, NodeStates.Walkable).Position;

							//	MovableData movable = ObjectUtilities.GetMovable(newData);
							//	if (movable == null || !newData.IsInParachuteState)
							//		newData.Position = landingPosition;
							//	else
							//	{
							//		newData.Position = landingPosition + PARACHUTE_POSITION_DIFF;
							//		movable.LandingPosition = landingPosition;
							//	}
							//}

							//mutations.Add(new InstantiateMutation(newData.ID, ev.UnitID, newData.Position));

							//if (newData.IsInParachuteState)
							//	mutations.Add(new StartParachuteMutation(newData.ID));
						}
						break;
				}
			}
		}
	}
}
