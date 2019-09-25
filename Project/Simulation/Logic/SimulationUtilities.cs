using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	static class SimulationUtilities
	{
		public static void MakeRandomDices(ConfigData Config, BoardData Board, MutationList Mutations)
		{
			Board.TurnDice1 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);
			Board.TurnDice2 = Config.Random.Next(ConfigData.MIN_DICE_NUMBER, ConfigData.MAX_DICE_NUMBER + 1);

			Mutations.Add(new DiceChangedMutation(1, Board.TurnDice1));
			Mutations.Add(new DiceChangedMutation(2, Board.TurnDice2));
		}
	}
}
