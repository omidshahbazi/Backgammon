using Simulation.Common;

namespace Simulation.Data.Game
{
	public class ConfigData
	{
		public const int POINT_COUNT = 24;

		public static readonly int[] POINT_CHECKER_COUNT = { 2, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 5, 5, 0, 0, 0, 3, 0, 5, 0, 0, 0, 0, 2 };
		public static readonly PlayerColors[] POINT_COLOR = { PlayerColors.White, 0, 0, 0, 0, PlayerColors.Black, 0, PlayerColors.Black, 0, 0, 0, PlayerColors.White, PlayerColors.Black, 0, 0, 0, PlayerColors.White, 0, PlayerColors.White, 0, 0, 0, 0, PlayerColors.Black };

		public const int MIN_DICE_NUMBER = 1;
		public const int MAX_DICE_NUMBER = 6;

		public const int WHITE_CHECKER_MOVE_DIRECTION = 1;
		public const int BLACK_CHECKER_MOVE_DIRECTION = -1;

		public int Seed;
		public Random Random = null;
	}
}