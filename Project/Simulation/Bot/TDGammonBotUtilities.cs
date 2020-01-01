using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System.Collections.Generic;

using LogicWrapper = Simulation.Logic.Logic;

namespace Simulation.Bot
{
	public static class TDGammonBotUtilities
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

		public static class OptimumConfigurationFinder
		{
			public static Configuration Find(int SampleCount, float Aggressive, float Safety)
			{
				TDGammonBotUtilities.Configuration minimum = new TDGammonBotUtilities.Configuration(0.1F, 0.1F, 1.0F, 1.0F);
				TDGammonBotUtilities.Configuration maximum = new TDGammonBotUtilities.Configuration(1.0F, 1.0F, 1.0F, 1.0F);

				return Find(minimum, maximum, minimum, SampleCount, Aggressive, Safety);
			}

			public static Configuration Find(Configuration Minimum, Configuration Maximum, Configuration Step, int SampleCount, float MinimumDesiredWeight, float MaximumDesiredWeight)
			{
				MinimumDesiredWeight = MathHelper.Clamp(MinimumDesiredWeight, 0, 1);
				MaximumDesiredWeight = MathHelper.Clamp(MaximumDesiredWeight, 0, 1);

				Random random = new Random(0);

				int[] seeds = new int[SampleCount];
				for (int i = 0; i < SampleCount; ++i)
					seeds[i] = random.Next(1, 999999999);

				Configuration[] confVariations = GetVariations(Minimum, Maximum, Step);
				float[] weights = new float[confVariations.Length];

				int totalSimulationCount = confVariations.Length * SampleCount;
				int simulatedCount = 0;
				double totalSimulatedTime = 0;

				Simulator simulator = new Simulator();
				for (int i = 0; i < confVariations.Length; ++i)
				{
					Configuration conf = confVariations[i];

					double startTime = DateTimeHelper.Time;

					weights[i] = Simulate(random, simulator, seeds, conf, SampleCount);

					totalSimulatedTime += (DateTimeHelper.Time - startTime);

					simulatedCount += SampleCount;

					System.Console.Clear();
					System.Console.WriteLine("Total simulation: {0} Simulated: {1} Speed: {2} (Per Sec) Percent: {3}%", totalSimulationCount, simulatedCount, 1 / (totalSimulatedTime / (float)simulatedCount), (simulatedCount / (float)totalSimulationCount) * 100);
				}

				List<float> inDesiredRange = new List<float>();
				for (int i = 0; i < weights.Length; ++i)
				{
					float weight = weights[i];

					if (weight < MinimumDesiredWeight || MaximumDesiredWeight < weight)
						continue;

					inDesiredRange.Add(weight);
				}

				float max = MathHelper.Max(weights);
				int index = System.Array.IndexOf(weights, max);

				if (index == -1)
				{
					System.Console.WriteLine("Couldn't find desired configuration");
					return null;
				}

				TDGammonBotUtilities.Configuration selectedCong = confVariations[index];

				System.Console.WriteLine("BlotWeight: {0} HitWeight: {1} BearOffWeight: {2} BaseDistance: {3}", selectedCong.BlotWeight, selectedCong.HitWeight, selectedCong.BearOffWeight, selectedCong.BaseDistance);

				return selectedCong;
			}

