using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System;
using System.Collections.Generic;

using LogicWrapper = Simulation.Logic.Logic;

namespace Simulation.Bot
{
	//https://towardsdatascience.com/create-ai-for-your-own-board-game-from-scratch-minimax-part-2-517e1c1e3362
	//https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/ -> 5
	public static class MinMaxBot
	{
		private class MoveInfoHolder
		{
			public MoveInfo[] Moves;
			public int Value;
		}

		private enum ValueTypes
		{
			Min,
			Max,
		}

		public const int MAX_DEPTH = 2;

		private static SerializerVisitor serializer = new SerializerVisitor();

		public static void PlayOneTurn(Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			PlayOneTurn(MAX_DEPTH, Simulator, Player, Serializer, FullStep);
		}

		public static void PlayOneTurn(int MaxDepth, Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			while (Player.MoveCount != 0)
			{
				MoveInfo[] moves = LogicWrapper.GetTotalPossibleBarToBoardMoves(board);
				if (moves.Length == 0)
				{
					MoveInfo[] optimalMoves = CalculateMinMax(board, MaxDepth);

					for (int j = 0; j < optimalMoves.Length; ++j)
						Simulator.SendEvent(BotUtilities.GetEventByMoveInfo(board.TurnColor, optimalMoves[j]));
				}
				else
					Simulator.SendEvent(BotUtilities.GetEventByMoveInfo(board.TurnColor, moves[0]));

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}
			}
		}

		private static MoveInfo[] CalculateMinMax(BoardData OriginalBoard, int MaxDepth)
		{
			serializer.Reset();
			OriginalBoard.Visit(serializer);

			MoveInfoHolder[] moveHolders = GetMoveInfoHolders(OriginalBoard);

			MutationList mutations = new MutationList();
			for (int i = 0; i < moveHolders.Length; ++i)
			{
				BoardData board = Deserializer.DeserializeBoardData(serializer.Data);

				MoveInfoHolder holder = moveHolders[i];

				mutations.Clear();

				for (int j = 0; j < holder.Moves.Length; ++j)
					BotUtilities.Simulate(board, holder.Moves[j], mutations);

				holder.Value += EvaluateBoardState(board, ValueTypes.Max);

				holder.Value += CalculateMinMax(board, ValueTypes.Min, MaxDepth - 1);
			}

			MoveInfoHolder appropriateHolder = FindAppropriateMoveInfoHolder(moveHolders, ValueTypes.Max);

			return appropriateHolder.Moves;
		}

		private static int CalculateMinMax(BoardData Board, ValueTypes ValueType, int MaxDepth)
		{
			if (MaxDepth < 1)
				return 0;

			PlayerData opponentPlayer = Utilities.GetOpponentPlayer(Board, Board.TurnColor);
			ValueTypes nextValueType = (ValueType == ValueTypes.Max ? ValueTypes.Min : ValueTypes.Max);

			SimulationUtilities.ToggleTurnColor(Board);

			int sum = 0;
			int count = 0;
			for (int i = 0; i < Constants.DICES_COMBINITIONS.Length; ++i)
			{
				int[] dices = Constants.DICES_COMBINITIONS[i];

				SimulationUtilities.UpdateDice(Board.TurnDice, dices[0], dices[1]);
				SimulationUtilities.UpdateMoveCount(Board, opponentPlayer);

				MoveInfoHolder[] moveHolders = GetMoveInfoHolders(Board);

				MutationList mutations = new MutationList();
				for (int j = 0; j < moveHolders.Length; ++j)
				{
					BoardData board = Deserializer.DeserializeBoardData(serializer.Data);

					MoveInfoHolder holder = moveHolders[j];

					mutations.Clear();

					for (int k = 0; k < holder.Moves.Length; ++k)
						BotUtilities.Simulate(board, holder.Moves[k], mutations);

					holder.Value = EvaluateBoardState(board, ValueType);

					holder.Value += CalculateMinMax(board, nextValueType, MaxDepth - 1);
				}

				MoveInfoHolder appropriateHolder = FindAppropriateMoveInfoHolder(moveHolders, ValueType);
				if (appropriateHolder != null)
				{
					sum += appropriateHolder.Value;
					++count;
				}
			}

			return (sum / count);
		}

		private static MoveInfoHolder FindAppropriateMoveInfoHolder(MoveInfoHolder[] MoveInfoHolders, ValueTypes ValueType)
		{
			if (MoveInfoHolders.Length == 0)
				return null;

			MoveInfoHolder appropriateHolder = MoveInfoHolders[0];

			for (int j = 1; j < MoveInfoHolders.Length; ++j)
			{
				MoveInfoHolder holder = MoveInfoHolders[j];

				if (ValueType == ValueTypes.Max)
				{
					if (holder.Value > appropriateHolder.Value)
						appropriateHolder = holder;
				}
				else if (ValueType == ValueTypes.Min)
				{
					if (holder.Value < appropriateHolder.Value)
						appropriateHolder = holder;
				}
			}

			return appropriateHolder;
		}

