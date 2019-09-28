using Simulation.Common;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	static class SimulationUtilities
	{
		public static void RandomDices(ConfigData Config, DiceData Dice)
		{
			Dice.Dice1 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			Dice.Dice2 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
		}

		public static PlayerData GetPlayer(BoardData Board, PlayerColors Color)
		{
			return (Color == PlayerColors.White ? Board.WhitePlayer : Board.BlackPlayer);
		}

		public static PlayerData GetOpponentPlayer(BoardData Board, PlayerColors Color)
		{
			return (Color == PlayerColors.White ? Board.BlackPlayer : Board.WhitePlayer);
		}

		public static int GetStartIndex(PlayerColors Color)
		{
			return (Color == PlayerColors.White ? 0 : ConfigData.POINT_COUNT - 1);
		}

		public static void GetBase(PlayerColors Color, out int FromIndex, out int ToIndex)
		{
			if (Color == PlayerColors.White)
			{
				FromIndex = 0;
				ToIndex = 5;
			}
			else
			{
				int lastIndex = ConfigData.POINT_COUNT - 1;
				FromIndex = lastIndex - 5;
				ToIndex = lastIndex;
			}
		}

		public static int GetMoveCount(DiceData Dice)
		{
			return (Dice.Dice1 == Dice.Dice2 ? 4 : 2);
		}
	}
}
