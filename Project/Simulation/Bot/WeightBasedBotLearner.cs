using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using System.Collections.Generic;

namespace Simulation.Bot
{
	public static class WeightBasedBotLearner
	{
		public static WeightBasedBot.Configuration Find(int SampleCount, float MinimumDesiredWeight, float MaximumDesiredWeight)
		{
			//WeightBasedBot.Configuration minimum = new WeightBasedBot.Configuration(0.1F, 0.1F, 0.1F, 0.1F);
			//WeightBasedBot.Configuration maximum = new WeightBasedBot.Configuration(1.0F, 1.0F, 1.0F, 1.0F);

			WeightBasedBot.Configuration maximum = new WeightBasedBot.Configuration(0.6F, 2, 1, 0.700000048F);

			return Find(maximum, maximum, maximum, SampleCount, MinimumDesiredWeight, MaximumDesiredWeight);
		}

		public static WeightBasedBot.Configuration Find(WeightBasedBot.Configuration Minimum, WeightBasedBot.Configuration Maximum, WeightBasedBot.Configuration Step, int SampleCount, float MinimumDesiredWeight, float MaximumDesiredWeight)
		{
			MinimumDesiredWeight = MathHelper.Clamp(MinimumDesiredWeight, 0, 1);
			MaximumDesiredWeight = MathHelper.Clamp(MaximumDesiredWeight, 0, 1);

			Random random = new Random(1);

			int[] seeds = new int[SampleCount];
			for (int i = 0; i < SampleCount; ++i)
				seeds[i] = random.Next(1, 999999999);

			WeightBasedBot.Configuration[] confVariations = GetVariations(Minimum, Maximum, Step);

			return Find(Minimum, confVariations, random, seeds, SampleCount, MinimumDesiredWeight, MaximumDesiredWeight);
		}

		private static WeightBasedBot.Configuration Find(WeightBasedBot.Configuration Base, WeightBasedBot.Configuration[] Variations, Random Random, int[] Seeds, int SampleCount, float MinimumDesiredWeight, float MaximumDesiredWeight)
		{
			float[] weights = new float[Variations.Length];

			int totalSimulationCount = Variations.Length * SampleCount;
			int simulatedCount = 0;
			double totalSimulatedTime = 0;

			Simulator simulator = new Simulator();
			for (int i = 0; i < Variations.Length; ++i)
			{
				WeightBasedBot.Configuration conf = Variations[i];

				double startTime = DateTimeHelper.Time;

				weights[i] = Simulate(Random, simulator, Seeds, Base, conf, SampleCount);

				totalSimulatedTime += (DateTimeHelper.Time - startTime);

				simulatedCount += SampleCount;

				double eachStepTime = totalSimulatedTime / (float)simulatedCount;

				System.Console.Clear();
				System.Console.WriteLine("Total simulation: {0} Simulated: {1} Speed: {2} (Per Sec) Remain: {3}s Percent: {4}%",
					totalSimulationCount,
					simulatedCount,
					1 / eachStepTime,
					(totalSimulationCount - simulatedCount) * eachStepTime,
					(simulatedCount / (float)totalSimulationCount) * 100);
			}

			List<WeightBasedBot.Configuration> inDesiredRangesConf = new List<WeightBasedBot.Configuration>();
			for (int i = 0; i < weights.Length; ++i)
			{
				float weight = weights[i];

				if (weight < MinimumDesiredWeight || MaximumDesiredWeight < weight)
					continue;

				inDesiredRangesConf.Add(Variations[i]);
			}

			if (inDesiredRangesConf.Count == 0)
			{
				System.Console.WriteLine("Couldn't find desired configuration");
				return null;
			}

			if (inDesiredRangesConf.Count == Variations.Length)
				return Variations[Variations.Length - 1];

			return Find(inDesiredRangesConf[0], inDesiredRangesConf.ToArray(), Random, Seeds, SampleCount, MinimumDesiredWeight, MaximumDesiredWeight);
		}

		private static float Simulate(Random Random, Simulator Simulator, int[] Seeds, WeightBasedBot.Configuration Base, WeightBasedBot.Configuration Configuration, int SampleCount)
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
						WeightBasedBot.PlayOneTurn(Base, Simulator, player);
					else
						WeightBasedBot.PlayOneTurn(Configuration, Simulator, player);

					if (player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
						break;

					Simulator.SendEvent(new FinishTurnEvent(color));
				}

				if (Simulator.Frame.Board.BlackPlayer.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
					samplesWeight[i] = 1;
			}

			return MathHelper.Average(samplesWeight);
		}

		private static WeightBasedBot.Configuration[] GetVariations(WeightBasedBot.Configuration Minimum, WeightBasedBot.Configuration Maximum, WeightBasedBot.Configuration Step)
		{
			List<WeightBasedBot.Configuration> variations = new List<WeightBasedBot.Configuration>();

			for (float blotWeight = Minimum.BlotWeight; blotWeight <= Maximum.BlotWeight; blotWeight += Step.BlotWeight)
				for (float hitWeight = Minimum.HitWeight; hitWeight <= Maximum.HitWeight; hitWeight += Step.HitWeight)
					for (float bearOffWeight = Minimum.BearOffWeight; bearOffWeight <= Maximum.BearOffWeight; bearOffWeight += Step.BearOffWeight)
						for (float baseDistance = Minimum.BaseDistance; baseDistance <= Maximum.BaseDistance; baseDistance += Step.BaseDistance)
							variations.Add(new WeightBasedBot.Configuration(blotWeight, hitWeight, bearOffWeight, baseDistance));

			return variations.ToArray();
		}
	}
}
