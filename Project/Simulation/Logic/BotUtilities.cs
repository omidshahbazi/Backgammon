using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public static class BotUtilities
	{
		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, MutationList Mutations = null)
		{
			while (Player.MoveCount != 0)
			{
				PlayBarToBoard(Simulator, Random, Player, Mutations);

				PlayBearOff(Simulator, Player, Mutations);

				PlayBoardToBoard(Simulator, Random, Mutations);
			}
		}

		public static bool PlayBoardToBoard(Simulator Simulator, Random Random, MutationList Mutations = null)
		{
			BoardData board = Simulator.Frame.Board;

			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[Random.Next(0, moves.Length)].To.ID), Mutations);

				return true;
			}

			return false;
		}

		public static bool PlayBarToBoard(Simulator Simulator, Random Random, PlayerData Player, MutationList Mutations = null)
		{
			BoardData board = Simulator.Frame.Board;

			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			Simulator.SendEvent(new BarToBoardMoveEvent(board.TurnColor, moves[Random.Next(0, moves.Length)].To.ID), Mutations);

			return true;
		}

		public static bool PlayBearOff(Simulator Simulator, PlayerData Player, MutationList Mutations = null)
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

				Simulator.SendEvent(new BearOffEvent(fromPoint.ID), Mutations);

				return true;
			}

			return false;
		}
	}
}
