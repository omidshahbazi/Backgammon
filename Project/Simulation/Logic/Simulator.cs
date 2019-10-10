using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using GameFramework.Common.Utilities;

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
		private MutationList mutations = null;
		private HasherVisitor hasher = null;

		public FrameData Frame
		{
			get;
			private set;
		}

		public ConfigData Config
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
			Config = new ConfigData();
			mutations = new MutationList();

			Frame = new FrameData();
			Frame.Board = new BoardData();

			hasher = new HasherVisitor();
		}

		public void Reset(int Seed)
		{
			Config.Seed = Seed;
			Config.Random = new Random(Seed);

			Utilities.InitializeBoard(Config, Frame.Board);

			hasher.Reset();
			Frame.Board.Visit(hasher);
			Frame.Hash = hasher.Value;
		}

		public void SetFrame(FrameData Frame)
		{
			this.Frame = Frame;
		}

		public void SendEvent(EventBase Event)
		{
			Frame.Events = new EventBase[1] { Event };

			logic.Simulate(Config, Frame.Board, Frame.Events, mutations);

			hasher.Reset();
			Frame.Board.Visit(hasher);
			Frame.Hash = hasher.Value;

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
