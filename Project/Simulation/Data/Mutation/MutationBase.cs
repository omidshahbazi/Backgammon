using Simulation.Common;
using System.Collections.Generic;

namespace Simulation.Data.Mutation
{
	public abstract class MutationBase
	{
		public enum Types
		{
			CheckerMoved = 0,
			DiceChanged = 1
		}

		public MutationBase()
		{
		}

		public abstract new Types GetType();
	}

	public class MutationList : List<MutationBase>
	{ }
}
