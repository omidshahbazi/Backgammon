using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public static class BotUtilities
	{
		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player)
		{
			while (Player.MoveCount != 0)
			{
				PlayBarToBoard(Simulator, Random, Player);

				PlayBearOff(Simulator, Player);

				PlayBoardToBoard(Simulator, Random);
			}
		}

		public static bool PlayBoardToBoard(Simulator Simulator, Random Random)
		{
			BoardData board = Simulator.Frame.Board;

			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[Random.Next(0, moves.Length)].To.ID));

				return true;
			}

			return false;
		}

		public static bool PlayBarToBoard(Simulator Simulator, Random Random, PlayerData Player)
		{
			BoardData board = Simulator.Frame.Board;

			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			Simulator.SendEvent(new BarToBoardMoveEvent(board.TurnColor, moves[Random.Next(0, moves.Length)].To.ID));

			return true;
		}

		public static bool PlayBearOff(Simulator Simulator, PlayerData Player)
		{
			BoardData board = Simulator.Frame.Board;

			if (Utilities.GetInBaseCheckerCount(board.Points, board.TurnColor) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return false;

			for (int i = 0; i < ConfigData.POINT_COUNT && Player.MoveCount != 0; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBearedOffs(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BearOffEvent(fromPoint.ID));

				return true;
			}

			return false;
		}
	}
}
