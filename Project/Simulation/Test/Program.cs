using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			testSimulation.Run(100);
			testSimulation.Run(13515610);
			testSimulation.Run(1121200);

			Console.ReadLine();
		}
	}
}