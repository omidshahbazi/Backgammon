//#define PRINT_ALL_STEPS
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

				BotUtilities.PlayOneTurn(simulator, random, player);

				if (!isFinished)
					SendEvent(new FinishTurnEvent(color));
			}
		}


		private void SendEvent(EventBase Event)
		{
			simulator.SendEvent(Event);

			serializer.SerializeFullStep(simulator.Frame);
		}

		private void Simulator_OnBoardToBoardMove(Identifier From, Identifier To)
		{
#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("BoardToBoardMove from {0} to {1}", From, To);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif
		}

		private void Simulator_OnBoardToBarMove(Identifier From)
		{
#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnBoardToBarMove from {0}", From);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif
		}

		private void Simulator_OnBarToBoardMove(Identifier To)
		{
#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnBarToBoardMove to {0}", To);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif
		}

		private void Simulator_OnBearedOff(Identifier From)
		{
#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnBearedOff from {0}", From);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif
		}

		private void Simulation_OnTurnChanged()
		{
			turnChanged = true;

#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnTurnChanged");

			Utilities.PrintBoard(simulator.Frame.Board);
#endif
		}

		private void Simulation_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("{0} is winner with {1} score(s)", WinnerColor, Score);

			Utilities.PrintBoard(simulator.Frame.Board);

			isFinished = true;
		}
	}
}