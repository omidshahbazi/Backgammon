using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public delegate void BoardToBoardMoveEventHandler(Identifier From, Identifier To);
	public delegate void BoardToBarMoveEventHandler(Identifier From);
	public delegate void BarToBoardMoveEventHandler(Identifier To);
	public delegate void BearedOffEventHandler(Identifier From);
	public delegate void DiceChangedEventHandler();
	public delegate void GameFinishedEventHandler(PlayerColors WinnerColor, int Score);

	public class Simulator
	{
		private SimulationLogic logic = null;
		private ConfigData config = null;
		private MutationList mutations = null;

		private EventBase[] events = null;

		public BoardData Board
		{
			get;
			private set;
		}

		public event BoardToBoardMoveEventHandler OnBoardToBoardMove;
		public event BoardToBarMoveEventHandler OnBoardToBarMove;
		public event BarToBoardMoveEventHandler OnBarToBoardMove;
		public event BearedOffEventHandler OnBearedOff;
		public event DiceChangedEventHandler OnTurnChanged;
		public event GameFinishedEventHandler OnGameFinished;

		public Simulator()
		{
			logic = new SimulationLogic();
			config = new ConfigData();
			Board = new BoardData();
			mutations = new MutationList();

			events = new EventBase[1];
		}

		public void Reset(int Seed)
		{
			config.Seed = Seed;
			config.Random = new Random(Seed);

			Utilities.InitializeBoard(config, Board);
		}

		public void SendEvent(EventBase Event)
		{
			events[0] = Event;

			logic.Simulate(config, Board, events, mutations);

			HandleMutations();
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
							if (OnBoardToBoardMove != null)
							{
								BoardToBoardMoveMutation m = (BoardToBoardMoveMutation)mutation;
								OnBoardToBoardMove(m.From, m.To);
							}
						}
						break;

					case MutationBase.Types.BoardToBarMove:
						{
							if (OnBoardToBarMove != null)
							{
								BoardToBarMoveMutation m = (BoardToBarMoveMutation)mutation;
								OnBoardToBarMove(m.From);
							}
						}
						break;

					case MutationBase.Types.BarToBoardMove:
						{
							if (OnBarToBoardMove != null)
							{
								BarToBoardMoveMutation m = (BarToBoardMoveMutation)mutation;
								OnBarToBoardMove(m.To);
							}
						}
						break;

					case MutationBase.Types.BearedOff:
						{
							if (OnBearedOff != null)
							{
								BearedOffMutation m = (BearedOffMutation)mutation;
								OnTurnChanged();
							}
						}
						break;

					case MutationBase.Types.TurnChanged:
						{
							if (OnTurnChanged != null)
							{
								TurnChangedMutation m = (TurnChangedMutation)mutation;
								OnTurnChanged();
							}
						}
						break;

					case MutationBase.Types.GameFinished:
						{
							if (OnGameFinished != null)
							{
								GameFinishedMutation m = (GameFinishedMutation)mutation;
								OnGameFinished(m.WinnerColor, m.Score);
							}
						}
						break;
				}
			}

			mutations.Clear();
		}
	}
}
