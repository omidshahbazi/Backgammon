using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			TestSimulation testSimulation = new TestSimulation();

			testSimulation.Run(101);
			//testSimulation.Run(13515610); //has problem
			//testSimulation.Run(1121200);//has problem

			Console.ReadLine();
		}
	}
}