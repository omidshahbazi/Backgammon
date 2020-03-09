using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using Simulation.Logic;

using LogicWrapper = Simulation.Logic.Logic;

namespace Simulation.Bot
{
	public static class WeightBasedBot
	{
		public class Configuration
		{
			public float BlotWeight
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

			public float BaseDistance
			{
				get;
				private set;
			}

			public Configuration(float BlotWeight, float HitWeight, float BearOffWeight, float BaseDistance)
			{
				if (BlotWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "BlotWeight");
				if (HitWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "HitWeight");
				if (BearOffWeight == 0)
					throw new System.ArgumentException("Value cannot be zero", "HitWeight");
				if (BaseDistance == 0)
					throw new System.ArgumentException("Value cannot be zero", "BaseDistance");

				this.BlotWeight = BlotWeight;
				this.HitWeight = HitWeight;
				this.BearOffWeight = BearOffWeight;
				this.BaseDistance = BaseDistance;
			}
		}

		public static readonly Configuration EXPERT_CONFIGURATION = new Configuration(0.2F, 8.0F, 2.0F, 2.0F);
		public static readonly Configuration HARD_CONFIGURATION = new Configuration(0.4F, 4.0F, 1.5F, 1.2F);
		public static readonly Configuration MEDIUM_CONFIGURATION = new Configuration(0.6F, 2.0F, 0.9F, 0.6F);
		public static readonly Configuration EASY_CONFIGURATION = new Configuration(0.8F, 1.0F, 0.1F, 0.2F);

		//public static readonly Configuration EXPERT_CONFIGURATION = new Configuration(0.1F, 4.0F, 1.2F, 2.0F);
		//public static readonly Configuration HARD_CONFIGURATION = new Configuration(0.5F, 2.0F, 1.2F, 1.2F);
		//public static readonly Configuration MEDIUM_CONFIGURATION = new Configuration(0.8F, 1.2F, 1.2F, 0.5F);
		//public static readonly Configuration EASY_CONFIGURATION = new Configuration(1.1F, 0.5F, 1.2F, 0.1F);

		private static SerializerVisitor serializer = new SerializerVisitor();

		public static void PlayOneTurn(Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			PlayOneTurn(EXPERT_CONFIGURATION, Simulator, Player, Serializer, FullStep);
		}

