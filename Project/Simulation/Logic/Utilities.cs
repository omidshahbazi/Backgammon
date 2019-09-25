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

			Board.WhitePlayer = new PlayerData();
			InitializePlayer(Config, Board.WhitePlayer, PlayerColors.White);

			Board.BlackPlayer = new PlayerData();
			InitializePlayer(Config, Board.BlackPlayer, PlayerColors.Black);

			int whiteDice = Board.WhitePlayer.InitialDice.Dice1 + Board.WhitePlayer.InitialDice.Dice2;
			int blackDice = Board.BlackPlayer.InitialDice.Dice1 + Board.BlackPlayer.InitialDice.Dice2;
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

			InitializeDice(Config, Board.TurnDice);
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
			SimulationUtilities.MakeRandomDices(Config, Dice);
		}
	}
}
