using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public static class BotUtilities
	{
		public static void PlayOneTurn(Simulator Simulator, Random Random, BoardData Board, PlayerData Player)
		{
			while (Player.MoveCount != 0)
			{
				HandleBarToBoard(Simulator, Random, Board, Player);

				HandleBearOff(Simulator, Board, Player);

				HandleBoardToBoard(Simulator, Random, Board, Player);
			}
		}

		public static bool HandleBoardToBoard(Simulator Simulator, Random Random, BoardData Board, PlayerData Player)
		{
			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = Board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(Board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[Random.Next(0, moves.Length)].To.ID));

				return true;
			}

			return false;
		}

		public static bool HandleBarToBoard(Simulator Simulator, Random Random, BoardData Board, PlayerData Player)
		{
			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(Board);

			if (moves == null || moves.Length == 0)
				return false;

			Simulator.SendEvent(new BarToBoardMoveEvent(Board.TurnColor, moves[Random.Next(0, moves.Length)].To.ID));

			return true;
		}

		public static bool HandleBearOff(Simulator Simulator, BoardData Board, PlayerData Player)
		{
			if (Utilities.GetInBaseCheckerCount(Board.Points, Board.TurnColor) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return false;

			for (int i = 0; i < ConfigData.POINT_COUNT && Player.MoveCount != 0; ++i)
			{
				PointData fromPoint = Board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBearedOffs(Board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BearOffEvent(fromPoint.ID));

				return true;
			}

			return false;
		}
	}
}
