using GameFramework.Common.Extensions;
using Simulation.Data.Game;
using System;

namespace Simulation.Logic
{
	static class SimulationUtilities
	{
		public static void RandomDices(ConfigData Config, DiceData Dice)
		{
			int dice1 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			int dice2 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);

			Dice.IsPair = (dice1 == dice2);

			if (Dice.IsPair)
			{
				Dice.Moves = new int[4];
				Dice.Moves[0] = dice1;
				Dice.Moves[1] = dice2;
				Dice.Moves[2] = dice1;
				Dice.Moves[3] = dice2;
			}
			else
			{
				Dice.Moves = new int[2];
				Dice.Moves[0] = dice1;
				Dice.Moves[1] = dice2;
			}
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

		public static bool ApplyMoveCount(PlayerData Player, DiceData Dice, int FromIndex, int ToIndex, bool IsBearOff)
		{
			if (Dice.Moves == null || Dice.Moves.Length == 0)
				return false;

			int movement = Math.Abs(ToIndex - FromIndex);
			//if (IsBearOff)
			//	--movement;

			int moveCount = GetMoveCount(Dice, movement);

			if (Player.MoveCount - moveCount < 0)
				return false;

			Player.MoveCount -= moveCount;

			return ConsumeDice(Dice, movement);
		}

		public static bool ConsumeDice(DiceData Dice, int Movement)
		{
			for (int i = 0; i < Dice.Moves.Length; ++i)
			{
				if (Dice.Moves[i] != Movement)
					continue;

				ArrayUtilities.RemoveAt(ref Dice.Moves, i);

				return true;
			}

			int sum = 0;
			for (int i = 0; i < Dice.Moves.Length; ++i)
			{
				sum += Dice.Moves[i];

				if (sum != Movement)
					continue;

				ArrayUtilities.RemoveRange(ref Dice.Moves, 0, (i + 1));

				return true;
			}

			return false;
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
	}
}
