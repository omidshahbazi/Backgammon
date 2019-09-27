using Simulation.Common;
using Simulation.Data.Game;

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

		public static void InitializeBoard(ConfigData Config, BoardData Board)
		{
			Board.Points = new PointData[ConfigData.POINT_COUNT];
			for (int i =0; i < ConfigData.POINT_COUNT; ++i)
			{
				Board.Points[i] = new PointData();

				InitializePoint(Board.Points[i], i);
			}

			Board.WhitePlayer = new PlayerData();
			InitializePlayer(Config, Board.WhitePlayer, PlayerColors.White);

			Board.BlackPlayer = new PlayerData();
			InitializePlayer(Config, Board.BlackPlayer, PlayerColors.Black);

			int whiteDice = Board.WhitePlayer.InitialDice.Dice1;
			int blackDice = Board.BlackPlayer.InitialDice.Dice1;
			if (whiteDice > blackDice)
				Board.TurnColor = PlayerColors.White;
			else if (whiteDice < blackDice)
				Board.TurnColor = PlayerColors.Black;
			else
			{
				if (Board.WhitePlayer.InitialDice.Dice1 == 1)
					++Board.WhitePlayer.InitialDice.Dice1;
				else
					--Board.WhitePlayer.InitialDice.Dice1;

				Board.TurnColor = PlayerColors.Black;
			}

			Board.TurnDice = new DiceData();
			InitializeDice(Config, Board.TurnDice);
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
		}

		public static void InitializeDice(ConfigData Config, DiceData Dice)
		{
			SimulationUtilities.RandomDices(Config, Dice);
		}
	}
}
