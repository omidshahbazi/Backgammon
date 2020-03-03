using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			//testSimulation.Run(248668584);
			//Console.ReadLine();

			Random random = new Random(1);

			//while (true)
			//{
			//	int seed = random.Next(1, 999999999);

			//	testSimulation.Run(seed);

			//	Console.ReadLine();
			//}

			const int GAME_COUNT = 50;
			int blackWinCount = 0;
			for (int i = 0; i < GAME_COUNT; ++i)
			{
				int seed = random.Next(1, 999999999);

				if (testSimulation.Run(seed) == Simulation.Data.Game.PlayerColors.Black)
					++blackWinCount;
			}

			Console.WriteLine("Black win rate: {0}%", (blackWinCount / (float)GAME_COUNT) * 100);
			Console.ReadLine();
		}
	}
}