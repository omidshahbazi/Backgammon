using Simulation.Common;

namespace Simulation.Data.Game
{
	public class ConfigData
	{
		public const int MIN_DICE_NUMBER = 1;
		public const int MAX_DICE_NUMBER = 6;

		public int Seed;
		public Random Random = null;
	}
}
