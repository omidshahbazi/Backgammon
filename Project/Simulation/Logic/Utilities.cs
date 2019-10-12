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
			SimulationUtilities.GetBaseIndecies(BaseColor, out fromIndex, out toIndex);

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

		public static void InitializeBoard(ConfigData Config, BoardData Board)
		{
			Board.Points = new PointData[ConfigData.POINT_COUNT];
			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				Board.Points[i] = new PointData();

				InitializePoint(Board.Points[i], i);
			}

			Board.WhitePlayer = new PlayerData();
			InitializePlayer(Config, Board.WhitePlayer, PlayerColors.White);

			Board.BlackPlayer = new PlayerData();
			InitializePlayer(Config, Board.BlackPlayer, PlayerColors.Black);

			int whiteDice = Board.WhitePlayer.InitialDice.Moves[0];
			int blackDice = Board.BlackPlayer.InitialDice.Moves[1];
			if (whiteDice > blackDice)
				Board.TurnColor = PlayerColors.White;
			else if (whiteDice < blackDice)
				Board.TurnColor = PlayerColors.Black;
			else
			{
				if (Board.WhitePlayer.InitialDice.Moves[0] == 1)
					++Board.WhitePlayer.InitialDice.Moves[0];
				else
					--Board.WhitePlayer.InitialDice.Moves[0];

				Board.TurnColor = PlayerColors.Black;
			}

			Board.TurnDice = new DiceData();
			InitializeDice(Config, Board.TurnDice);

			(Board.TurnColor == PlayerColors.White ? Board.WhitePlayer : Board.BlackPlayer).MoveCount = Logic.GetTotalPossibleMoveCount(Board);
		}

		public static void InitializePoint(PointData Point, int Index)
		{
			Point.ID = new Identifier(Index);
			Point.Index = Index;
			Point.CheckerCount = ConfigData.POINT_CHECKER_COUNT[Index];
			Point.Color = ConfigData.POINT_COLOR[Index];
		}

		public static void InitializePlayer(ConfigData Config, PlayerData Player, PlayerColors Color)
		{
			Player.InitialDice = new DiceData();
			InitializeDice(Config, Player.InitialDice);

			Player.Color = Color;

			Player.BarCheckerCount = 0;
			Player.BearedOffCheckersCount = 0;

			Player.MoveCount = 0;
		}

		public static void InitializeDice(ConfigData Config, DiceData Dice)
		{
			SimulationUtilities.RandomDices(Config, Dice);
		}

		public static void PrintBoard(BoardData Board)
		{
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

			for (int i = eachSidePointCount; i < Board.Points.Length; ++i)
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

			for (int i = eachSidePointCount; i < Board.Points.Length; ++i)
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

			Console.Write("--------------------------------------------------------------------------------------------------------------");

			Console.WriteLine();
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
