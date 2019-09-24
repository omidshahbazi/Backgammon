using Simulation.Common;
using System.Collections.Generic;

namespace Simulation.Data.Mutation
{
	public abstract class MutationBase
	{
		public enum Types
		{
		}

		public Identifier Sender
		{
			get;
			private set;
		}

		public MutationBase(Identifier Sender)
		{
			this.Sender = Sender;
		}

		public abstract new Types GetType();
	}

	public class MutationList : List<MutationBase>
	{ }
}
