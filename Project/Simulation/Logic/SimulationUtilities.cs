using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	static class SimulationUtilities
	{
		public static void MakeRandomDices(ConfigData Config, DiceData Dice, MutationList Mutations)
		{
			MakeRandomDices(Config, Dice);

			Mutations.Add(new DiceChangedMutation(1, Dice.Dice1));
			Mutations.Add(new DiceChangedMutation(2, Dice.Dice2));
		}

		public static void MakeRandomDices(ConfigData Config, DiceData Dice)
		{
			Dice.Dice1 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			Dice.Dice2 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
		}
	}
}
