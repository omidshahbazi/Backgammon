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
			BearedOff = 3,
			TurnChanged = 4,
			GameFinished = 5
		}

		public MutationBase()
		{
		}

		public abstract new Types GetType();
	}

	public class MutationList : List<MutationBase>
	{ }
}
