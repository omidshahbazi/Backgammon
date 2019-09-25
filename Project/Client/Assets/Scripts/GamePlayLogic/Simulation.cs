using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
	class Simulation
	{
		private const int POINT_COUNT = 24;

		private SimulationLogic logic = null;
		private ConfigData config = null;
		private BoardData board = null;
		private MutationList mutations = null;

		private EventBase[] events = null;

		public Simulation()
		{
			logic = new SimulationLogic();
			config = new ConfigData();
			board = new BoardData();
			mutations = new MutationList();

			events = new EventBase[1];
		}

		public void Refresh(int Seed)
		{
			config.Seed = Seed;
			config.Random = new Random(Seed);

			board.Points = new PointData[POINT_COUNT];
		}

		private void HandleMutations()
		{
			for (int i = 0; i < mutations.Count; ++i)
			{
				MutationBase mutation = mutations[i];

				switch (mutation.GetType())
				{
					case MutationBase.Types.BoardToBoardMove:
						{
						}
						break;

					case MutationBase.Types.BoardToBarMove:
						{
						}
						break;

					case MutationBase.Types.BarToBoardMove:
						{
						}
						break;

					case MutationBase.Types.DiceChanged:
						{
						}
						break;
				}
			}

			mutations.Clear();
		}

		private void SendEvent(EventBase Event)
		{
			events[0] = Event;

			logic.Simulate(config, board, events, mutations);

			HandleMutations();
		}
	}
}
