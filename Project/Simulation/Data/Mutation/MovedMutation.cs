using Simulation.Common;

namespace Simulation.Data.Mutation
{
	public class MovedMutation : MutationBase
	{
		public Identifier From
		{
			get;
			private set;
		}

		public Identifier To
		{
			get;
			private set;
		}

		public MovedMutation(Identifier From, Identifier To)
		{
			this.From = From;
			this.To = To;
		}

		public override Types GetType()
		{
			return Types.CheckerMoved;
		}
	}
}