		public static void PlayOneTurn(Configuration Configuration, Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			while (Player.MoveCount != 0)
			{
				EventBase ev = null;

				MoveInfo[] moves = LogicWrapper.GetTotalPossibleBarToBoardMoves(board);
				if (moves.Length == 0)
				{
					moves = BotUtilities.GetNonLockableMoves(board);

					float[] weights = new float[moves.Length];
					FilleWeightList(Configuration, board, moves, weights);

					float maxWeight = MathHelper.Max(weights);
					int moveIndex = System.Array.IndexOf(weights, maxWeight);

					ev = BotUtilities.GetEventByMoveInfo(board.TurnColor, moves[moveIndex]);
				}
				else
				{
					ev = BotUtilities.GetEventByMoveInfo(board.TurnColor, moves[0]);
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
			serializer.Reset();

			Board.Visit(serializer);

			for (int i = 0; i < Moves.Length; ++i)
			{
				BoardData board = Deserializer.DeserializeBoardData(serializer.Data);

				Weights[i] = GetWeight(Configuration, board, Moves[i]);
			}
		}

		private static float GetWeight(Configuration Configuration, BoardData Board, MoveInfo Move)
		{
			float mutationsWeight = SimulateAndCalculateSelfMoveWeight(Configuration, Board, Move);
			mutationsWeight += GetHeuristicValue(Configuration, Board.TurnColor, Move);

			float[] weights = new float[Constants.DICES_COMBINITIONS.Length];

			PlayerData player = Utilities.GetOpponentPlayer(Board, Board.TurnColor);

			SimulationUtilities.ToggleTurnColor(Board);

			for (int i = 0; i < weights.Length; ++i)
			{
				weights[i] = mutationsWeight;

				int[] dices = Constants.DICES_COMBINITIONS[i];

				SimulationUtilities.UpdateDice(Board.TurnDice, dices[0], dices[1]);
				SimulationUtilities.UpdateMoveCount(Board, player);

				MoveInfo[] moves = BotUtilities.GetNonLockableMoves(Board);

				for (int j = 0; j < moves.Length; ++j)
				{
					float weight = SimulateAndCalculateOpponentMoveWeight(Configuration, Board, Move, moves[j]);

					weights[i] *= 1 / weight;
				}
			}

			return CalculateWeightedAverage(weights);
		}

		private static float SimulateAndCalculateSelfMoveWeight(Configuration Configuration, BoardData Board, MoveInfo Move)
		{
			MutationList mutations = new MutationList();

			BotUtilities.Simulate(Board, Move, mutations);

			float weight = 1;

			for (int i = 0; i < mutations.Count; ++i)
			{
				MutationBase mutation = mutations[i];

				if (mutation.GetType() == MutationBase.Types.BoardToBarMove)
				{
					weight *= Configuration.HitWeight;
				}
				else if (mutation.GetType() == MutationBase.Types.BoardToBoardMove)
				{
					BoardToBoardMoveMutation boardToBoardMoveMutation = (BoardToBoardMoveMutation)mutation;

					PointData fromPoint = Utilities.FindPoint(Board, boardToBoardMoveMutation.From);
					if (fromPoint.CheckerCount == 0)
						continue;

					if (fromPoint.CheckerCount == 1 && BotUtilities.IsThereAnyOpponentCheckerAhead(Board, fromPoint))
						weight *= Configuration.BlotWeight;

					PointData toPoint = Utilities.FindPoint(Board, boardToBoardMoveMutation.To);
					if (toPoint.CheckerCount == 1 && BotUtilities.IsThereAnyOpponentCheckerAhead(Board, toPoint))
						weight *= Configuration.BlotWeight;
				}
				else if (mutation.GetType() == MutationBase.Types.BearedOff)
				{
					weight *= Configuration.BearOffWeight;
				}
			}

			return weight;
		}

		private static float SimulateAndCalculateOpponentMoveWeight(Configuration Configuration, BoardData Board, MoveInfo ReferenceMove, MoveInfo Move)
		{
			MutationList mutations = new MutationList();

			BotUtilities.Simulate(Board, Move, mutations);

			float weight = 1;

			for (int i = 0; i < mutations.Count; ++i)
			{
				MutationBase mutation = mutations[i];

				if (mutation.GetType() == MutationBase.Types.BoardToBarMove)
				{
					BoardToBarMoveMutation boardToBarMoveMutation = (BoardToBarMoveMutation)mutation;

					if ((ReferenceMove.From != null && boardToBarMoveMutation.From == ReferenceMove.From.ID) ||
						(ReferenceMove.To != null && boardToBarMoveMutation.From == ReferenceMove.To.ID))
						weight *= Configuration.HitWeight;
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
				return 0;

			if (fromIndex <= Move.To.Index && Move.To.Index <= toIndex)
				return 0;

			return (1 + (System.Math.Abs(targetIndex - Move.To.Index) / (float)ConfigData.POINT_COUNT)) * Configuration.BaseDistance;
		}

		private static float CalculateWeightedAverage(float[] Weights)
		{
			float weightedSum = 0;
			float multiplierSum = 0;

			for (uint i = 0; i < Constants.DICES_COMBINITIONS.Length; ++i)
			{
				int[] dices = Constants.DICES_COMBINITIONS[i];

				float multiplier = BotUtilities.GetDicesProbability(dices[0], dices[1]);

				weightedSum += Weights[(int)i] * multiplier;

				multiplierSum += multiplier;
			}

			return weightedSum / multiplierSum;
		}
	}
}
