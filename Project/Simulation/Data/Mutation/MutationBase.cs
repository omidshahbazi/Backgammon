using System.Collections.Generic;

namespace Simulation.Data.Mutation
{
	public abstract class MutationBase
	{
		public enum Types
		{
			BoardToBoardMove = 0,
			BoardToBarMove = 1,
			BarToBoardMove = 2,
			DiceChanged = 3
		}

		public MutationBase()
		{
		}

		public abstract new Types GetType();
	}

	public class MutationList : List<MutationBase>
	{ }
}