			private static float Simulate(Random Random, Simulator Simulator, int[] Seeds, TDGammonBotUtilities.Configuration Configuration, int SampleCount)
			{
				float[] samplesWeight = new float[SampleCount];

				for (int i = 0; i < SampleCount; ++i)
				{
					Simulator.Reset(Seeds[i]);

					while (true)
					{
						BoardData board = Simulator.Frame.Board;
						PlayerColors color = board.TurnColor;
						PlayerData player = (color == PlayerColors.White ? board.WhitePlayer : board.BlackPlayer);

						if (color == PlayerColors.White)
							RandomBotUtilities.PlayOneTurn(Simulator, Random, player);
						else
							TDGammonBotUtilities.PlayOneTurn(Configuration, Simulator, player);

						if (player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
							break;

						Simulator.SendEvent(new FinishTurnEvent(color));
					}

					if (Simulator.Frame.Board.BlackPlayer.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
						samplesWeight[i] = 1;
				}

				return MathHelper.Average(samplesWeight);
			}

			private static Configuration[] GetVariations(Configuration Minimum, Configuration Maximum, Configuration Step)
			{
				List<Configuration> variations = new List<Configuration>();

				for (float blotWeight = Minimum.BlotWeight; blotWeight <= Maximum.BlotWeight; blotWeight += Step.BlotWeight)
					for (float hitWeight = Minimum.HitWeight; hitWeight <= Maximum.HitWeight; hitWeight += Step.HitWeight)
						for (float bearOffWeight = Minimum.BearOffWeight; bearOffWeight <= Maximum.BearOffWeight; bearOffWeight += Step.BearOffWeight)
							for (float baseDistance = Minimum.BaseDistance; baseDistance <= Maximum.BaseDistance; baseDistance += Step.BaseDistance)
								variations.Add(new Configuration(blotWeight, hitWeight, bearOffWeight, baseDistance));

				return variations.ToArray();
			}
		}

		//public static readonly Configuration DEFAULT_CONFIGURATION = new Configuration(0.6F, 2, 1, 0.700000048F);
		public static readonly Configuration DEFAULT_CONFIGURATION = new Configuration(0.1F, 1, 1, 0.3F);

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

		public static void PlayOneTurn(Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			PlayOneTurn(DEFAULT_CONFIGURATION, Simulator, Player, Serializer, FullStep);
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
					moves = GetNonLockableMoves(board);

					float[] weights = new float[moves.Length];
					FilleWeightList(Configuration, board, moves, weights);

					float maxWeight = MathHelper.Max(weights);
					int moveIndex = System.Array.IndexOf(weights, maxWeight);

					ev = GetEventByMoveInfo(board.TurnColor, moves[moveIndex]);
				}
				else
				{
					ev = GetEventByMoveInfo(board.TurnColor, moves[0]);
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
			float mutationsWeight = SimulateAndCalculateSelfMoveWeight(Configuration, Board, Move);
			mutationsWeight *= GetHeuristicValue(Configuration, Board.TurnColor, Move);

			float[] weights = new float[DICES_COMBINITIONS.Length];

			PlayerData player = Utilities.GetOpponentPlayer(Board, Board.TurnColor);

			SimulationUtilities.ToggleTurnColor(Board);

			for (int i = 0; i < weights.Length; ++i)
			{
				weights[i] = mutationsWeight;

				int[] dices = DICES_COMBINITIONS[i];

				SimulationUtilities.UpdateDice(Board.TurnDice, dices[0], dices[1]);
				SimulationUtilities.UpdateMoveCount(Board, player);

				MoveInfo[] moves = GetNonLockableMoves(Board);

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

			Simulate(Board, Move, mutations);

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

					int fromCheckerCount = Utilities.FindPoint(Board, boardToBoardMoveMutation.From).CheckerCount;

					if (fromCheckerCount == 0)
						continue;

					if (fromCheckerCount == 1)
						weight *= Configuration.BlotWeight;

					if (Utilities.FindPoint(Board, boardToBoardMoveMutation.To).CheckerCount == 1)
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

			Simulate(Board, Move, mutations);

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

		private static void Simulate(BoardData Board, MoveInfo Move, MutationList Mutations)
		{
			EventBase ev = GetEventByMoveInfo(Board.TurnColor, Move);

			logic.Simulate(null, Board, new EventBase[] { ev }, Mutations);
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
				return Configuration.BaseDistance;

			if (fromIndex <= Move.To.Index && Move.To.Index <= toIndex)
				return Configuration.BaseDistance;

			return (1 + (System.Math.Abs(targetIndex - Move.To.Index) / (float)ConfigData.POINT_COUNT)) * Configuration.BaseDistance;
		}

		private static MoveInfo[] GetNonLockableMoves(BoardData Board)
		{
			List<MoveInfo> moves = new List<MoveInfo>();

			moves.AddRange(LogicWrapper.GetTotalPossibleBoardToBoardMoves(Board));
			moves.AddRange(LogicWrapper.GetTotalPossibleBearedOffMoves(Board));

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
