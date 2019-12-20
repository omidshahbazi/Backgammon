using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class SmartBotUtilities
	{
		private static SimulationLogic logic = new SimulationLogic();
		private static SerializerVisitor Serializer = new SerializerVisitor();

		private static int[][] DICES_COMBINITIONS =
		{
			new int[] {1, 1},
			new int[] {1, 2},
			new int[] {1, 3},
			new int[] {1, 4},
			new int[] {1, 5},
			new int[] {1, 6},
			new int[] {2, 2},
			new int[] {2, 3},
			new int[] {2, 4},
			new int[] {2, 5},
			new int[] {2, 6},
			new int[] {3, 3},
			new int[] {3, 4},
			new int[] {3, 5},
			new int[] {3, 6},
			new int[] {4, 4},
			new int[] {4, 5},
			new int[] {4, 6},
			new int[] {5, 5},
			new int[] {4, 6}
		};

		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			while (Player.MoveCount != 0)
			{
				EventBase ev = null;

				MoveInfo[] moves = Logic.GetTotalPossibleBarToBoardMoves(board);
				if (moves.Length == 0)
				{
					moves = GetNonLockableMoves(board);

					float[] weights = new float[moves.Length];
					FilleWeightList(board, moves, weights);

					float maxWeight = MathUtilities.Max(weights);
					int moveIndex = System.Array.IndexOf(weights, maxWeight);

					MoveInfo move = moves[moveIndex];

					if (move.From != null && move.To != null)
						ev = new BoardToBoardMoveEvent(move.From.ID, move.To.ID);
					else
						ev = new BearOffEvent(move.From.ID);
				}
				else
				{
					MoveInfo move = moves[Random.Next(0, moves.Length)];
					ev = new BarToBoardMoveEvent(Player.Color, move.To.ID);
				}

				Simulator.SendEvent(ev);

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}
			}
		}

		//we  play each move and make an initial weight based on what happens, then iterate over all combinition of dices and change the weights for the prev move
		private static void FilleWeightList(BoardData Board, MoveInfo[] Moves, float[] Weights)
		{
			BoardData board = CloneBoard(Board);

			for (int i = 0; i < Moves.Length; ++i)
				Weights[i] = GetWeight(board, Moves[i]);
		}

		private static float GetWeight(BoardData Board, MoveInfo Move)
		{
			MutationList mutations = new MutationList();

			EventBase ev = GetEventByMoveInfo(Move);

			logic.Simulate(null, Board, new EventBase[] { ev }, mutations);

			float[] weights = new float[DICES_COMBINITIONS.Length];

			for (int i = 0; i < weights.Length; ++i)
			{

			}

			return CalculateWeightedAverage(weights);
		}

		private static MoveInfo[] GetNonLockableMoves(BoardData Board)
		{
			List<MoveInfo> moves = new List<MoveInfo>();

			moves.AddRange(Logic.GetTotalPossibleBoardToBoardMoves(Board));
			moves.AddRange(Logic.GetTotalPossibleBearedOffMoves(Board));

			return moves.ToArray();
		}

		private static BoardData CloneBoard(BoardData Board)
		{
			Serializer.Reset();

			Board.Visit(Serializer);

			return Deserializer.DeserializeBoardData(Serializer.Data);
		}

		private static float CalculateWeightedAverage(float[] Weights)
		{
			float weightedSum = 0;

			float coefficientSum = 0;

			for (uint i = 0; i < DICES_COMBINITIONS.Length; ++i)
			{
				int[] dices = DICES_COMBINITIONS[i];

				float multiplier = GetDicesProbability(dices[0], dices[1]);

				weightedSum += Weights[(int)i] * multiplier;

				coefficientSum += multiplier;
			}

			return weightedSum / coefficientSum;
		}

		private static float GetDicesProbability(int Dice1, int Dice2)
		{
			return 1.0F / (Dice1 == Dice2 ? (ConfigData.MAX_DICE_NUMBER * ConfigData.MAX_DICE_NUMBER) : (ConfigData.MAX_DICE_NUMBER * 2));
		}

		private static EventBase GetEventByMoveInfo(MoveInfo Move)
		{
			if (Move.From != null && Move.To != null)
				return new BoardToBoardMoveEvent(Move.From.ID, Move.To.ID);
			else if (Move.From != null)
				return new BearOffEvent(Move.From.ID);

			return new BarToBoardMoveEvent(Move.To.Color, Move.To.ID);
		}
	}
}
