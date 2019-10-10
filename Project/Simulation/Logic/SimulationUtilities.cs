using Simulation.Data.Game;
using System;

namespace Simulation.Logic
{
	static class SimulationUtilities
	{
		public static void RandomDices(ConfigData Config, DiceData Dice)
		{
			Dice.Dice1 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			Dice.Dice2 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			Dice.AreSame = (Dice.Dice1 == Dice.Dice2);
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

		public static int GetOutIndex(PlayerColors Color)
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
			return (Dice.AreSame ? 4 : 2);
		}

		public static bool IsDoubleMove(DiceData Dice, int FromIndex, int ToIndex)
		{
			int movement = Math.Abs(ToIndex - FromIndex);

			return (Dice.AreSame && movement >= Dice.Dice1 * 2) || (movement >= Dice.Dice1 + Dice.Dice2);
		}

		public static void ApplyMoveCount(PlayerData Player, DiceData Dice, int FromIndex, int ToIndex)
		{
			Player.MoveCount -= (IsDoubleMove(Dice, FromIndex, ToIndex) ? 2 : 1);
		}
	}
}
