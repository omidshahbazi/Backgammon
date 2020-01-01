//#define PRINT_ALL_STEPS
using GameFramework.Common.Utilities;
using Simulation.Bot;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;

namespace Test
{
	class TestSimulation
	{
		private class Statistics
		{
			public int BlotCount;
			public int HitCount;
		}

		private Simulator simulator = null;
		private SessionSerializer serializer = null;
		private Random random = null;

		private bool isFinished = false;
		private bool turnChanged = false;

		private Statistics WhitePlayerStatistics = null;
		private Statistics BlackPlayerStatistics = null;

		private Statistics TurnPlayerStatistics = null;

		public TestSimulation()
		{
			simulator = new Simulator();
			simulator.OnBoardToBoardMove += Simulator_OnBoardToBoardMove;
			simulator.OnBoardToBarMove += Simulator_OnBoardToBarMove;
			simulator.OnBarToBoardMove += Simulator_OnBarToBoardMove;
			simulator.OnBearedOff += Simulator_OnBearedOff;
			simulator.OnTurnChanged += Simulation_OnTurnChanged;
			simulator.OnGameFinished += Simulation_OnGameFinished;

			//TDGammonBotUtilities.Configuration minimum = new TDGammonBotUtilities.Configuration(0.1F, 1, 1, 0.1F);
			//TDGammonBotUtilities.Configuration maximum = new TDGammonBotUtilities.Configuration(0.6F, 1, 1, 1);
			//TDGammonBotUtilities.Configuration conf = TDGammonBotUtilities.OptimumConfigurationFinder.Find(minimum, maximum, minimum, 10);
		}

		public void Run(int Seed)
		{
			TDGammonBotUtilities.Configuration conf = TDGammonBotUtilities.DEFAULT_CONFIGURATION;

			System.Console.WriteLine("Seed: {0}", Seed);

			simulator.Reset(Seed);

			serializer = new SessionSerializer();
			serializer.SerializeConfigState(simulator.Config);
			serializer.SerializeInitialState(simulator.Frame);

			random = new Random(Seed);

			isFinished = false;
			turnChanged = true;

#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnTurnChanged {0} {1}", simulator.Frame.Board.TurnColor, simulator.Frame.Board.TurnNumber);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif

			WhitePlayerStatistics = new Statistics();
			BlackPlayerStatistics = new Statistics();
			TurnPlayerStatistics = (simulator.Frame.Board.TurnColor == PlayerColors.White ? WhitePlayerStatistics : BlackPlayerStatistics);

			while (!isFinished)
			{
				if (!turnChanged)
					continue;

				turnChanged = false;

				BoardData board = simulator.Frame.Board;
				PlayerColors color = board.TurnColor;
				PlayerData player = (color == PlayerColors.White ? board.WhitePlayer : board.BlackPlayer);

				if (color == PlayerColors.White)
					RandomBotUtilities.PlayOneTurn(simulator, random, player);
				else
					TDGammonBotUtilities.PlayOneTurn(conf, simulator, player);

				if (!isFinished)
					SendEvent(new FinishTurnEvent(color));
			}

			PrintStatistics(PlayerColors.White, WhitePlayerStatistics);
			PrintStatistics(PlayerColors.Black, BlackPlayerStatistics);
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

			if (Utilities.FindPoint(simulator.Frame.Board, From).CheckerCount == 1)
				TurnPlayerStatistics.BlotCount++;
			if (Utilities.FindPoint(simulator.Frame.Board, To).CheckerCount == 1)
				TurnPlayerStatistics.BlotCount++;
		}

		private void Simulator_OnBoardToBarMove(Identifier From)
		{
#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnBoardToBarMove from {0}", From);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif

			TurnPlayerStatistics.HitCount++;
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

		private void Simulation_OnTurnChanged(PlayerColors Color)
		{
			turnChanged = true;

#if PRINT_ALL_STEPS
			System.Console.WriteLine();
			System.Console.WriteLine("OnTurnChanged {0} {1}", Color, simulator.Frame.Board.TurnNumber);

			Utilities.PrintBoard(simulator.Frame.Board);
#endif

			TurnPlayerStatistics = (Color == PlayerColors.White ? BlackPlayerStatistics : WhitePlayerStatistics);
		}

		private void Simulation_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("{0} is winner with {1} score(s)", WinnerColor, Score);

			Utilities.PrintBoard(simulator.Frame.Board);

			isFinished = true;
		}

		private static void PrintStatistics(PlayerColors Color, Statistics Statistics)
		{
			System.Console.WriteLine("{0} Player Statistics:", Color);
			System.Console.WriteLine("Blot: {0} Hit: {1}", Statistics.BlotCount, Statistics.HitCount);
		}
	}
}