using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class SmartBotUtilities
	{
		private static SerializerVisitor Serializer = new SerializerVisitor();

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

					//float maxQuality = MathUtilities.Max(moveQuality);
					//int moveIndex = moveQuality.IndexOf(maxQuality);
					//if (moveIndex == -1)
					//continue;
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

		private static void FilleWeightList(BoardData Board, MoveInfo[] Moves, float[] Weights)
		{
			BoardData board = CloneBoard(Board);


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

		//private static float WeightedAverage(List<float> Values)
		//{
		//	float weightedSum = 0;

		//	float coefficientSum = 0;

		//	for (uint i = 0; i < Values.Count; ++i)
		//	{
		//		float multiplier = GetDiceProbability(DICE_PAIRS[i][0], DICE_PAIRS[i][1]);

		//		weightedSum += Values[(int)i] * multiplier;

		//		coefficientSum += multiplier;
		//	}

		//	return weightedSum / coefficientSum;
		//}

		//private static float GetDiceProbability(int Dice1, int Dice2)
		//{
		//	return 1.0F / (Dice1 == Dice2 ? (ConfigData.MAX_DICE_NUMBER * ConfigData.MAX_DICE_NUMBER) : (ConfigData.MAX_DICE_NUMBER * 2));
		//}
	}
}
