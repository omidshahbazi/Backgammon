using Simulation.Common;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public static class InitializeUtilities
	{
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

			SimulationUtilities.UpdateMoveCount(Board, Board.TurnColor == PlayerColors.White ? Board.WhitePlayer : Board.BlackPlayer);

			Board.TurnNumber = 1;
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
	}
}
