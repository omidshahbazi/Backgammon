using Simulation.Common;
using Simulation.Data.Game;
using System;

namespace Simulation.Logic
{
	public static class Utilities
	{
		public static PointData FindPoint(BoardData Board, Identifier Identifier)
		{
			return FindPoint(Board.Points, Identifier);
		}

		public static PointData FindPoint(PointData[] Points, Identifier Identifier)
		{
			for (int i = 0; i < Points.Length; ++i)
			{
				PointData point = Points[i];

				if (point.ID != Identifier)
					continue;

				return point;
			}

			return null;
		}

		public static MoveInfo FindInFromPoint(MoveInfo[] Moves, Identifier Identifier)
		{
			for (int i = 0; i < Moves.Length; ++i)
			{
				MoveInfo info = Moves[i];

				if (info.From.ID != Identifier)
					continue;

				return info;
			}

			return null;
		}

		public static MoveInfo FindInToPoint(MoveInfo[] Moves, Identifier Identifier)
		{
			for (int i = 0; i < Moves.Length; ++i)
			{
				MoveInfo info = Moves[i];

				if (info.To.ID != Identifier)
					continue;

				return info;
			}

			return null;
		}

		public static PlayerData GetPlayer(BoardData Board, PlayerColors Color)
		{
			return (Color == PlayerColors.White ? Board.WhitePlayer : Board.BlackPlayer);
		}

		public static PlayerData GetOpponentPlayer(BoardData Board, PlayerColors Color)
		{
			return (Color == PlayerColors.White ? Board.BlackPlayer : Board.WhitePlayer);
		}

		public static int GetDirection(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? ConfigData.WHITE_CHECKER_MOVE_DIRECTION : ConfigData.BLACK_CHECKER_MOVE_DIRECTION);
		}

