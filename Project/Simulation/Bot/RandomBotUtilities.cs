using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;

using LogicWrapper = Simulation.Logic.Logic;

namespace Simulation.Bot
{
	public static class RandomBotUtilities
	{
		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			while (Player.MoveCount != 0)
			{
				PlayBarToBoard(Simulator, Random, Player, Serializer, FullStep);

				PlayBearOff(Simulator, Random, Player, Serializer, FullStep);

				PlayBoardToBoard(Simulator, Random, Serializer, FullStep);
			}
		}

		private static bool PlayBoardToBoard(Simulator Simulator, Random Random, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			MoveInfo[] moves = LogicWrapper.GetTotalPossibleBoardToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			MoveInfo move = moves[Random.Next(0, moves.Length)];

			Simulator.SendEvent(new BoardToBoardMoveEvent(move.From.ID, move.To.ID));

			if (Serializer != null)
			{
				if (FullStep)
					Serializer.SerializeFullStep(Simulator.Frame);
				else
					Serializer.SerializeStep(Simulator.Frame);
			}

			return true;
		}

		private static bool PlayBarToBoard(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = LogicWrapper.GetTotalPossibleBarToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			MoveInfo move = moves[Random.Next(0, moves.Length)];

			Simulator.SendEvent(new BarToBoardMoveEvent(board.TurnColor, move.To.ID));

			if (Serializer != null)
			{
				if (FullStep)
					Serializer.SerializeFullStep(Simulator.Frame);
				else
					Serializer.SerializeStep(Simulator.Frame);
			}

			return true;
		}

		private static bool PlayBearOff(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			if (Utilities.GetInBaseCheckerCount(board.Points, board.TurnColor) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return false;

			MoveInfo[] moves = LogicWrapper.GetTotalPossibleBearedOffMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			MoveInfo move = moves[Random.Next(0, moves.Length)];

			Simulator.SendEvent(new BearOffEvent(move.From.ID));

			if (Serializer != null)
			{
				if (FullStep)
					Serializer.SerializeFullStep(Simulator.Frame);
				else
					Serializer.SerializeStep(Simulator.Frame);
			}

			return true;
		}
	}
}
