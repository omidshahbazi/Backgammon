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
		public class Configuration
		{
			public float MoveWeight
			{
				get;
				private set;
			}

			public float HitWeight
			{
				get;
				private set;
			}

			public float BearOffWeight
			{
				get;
				private set;
			}

			public float BlotWeight
			{
				get;
				private set;
			}

			public float HeuristicMultiplier
			{
				get;
				private set;
			}

			public Configuration(float MoveWeight, float HitWeight, float BearOffWeight, float BlotWeight, float HeuristicMultiplier)
			{
				if (MoveWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "MoveWeight");
				if (HitWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "HitWeight");
				if (MoveWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "MoveWeight");
				if (BlotWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "BlotWeight");
				if (HeuristicMultiplier == 0)
					throw new System.ArgumentException("Value cannot be zero", "HeuristicMultiplier");

				this.MoveWeight = MoveWeight;
				this.HitWeight = HitWeight;
				this.BearOffWeight = BearOffWeight;
				this.BlotWeight = BlotWeight;
				this.HeuristicMultiplier = HeuristicMultiplier;
			}
		}

		public static readonly Configuration DEFAULT_CONFIGURATION = new Configuration(1.0F, 1.3F, 1.3F, 0.1F, 1.0F);

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
			new int[] {6, 6}
		};

		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			PlayOneTurn(DEFAULT_CONFIGURATION, Simulator, Random, Player, Serializer, FullStep);
		}

		public static void PlayOneTurn(Configuration Configuration, Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
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
					FilleWeightList(Configuration, board, moves, weights);

					float maxWeight = MathUtilities.Max(weights);
					int moveIndex = System.Array.IndexOf(weights, maxWeight);

					ev = GetEventByMoveInfo(board.TurnColor, moves[moveIndex]);
				}
				else
				{
					ev = GetEventByMoveInfo(board.TurnColor, moves[Random.Next(0, moves.Length)]);
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

		private static void FilleWeightList(Configuration Configuration, BoardData Board, MoveInfo[] Moves, float[] Weights)
		{
			Serializer.Reset();

			Board.Visit(Serializer);

			for (int i = 0; i < Moves.Length; ++i)
			{
				BoardData board = Deserializer.DeserializeBoardData(Serializer.Data);

				Weights[i] = GetWeight(Configuration, board, Moves[i]);
			}
		}

		private static float GetWeight(Configuration Configuration, BoardData Board, MoveInfo Move)
		{
			float mutationsWeight = SimulateAndCalculateWeight(Configuration, Board, null, Move) + GetHeuristicValue(Configuration, Board.TurnColor, Move);

			float[] weights = new float[DICES_COMBINITIONS.Length];

			PlayerData player = Utilities.GetOpponentPlayer(Board, Board.TurnColor);
			Board.TurnColor = player.Color;

			for (int i = 0; i < weights.Length; ++i)
			{
				weights[i] = mutationsWeight;

				int[] dices = DICES_COMBINITIONS[i];

				SimulationUtilities.UpdateDice(Board.TurnDice, dices[0], dices[1]);

				SimulationUtilities.UpdateMoveCount(Board, player);

				MoveInfo[] moves = GetNonLockableMoves(Board);

				for (int j = 0; j < moves.Length; ++j)
				{
					float weight = SimulateAndCalculateWeight(Configuration, Board, Move, moves[j]);

					weights[i] *= (weight == 0 ? 1 : 1 / weight);
				}
			}

			return CalculateWeightedAverage(weights);
		}

		private static float SimulateAndCalculateWeight(Configuration Configuration, BoardData Board, MoveInfo ReferenceMove, MoveInfo Move)
		{
			MutationList mutations = new MutationList();

			EventBase ev = GetEventByMoveInfo(Board.TurnColor, Move);

			logic.Simulate(null, Board, new EventBase[] { ev }, mutations);

			float weight = 1;

			for (int i = 0; i < mutations.Count; ++i)
			{
				MutationBase mutation = mutations[i];

				if (mutation.GetType() == MutationBase.Types.BoardToBarMove)
				{
					BoardToBarMoveMutation boardToBarMoveMutation = (BoardToBarMoveMutation)mutation;

					if (ReferenceMove == null ||
						(ReferenceMove.From != null && boardToBarMoveMutation.From == ReferenceMove.From.ID) ||
						(ReferenceMove.To != null && boardToBarMoveMutation.From == ReferenceMove.To.ID))
						weight *= Configuration.HitWeight;
				}
				else if (mutation.GetType() == MutationBase.Types.BoardToBoardMove)
				{
					weight *= Configuration.MoveWeight;

					BoardToBoardMoveMutation boardToBoardMoveMutation = (BoardToBoardMoveMutation)mutation;

					if (Utilities.FindPoint(Board, boardToBoardMoveMutation.From).CheckerCount == 1)
						weight *= Configuration.BlotWeight;

					if (Utilities.FindPoint(Board, boardToBoardMoveMutation.To).CheckerCount == 1)
						weight *= Configuration.BlotWeight;
				}
				else if (mutation.GetType() == MutationBase.Types.BearedOff)
				{
					if (ReferenceMove != null)
						continue;

					weight *= Configuration.BearOffWeight;
				}
			}

			return weight;
		}

		private static float GetHeuristicValue(Configuration Configuration, PlayerColors Color, MoveInfo Move)
		{
			int fromIndex;
			int toIndex;
			Utilities.GetBaseIndecies(Color, out fromIndex, out toIndex);

			int targetIndex = fromIndex;

			if (Utilities.GetDirection(Color) < 0)
				targetIndex = toIndex;

			if (Move.To == null)
				return Configuration.HeuristicMultiplier;

			if (fromIndex <= Move.To.Index && Move.To.Index <= toIndex)
				return Configuration.HeuristicMultiplier;

			return (1 + (System.Math.Abs(targetIndex - Move.To.Index) / (float)ConfigData.POINT_COUNT)) * Configuration.HeuristicMultiplier;
		}

		private static MoveInfo[] GetNonLockableMoves(BoardData Board)
		{
			List<MoveInfo> moves = new List<MoveInfo>();

			moves.AddRange(Logic.GetTotalPossibleBoardToBoardMoves(Board));
			moves.AddRange(Logic.GetTotalPossibleBearedOffMoves(Board));

			return moves.ToArray();
		}

		private static float CalculateWeightedAverage(float[] Weights)
		{
			float weightedSum = 0;
			float multiplierSum = 0;

			for (uint i = 0; i < DICES_COMBINITIONS.Length; ++i)
			{
				int[] dices = DICES_COMBINITIONS[i];

				float multiplier = GetDicesProbability(dices[0], dices[1]);

				weightedSum += Weights[(int)i] * multiplier;

				multiplierSum += multiplier;
			}

			return weightedSum / multiplierSum;
		}

		private static float GetDicesProbability(int Dice1, int Dice2)
		{
			return 1.0F / (Dice1 == Dice2 ? (ConfigData.MAX_DICE_NUMBER * ConfigData.MAX_DICE_NUMBER) : (ConfigData.MAX_DICE_NUMBER * 2));
		}

		private static EventBase GetEventByMoveInfo(PlayerColors Color, MoveInfo Move)
		{
			if (Move.From != null && Move.To != null)
				return new BoardToBoardMoveEvent(Move.From.ID, Move.To.ID);
			else if (Move.From != null)
				return new BearOffEvent(Move.From.ID);

			return new BarToBoardMoveEvent(Color, Move.To.ID);
		}
	}
}