		public static int GetStartIndex(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? 0 : ConfigData.POINT_COUNT - 1);
		}

		public static int GetEndIndex(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? ConfigData.POINT_COUNT - 1 : 0);
		}

		public static int GetBarIndex(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? -1 : ConfigData.POINT_COUNT);
		}

		public static int GetBearOffIndex(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? ConfigData.POINT_COUNT : -1);
		}

		public static void GetBaseIndecies(PlayerColors Color, out int FromIndex, out int ToIndex)
		{
			if (Color == PlayerColors.White)
			{
				int lastIndex = ConfigData.POINT_COUNT - 1;
				FromIndex = lastIndex - 5;
				ToIndex = lastIndex;
			}
			else
			{
				FromIndex = 0;
				ToIndex = 5;
			}
		}

		public static int GetMoveCount(DiceData Dice)
		{
			return (Dice.IsPair ? 4 : 2);
		}

		public static int GetMoveCount(DiceData Dice, int Movement)
		{
			if (Dice.IsPair)
				return (int)Math.Ceiling((float)Movement / Dice.Moves[0]);

			if (Dice.Moves.Length > 1)
			{
				if (Movement == Dice.Moves[0] + Dice.Moves[1])
					return 2;
			}

			return 1;
		}

		public static int GetOutOfBaseCheckerCount(PointData[] Points, PlayerColors Color)
		{
			return (ConfigData.PLAYER_CHECKER_COUNT - GetInBaseCheckerCount(Points, Color));
		}

		public static int GetInBaseCheckerCount(PointData[] Points, PlayerColors Color)
		{
			return GetInBaseCheckerCount(Points, Color, Color);
		}

		public static int GetInBaseOpponentCheckerCount(PointData[] Points, PlayerColors Color)
		{
			return GetInBaseCheckerCount(Points, Color, (Color == PlayerColors.White ? PlayerColors.Black : PlayerColors.White));
		}

		private static int GetInBaseCheckerCount(PointData[] Points, PlayerColors BaseColor, PlayerColors CheckerColor)
		{
			int fromIndex;
			int toIndex;
			GetBaseIndecies(BaseColor, out fromIndex, out toIndex);

			int count = 0;

			for (int i = 0; i < Points.Length; ++i)
			{
				PointData point = Points[i];

				if (point.Color != CheckerColor)
					continue;

				if (point.Index < fromIndex || toIndex < point.Index)
					continue;

				count += point.CheckerCount;
			}

			return count;
		}

		public static bool IsMovePossible(DiceData Dice, int Movement, bool IsBearOff, out int Index)
		{
			return IsMovePossible(Dice.Moves, Movement, IsBearOff, out Index);
		}

		public static bool IsMovePossible(int[] Moves, int Movement, bool IsBearOff, out int Index)
		{
			Index = -1;

			for (int i = 0; i < Moves.Length; ++i)
			{
				if (Moves[i] == Movement ||
					(Moves[i] >= Movement && IsBearOff))
				{
					Index = i;
					return true;
				}
			}

			int sum = 0;
			for (int i = 0; i < Moves.Length; ++i)
			{
				sum += Moves[i];

				if (sum == Movement ||
					(sum >= Movement && IsBearOff))
				{
					Index = i;
					return true;
				}

			}

			return false;
		}

		public static bool IsPointOpenToMoveFrom(PointData Point, PlayerColors Color)
		{
			return (Point.Color == Color && Point.CheckerCount != 0);
		}

		public static bool IsPointOpenToMoveTo(PointData Point, PlayerColors Color)
		{
			return (Point.Color == Color || Point.CheckerCount < 2);
		}

		public static bool IsBearOffPossible(BoardData Board, PointData FromPoint)
		{
			int fromIndex;
			int toIndex;
			int incDir = GetDirection(Board.TurnColor);

			if (incDir == 1)
			{
				GetBaseIndecies(Board.TurnColor, out fromIndex, out toIndex);

				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					int dice = Board.TurnDice.Moves[i];

					if (IsPointOpenToMoveFrom(Board.Points[toIndex - dice], Board.TurnColor))
						return true;
				}

				for (int i = fromIndex; i < FromPoint.Index; ++i)
				{
					PointData point = Board.Points[i];

					if (!IsPointOpenToMoveFrom(point, Board.TurnColor))
						continue;

					return false;
				}
			}
			else if (incDir == -1)
			{
				GetBaseIndecies(Board.TurnColor, out toIndex, out fromIndex);

				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					int dice = Board.TurnDice.Moves[i];

					if (IsPointOpenToMoveFrom(Board.Points[dice - 1], Board.TurnColor))
						return true;
				}

				for (int i = fromIndex; i > FromPoint.Index; --i)
				{
					PointData point = Board.Points[i];

					if (!IsPointOpenToMoveFrom(point, Board.TurnColor))
						continue;

					return false;
				}
			}

			return true;
		}

		public static void PrintBoard(BoardData Board)
		{
			Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");

			int eachSidePointCount = Board.Points.Length / 2;

			for (int i = 0; i < eachSidePointCount; ++i)
			{
				PointData point = Board.Points[i];

				Console.Write('P');
				Console.Write(i);
				Console.Write('\t');
			}

			Console.Write("Bar");
			Console.Write('\t');
			Console.Write("Out");

			if (Board.TurnColor == PlayerColors.Black)
			{
				Console.Write('\t');
				Console.Write("Dices");
			}

			Console.WriteLine();

			for (int i = 0; i < eachSidePointCount; ++i)
			{
				PointData point = Board.Points[i];

				Console.ForegroundColor = (point.Color == PlayerColors.White ? ConsoleColor.White : ConsoleColor.Red);

				Console.Write(point.CheckerCount);
				Console.Write('\t');
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(Board.BlackPlayer.BarCheckerCount);
			Console.Write('\t');
			Console.Write(Board.BlackPlayer.BearedOffCheckersCount);

			if (Board.TurnColor == PlayerColors.Black)
			{
				Console.Write('\t');
				PrintDice(Board.TurnDice);
			}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

			for (int i = Board.Points.Length - 1; i >= eachSidePointCount; --i)
			{
				PointData point = Board.Points[i];

				Console.ForegroundColor = (point.Color == PlayerColors.White ? ConsoleColor.White : ConsoleColor.Red);

				Console.Write(point.CheckerCount);
				Console.Write('\t');
			}
			Console.ForegroundColor = ConsoleColor.White;

			Console.Write(Board.WhitePlayer.BarCheckerCount);
			Console.Write('\t');
			Console.Write(Board.WhitePlayer.BearedOffCheckersCount);

			if (Board.TurnColor == PlayerColors.White)
			{
				Console.Write('\t');
				PrintDice(Board.TurnDice);
			}

			Console.WriteLine();

			for (int i = Board.Points.Length - 1; i >= eachSidePointCount; --i)
			{
				PointData point = Board.Points[i];

				Console.Write('P');
				Console.Write(i);
				Console.Write('\t');
			}

			Console.Write("Bar");
			Console.Write('\t');
			Console.Write("Out");

			if (Board.TurnColor == PlayerColors.White)
			{
				Console.Write('\t');
				Console.Write("Dices");
			}

			Console.WriteLine();

			Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
		}

		public static void PrintDice(DiceData Dice)
		{
			for (int i = 0; i < Dice.Moves.Length; ++i)
			{
				if (i != 0)
					Console.Write(',');

				Console.Write(Dice.Moves[i]);
			}
		}
	}
}
