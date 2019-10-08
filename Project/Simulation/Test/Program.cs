using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			testSimulation.Run(100);

			Console.ReadLine();
		}
	}
}