using GameFramework.Common.Utilities;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;

namespace Test
{
	class TestSimulation
	{
		private Simulator simulator = null;
		private SessionSerializer serializer = null;
		private Random random = null;

		private bool isFinished = false;
		private bool turnChanged = false;

		public TestSimulation()
		{
			simulator = new Simulator();
			simulator.OnBoardToBoardMove += Simulator_OnBoardToBoardMove;
			simulator.OnBoardToBarMove += Simulator_OnBoardToBarMove;
			simulator.OnBarToBoardMove += Simulator_OnBarToBoardMove;
			simulator.OnBearedOff += Simulator_OnBearedOff;
			simulator.OnTurnChanged += Simulation_OnTurnChanged;
			simulator.OnGameFinished += Simulation_OnGameFinished;
		}

		public void Run(int Seed)
		{
			simulator.Reset(Seed);

			serializer = new SessionSerializer();
			serializer.SerializeConfigState(simulator.Config);
			serializer.SerializeInitialState(simulator.Frame);

			random = new Random(Seed);

			isFinished = false;
			turnChanged = true;

			int turnNumber = 0;

			while (!isFinished)
			{
				if (!turnChanged)
					continue;

				turnChanged = false;

				++turnNumber;

				BoardData board = simulator.Frame.Board;
				PlayerColors color = board.TurnColor;
				PlayerData player = (color == PlayerColors.White ? board.WhitePlayer : board.BlackPlayer);

				if (Utilities.GetInBaseCheckerCount(board.Points, color) + player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
				{
					HandleBearOff(board, player);
				}

				while (player.MoveCount != 0)
				{
					if (player.BarCheckerCount != 0)// dice 1 in turn 32
					{
						MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(board);

						if (moves == null || moves.Length == 0)
							continue;

						SendEvent(new BarToBoardMoveEvent(color, moves[random.Next(0, moves.Length)].To.ID));

						continue;
					}
					else if (Utilities.GetInBaseCheckerCount(board.Points, color) + player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
					{
						HandleBearOff(board, player);
					}

					for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
					{
						PointData fromPoint = board.Points[i];

						MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

						if (moves != null && moves.Length != 0)
						{
							SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[random.Next(0, moves.Length)].To.ID));

							break;
						}
					}
				}

				if (!isFinished)
					SendEvent(new FinishTurnEvent(color));
			}
		}

		private void HandleBearOff(BoardData Board, PlayerData Player)
		{
			for (int i = 0; i < ConfigData.POINT_COUNT && Player.MoveCount != 0; ++i)
			{
				PointData fromPoint = Board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBearedOffs(Board, fromPoint.ID);

				if (moves != null && moves.Length != 0)
					SendEvent(new BearOffEvent(fromPoint.ID));
			}
		}

		private void SendEvent(EventBase Event)
		{
			simulator.SendEvent(Event);

			serializer.SerializeFullStep(simulator.Frame);
		}

		private void Simulator_OnBoardToBoardMove(Identifier From, Identifier To)
		{
			System.Console.WriteLine("BoardToBoardMove from {0} to {1}", From, To);
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulator_OnBoardToBarMove(Identifier From)
		{
			System.Console.WriteLine("OnBoardToBarMove from {0}", From);
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulator_OnBarToBoardMove(Identifier To)
		{
			System.Console.WriteLine("OnBarToBoardMove to {0}", To);
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulator_OnBearedOff(Identifier From)
		{
			System.Console.WriteLine("OnBearedOff from {0}", From);
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulation_OnTurnChanged()
		{
			turnChanged = true;

			System.Console.WriteLine("OnTurnChanged");
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulation_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
			//System.Console.Clear();
			System.Console.WriteLine("{0} is winner with {1} score(s)", WinnerColor, Score);
			System.Console.WriteLine();

			Utilities.PrintBoard(simulator.Frame.Board);

			System.Console.WriteLine();

			isFinished = true;
		}
	}
}