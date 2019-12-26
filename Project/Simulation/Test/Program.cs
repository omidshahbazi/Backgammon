using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			testSimulation.Run(248668584);
			Console.ReadLine();

			Random random = new Random(1);

			while (true)
			{
				int seed = random.Next(1, 999999999);

				testSimulation.Run(seed);

				Console.ReadLine();
			}
		}
	}
}