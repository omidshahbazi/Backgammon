using GameFramework.Common.Utilities;
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

				if (Logic.GetInBaseCheckerCount(board, color) + player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
				{
					for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
					{
						PointData fromPoint = board.Points[i];

						PointData[] points = Logic.GetPossibleBearedOffs(board, fromPoint.ID);

						if (points != null && points.Length != 0)
							SendEvent(new BearOffEvent(fromPoint.ID));
					}
				}

				while (player.MoveCount != 0)
				{
					if (player.BarCheckerCount != 0)
					{
						PointData[] points = Logic.GetPossibleBarToBoardMoves(board, color);

						if (points == null || points.Length == 0)
							continue;

						SendEvent(new BarToBoardMoveEvent(color, points[random.Next(0, points.Length)].ID));

						continue;
					}

					for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
					{
						PointData fromPoint = board.Points[i];

						PointData[] points = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

						if (points != null && points.Length != 0)
						{
							SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, points[random.Next(0, points.Length)].ID));

							break;
						}
					}
				}

				SendEvent(new FinishTurnEvent(color));
			}
		}

		private void SendEvent(EventBase Event)
		{
			simulator.SendEvent(Event);

			serializer.SerializeFullStep(simulator.Frame);
		}

		private void Simulation_OnTurnChanged()
		{
			turnChanged = true;

			Utilities.PrintBoard(simulator.Frame.Board);
		}

		private void Simulation_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
			System.Console.Clear();
			System.Console.WriteLine("Winner is {0} with {1} score(s)", WinnerColor, Score);
			System.Console.WriteLine();

			isFinished = true;
		}
	}
}