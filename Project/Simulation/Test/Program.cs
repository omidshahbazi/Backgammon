using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Simulator simulation = new Simulator();
			simulation.OnBarToBoardMove += Simulation_OnBarToBoardMove;
			simulation.OnBoardToBarMove += Simulation_OnBoardToBarMove;
			simulation.OnBoardToBoardMove += Simulation_OnBoardToBoardMove;
			simulation.OnTurnChanged += Simulation_OnTurnChanged;
			simulation.Reset(103);

			PointData[] point = Logic.GetPossibleBoardToBoardMoves(simulation.Frame.Board, new Identifier(23));
			point = Logic.GetPossibleBarToBoardMoves(simulation.Frame.Board, PlayerColors.Black);

			simulation.SendEvent(new BoardToBoardMoveEvent(new Identifier(23), new Identifier(19)));
			//simulation.SendEvent(new BarToBoardMoveEvent(PlayerColors.Black, new Identifier(6)));
			simulation.SendEvent(new FinishTurnEvent(PlayerColors.Black));
			simulation.SendEvent(new FinishTurnEvent(PlayerColors.White));

			Console.ReadLine();
		}

		private static void Simulation_OnTurnChanged()
		{
			Console.WriteLine("Simulation_OnDiceChanged");
		}

		private static void Simulation_OnBoardToBoardMove(Identifier From, Identifier To)
		{
			Console.WriteLine("Simulation_OnBoardToBoardMove {0}, {1}", From, To);
		}

		private static void Simulation_OnBoardToBarMove(Identifier From)
		{
			Console.WriteLine("Simulation_OnBoardToBarMove {0}", From);
		}

		private static void Simulation_OnBarToBoardMove(Identifier To)
		{
			Console.WriteLine("Simulation_OnBarToBoardMove {0}", To);
		}
	}
}