		private static MoveInfoHolder[] GetMoveInfoHolders(BoardData Board)
		{
			DiceData dice = Board.TurnDice;

			List<MoveInfoHolder> moveHolders = new List<MoveInfoHolder>();

			if (dice.IsPair)
			{
				MoveInfo[] moves = BotUtilities.GetNonLockableMoves(Board, dice.Moves[0]);

				if (moves.Length < 4)
					moveHolders.Add(new MoveInfoHolder() { Moves = moves });
				else
				{
					for (int i = 0; i < moves.Length; ++i)
						for (int j = i + 1; j < moves.Length; ++j)
							for (int k = j + 1; k < moves.Length; ++k)
								for (int l = k + 1; l < moves.Length; ++l)
									moveHolders.Add(new MoveInfoHolder() { Moves = new MoveInfo[4] { moves[i], moves[j], moves[k], moves[l] } });
				}
			}
			else
			{
				MoveInfo[] moves1 = BotUtilities.GetNonLockableMoves(Board, dice.Moves[0]);

				if (dice.Moves.Length < 2)
				{
					for (int i = 0; i < moves1.Length; ++i)
						moveHolders.Add(new MoveInfoHolder() { Moves = new MoveInfo[1] { moves1[i] } });
				}
				else
				{
					MoveInfo[] moves2 = BotUtilities.GetNonLockableMoves(Board, dice.Moves[1]);

					if (moves1.Length == 0)
					{
						for (int i = 0; i < moves2.Length; ++i)
							moveHolders.Add(new MoveInfoHolder() { Moves = new MoveInfo[1] { moves2[i] } });
					}
					else if (moves2.Length == 0)
					{
						for (int i = 0; i < moves1.Length; ++i)
							moveHolders.Add(new MoveInfoHolder() { Moves = new MoveInfo[1] { moves1[i] } });
					}
					else
					{
						for (int i = 0; i < moves1.Length; ++i)
							for (int j = 0; j < moves2.Length; ++j)
								moveHolders.Add(new MoveInfoHolder() { Moves = new MoveInfo[2] { moves1[i], moves2[j] } });
					}
				}
			}

			return moveHolders.ToArray();
		}

		private static int EvaluateBoardState(BoardData Board, ValueTypes ValueType)
		{
			return (CalculateBarCheckersValue(Board, ValueType) + CalculateBlotValue(Board, ValueType) + CalculateDistancesValue(Board, ValueType));
		}

		private static int CalculateBlotValue(BoardData Board, ValueTypes ValueType)
		{
			const int VALUE = 1000;

			int value = 0;

			if (Board.WhitePlayer.BarCheckerCount == 0 && Board.BlackPlayer.BarCheckerCount == 0)
			{
				bool flag2 = false;

				PlayerData player = null;

				for (int i = 0; i < Board.Points.Length; ++i)
				{
					PointData point = Board.Points[i];

					if (point.CheckerCount != 0)
					{
						if (player == null)
						{
							player = Utilities.GetPlayer(Board, point.Color);
						}
						else if (!flag2)
						{
							if (Utilities.GetPlayer(Board, point.Color) != player)
								flag2 = true;
						}
						else if (Utilities.GetPlayer(Board, point.Color) == player)
						{
							value = VALUE;
							break;
						}
					}
				}
			}
			else
				value = VALUE;

			if (value == 0)
				return 0;

			int sum = 0;

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				if (point.CheckerCount == 1)
					sum += Utilities.GetDirection(point.Color) * value;
			}

			return sum;
		}

		private static int CalculateBarCheckersValue(BoardData Board, ValueTypes ValueType)
		{
			int value = 0;

			value += CalculatePlayerBarCheckersValue(Board, Board.WhitePlayer, ValueType);

			value += CalculatePlayerBarCheckersValue(Board, Board.BlackPlayer, ValueType);

			return value;
		}

		private static int CalculatePlayerBarCheckersValue(BoardData Board, PlayerData Player, ValueTypes ValueType)
		{
			const int START_NUMBER = 3000;

			int value = 0;

			if (Player.BarCheckerCount != 0)
			{
				int sum = 0;

				for (int i = 0; i < Board.Points.Length; ++i)
				{
					PointData point = Board.Points[i];

					if (point.Color == Player.Color)
						sum += (i + 1) * point.CheckerCount;
				}

				value += (ValueType == ValueTypes.Min ? -1 : 1) * (START_NUMBER + sum) * Board.WhitePlayer.BarCheckerCount;
			}

			return value;
		}

		private static int CalculateDistancesValue(BoardData Board, ValueTypes ValueType)
		{
			int distance = 0;

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				if (point.CheckerCount == 0)
					continue;

				int dir = Utilities.GetDirection(point.Color);

				if (dir == 1)
					distance += (Board.Points.Length - i) * point.CheckerCount;
				else
					distance += (i + 1) * point.CheckerCount;

				distance *= (ValueType == ValueTypes.Min ? -1 : 1);
			}

			return distance + ((Board.WhitePlayer.BearedOffCheckersCount - Board.BlackPlayer.BearedOffCheckersCount) * Board.Points.Length);
		}
	}
}
