using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			//testSimulation.Run(100);
			//testSimulation.Run(13515610);
			//testSimulation.Run(1121200);

			Random random = new Random(1);

			while (true)
			{
				int seed = random.Next(1, 999999999);

				Console.WriteLine("Seed: {0}", seed);

				testSimulation.Run(seed);

				//Console.ReadLine();
			}
		}
	}
}