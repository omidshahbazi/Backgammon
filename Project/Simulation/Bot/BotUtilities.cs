using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Logic;
using System.Collections.Generic;

using LogicWrapper = Simulation.Logic.Logic;

namespace Simulation.Bot
{
	static class BotUtilities
	{
		private static SimulationLogic logic = new SimulationLogic();

		public static float GetDicesProbability(int Dice1, int Dice2)
		{
			return 1.0F / (Dice1 == Dice2 ? (ConfigData.MAX_DICE_NUMBER * ConfigData.MAX_DICE_NUMBER) : (ConfigData.MAX_DICE_NUMBER * 2));
		}

		public static EventBase GetEventByMoveInfo(PlayerColors Color, MoveInfo Move)
		{
			if (Move.From != null && Move.To != null)
				return new BoardToBoardMoveEvent(Move.From.ID, Move.To.ID);
			else if (Move.From != null)
				return new BearOffEvent(Move.From.ID);

			return new BarToBoardMoveEvent(Color, Move.To.ID);
		}

		public static MoveInfo[] GetNonLockableMoves(BoardData Board)
		{
			List<MoveInfo> moves = new List<MoveInfo>();

			moves.AddRange(LogicWrapper.GetTotalPossibleBoardToBoardMoves(Board));
			moves.AddRange(LogicWrapper.GetTotalPossibleBearedOffMoves(Board));

			return moves.ToArray();
		}

		public static MoveInfo[] GetNonLockableMoves(BoardData Board, int Dice)
		{
			List<MoveInfo> moves = new List<MoveInfo>();

			moves.AddRange(LogicWrapper.GetPossibleBoardToBoardMoves(Board, Dice));
			moves.AddRange(LogicWrapper.GetPossibleBearedOffMoves(Board, Dice));

			return moves.ToArray();
		}

		public static bool IsThereAnyOpponentCheckerAhead(BoardData Board, PointData Point)
		{
			int dir = Utilities.GetDirection(Point.Color);
			int endIndex = Utilities.GetEndIndex(Point.Color);

			if (dir == 1)
			{
				for (int i = Point.Index + 1; i <= endIndex; ++i)
					if (!Utilities.IsPointOpenToMoveTo(Board.Points[i], Point.Color))
						return true;
			}
			else
			{
				for (int i = endIndex; i < Point.Index; ++i)
					if (!Utilities.IsPointOpenToMoveTo(Board.Points[i], Point.Color))
						return true;
			}

			return false;
		}

		public static void Simulate(BoardData Board, MoveInfo Move, MutationList Mutations)
		{
			EventBase ev = GetEventByMoveInfo(Board.TurnColor, Move);

			logic.Simulate(null, Board, new EventBase[] { ev }, Mutations);
		}
	}
}
