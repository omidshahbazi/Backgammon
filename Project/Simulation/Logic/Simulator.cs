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
	public delegate void TurnChangedEventHandler(PlayerColors Color);
	public delegate void GameFinishedEventHandler(PlayerColors WinnerColor, int Score);

	public class Simulator
	{
		public class PlayerStatistics
		{
			public int TotalMoveCount;
			public int BlotCount;
			public int HitCount;
		}

		private SimulationLogic logic = null;
		private MutationList mutations = null;
		private HasherVisitor hasher = null;

		private PlayerStatistics turnPlayerStatistics = null;

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

		public PlayerStatistics WhitePlayerStatistics
		{
			get;
			private set;
		}

		public PlayerStatistics BlackPlayerStatistics
		{
			get;
			private set;
		}

		public event BoardToBoardMoveEventHandler OnBoardToBoardMove;
		public event BoardToBarMoveEventHandler OnBoardToBarMove;
		public event BarToBoardMoveEventHandler OnBarToBoardMove;
		public event BearedOffEventHandler OnBearedOff;
		public event TurnChangedEventHandler OnTurnChanged;
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

			InitializeUtilities.InitializeBoard(Config, Frame.Board);

			hasher.Reset();
			Frame.Board.Visit(hasher);
			Frame.Hash = hasher.Value;

			WhitePlayerStatistics = new PlayerStatistics();
			BlackPlayerStatistics = new PlayerStatistics();
			UpdateTurnPlayerStatistics();
		}

		public void SetConfig(ConfigData Config)
		{
			this.Config = Config;
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
							BoardToBoardMoveMutation m = (BoardToBoardMoveMutation)mutation;

							++turnPlayerStatistics.TotalMoveCount;

							if (Utilities.FindPoint(Frame.Board, m.From).CheckerCount == 1)
								++turnPlayerStatistics.BlotCount;
							if (Utilities.FindPoint(Frame.Board, m.To).CheckerCount == 1)
								++turnPlayerStatistics.BlotCount;

							if (OnBoardToBoardMove != null)
								OnBoardToBoardMove(m.From, m.To);
						}
						break;

					case MutationBase.Types.BoardToBarMove:
						{
							++turnPlayerStatistics.HitCount;

							if (OnBoardToBarMove != null)
							{
								BoardToBarMoveMutation m = (BoardToBarMoveMutation)mutation;
								OnBoardToBarMove(m.From);
							}
						}
						break;

					case MutationBase.Types.BarToBoardMove:
						{
							++turnPlayerStatistics.TotalMoveCount;

							if (OnBarToBoardMove != null)
							{
								BarToBoardMoveMutation m = (BarToBoardMoveMutation)mutation;
								OnBarToBoardMove(m.To);
							}
						}
						break;

					case MutationBase.Types.BearedOff:
						{
							++turnPlayerStatistics.TotalMoveCount;

							if (OnBearedOff != null)
							{
								BearedOffMutation m = (BearedOffMutation)mutation;
								OnBearedOff(m.From);
							}
						}
						break;

					case MutationBase.Types.TurnChanged:
						{
							if (OnTurnChanged != null)
							{
								TurnChangedMutation m = (TurnChangedMutation)mutation;
								OnTurnChanged(m.Color);
							}

							UpdateTurnPlayerStatistics();
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

		private void UpdateTurnPlayerStatistics()
		{
			turnPlayerStatistics = (Frame.Board.TurnColor == PlayerColors.White ? WhitePlayerStatistics : BlackPlayerStatistics);
		}
	}
}
