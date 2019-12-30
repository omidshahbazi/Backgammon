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

			UpdateDice(Dice, dice1, dice2);
		}

		public static void UpdateDice(DiceData Dice, int Dice1, int Dice2)
		{
			Dice.IsPair = (Dice1 == Dice2);

			if (Dice.IsPair)
			{
				Dice.Moves = new int[4];
				Dice.Moves[0] = Dice1;
				Dice.Moves[1] = Dice1;
				Dice.Moves[2] = Dice1;
				Dice.Moves[3] = Dice1;
			}
			else
			{
				Dice.Moves = new int[2];
				Dice.Moves[0] = Dice1;
				Dice.Moves[1] = Dice2;
			}
		}

		public static bool ApplyMoveCount(BoardData Board, PlayerData Player, int FromIndex, int ToIndex, bool IsBearOff)
		{
			if (Board.TurnDice.Moves == null || Board.TurnDice.Moves.Length == 0)
				return false;

			int movement = Math.Abs(ToIndex - FromIndex);
			//if (IsBearOff)
			//	--movement;

			int moveCount = Utilities.GetMoveCount(Board.TurnDice, movement);

			if (Player.MoveCount - moveCount < 0)
				return false;

			bool result = ConsumeDice(Board.TurnDice, movement, IsBearOff);

			return result;
		}

		public static void ToggleTurnColor(BoardData Board)
		{
			Board.TurnColor = (Board.TurnColor == PlayerColors.White ? PlayerColors.Black : PlayerColors.White);
		}

		public static void UpdateMoveCount(BoardData Board, PlayerData Player)
		{
			Player.MoveCount = Logic.GetTotalAvailableMoveCount(Board);
		}

		public static bool ConsumeDice(DiceData Dice, int Movement, bool IsBearOff)
		{
			for (int i = 0; i < Dice.Moves.Length; ++i)
			{
				int dice = Dice.Moves[i];

				if (dice == Movement ||
					(IsBearOff && dice >= Movement))
				{
					ArrayUtilities.RemoveAt(ref Dice.Moves, i);

					return true;
				}
			}

			int sum = 0;
			for (int i = 0; i < Dice.Moves.Length; ++i)
			{
				sum += Dice.Moves[i];

				if (sum == Movement ||
					(IsBearOff && sum >= Movement))
				{
					ArrayUtilities.RemoveRange(ref Dice.Moves, 0, (i + 1));

					return true;
				}
			}

			return false;
		}
	}
}
