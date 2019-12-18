using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;

namespace Simulation.Logic
{
	public static class DummyBotUtilities
	{
		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			while (Player.MoveCount != 0)
			{
				PlayBarToBoard(Simulator, Random, Player, Serializer, FullStep);

				PlayBearOff(Simulator, Player, Serializer, FullStep);

				PlayBoardToBoard(Simulator, Random, Serializer, FullStep);
			}
		}

		public static bool PlayBoardToBoard(Simulator Simulator, Random Random, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[Random.Next(0, moves.Length)].To.ID));

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}

				return true;
			}

			return false;
		}

		public static bool PlayBarToBoard(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			Simulator.SendEvent(new BarToBoardMoveEvent(board.TurnColor, moves[Random.Next(0, moves.Length)].To.ID));

			if (Serializer != null)
			{
				if (FullStep)
					Serializer.SerializeFullStep(Simulator.Frame);
				else
					Serializer.SerializeStep(Simulator.Frame);
			}

			return true;
		}

		public static bool PlayBearOff(Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
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

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}

				return true;
			}

			return false;
		}
	}
}
