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

		private bool turnChanged = false;

		public TestSimulation()
		{
			simulator = new Simulator();
			simulator.OnBoardToBoardMove += Simulation_OnBoardToBoardMove;
			simulator.OnBoardToBarMove += Simulation_OnBoardToBarMove;
			simulator.OnBarToBoardMove += Simulation_OnBarToBoardMove;
			simulator.OnTurnChanged += Simulation_OnTurnChanged;
			simulator.OnBearedOff += Simulation_OnBearedOff;
			simulator.OnGameFinished += Simulation_OnGameFinished;
		}

		public void Run(int Seed)
		{
			simulator.Reset(Seed);

			serializer = new SessionSerializer();
			serializer.SerializeConfigState(simulator.Config);
			serializer.SerializeInitialState(simulator.Frame);

			random = new Random(Seed);

			turnChanged = true;

			while (true)
			{
				if (!turnChanged)
					continue;

				BoardData board = simulator.Frame.Board;
				PlayerColors color = board.TurnColor;
				PlayerData player = (color == PlayerColors.White ? board.WhitePlayer : board.BlackPlayer);

				int moveCount = Logic.GetTotalPossibleMoveCount(board, color);

				while (moveCount != 0)
				{
					--moveCount;

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

						if (points == null || points.Length == 0)
							continue;

						SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, points[random.Next(0, points.Length)].ID));

						break;
					}
				}

				SendEvent(new FinishTurnEvent(color));

				turnChanged = false;
			}
		}

		private void SendEvent(EventBase Event)
		{
			simulator.SendEvent(Event);

			serializer.SerializeFullStep(simulator.Frame);
		}

		private void Simulation_OnBoardToBoardMove(Identifier From, Identifier To)
		{
		}

		private void Simulation_OnBoardToBarMove(Identifier From)
		{
		}

		private void Simulation_OnBarToBoardMove(Identifier To)
		{
		}

		private void Simulation_OnTurnChanged()
		{
			turnChanged = true;
		}

		private void Simulation_OnBearedOff(Identifier From)
		{
		}

		private void Simulation_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
		}
	}
}