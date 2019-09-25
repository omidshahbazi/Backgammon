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
		}

		private void ProcessEvents()
		{
			if (events == null)
				return;

			for (int i = 0; i < events.Length; ++i)
			{
				switch (events[i].GetType())
				{
					case EventBase.Types.BoardToBoardMove:
						{
							Handle_BoardToBoardMoveEvent((BoardToBoardMoveEvent)events[i]);
						}
						break;

					case EventBase.Types.BarToBoardMove:
						{
							Handle_BarToBoardMoveEvent((BarToBoardMoveEvent)events[i]);
						}
						break;
				}
			}
		}
	}
}
