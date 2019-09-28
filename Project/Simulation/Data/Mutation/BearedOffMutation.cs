using Simulation.Common;

namespace Simulation.Data.Mutation
{
	public class BearedOffMutation : MutationBase
	{
		public Identifier From
		{
			get;
			private set;
		}

		public BearedOffMutation(Identifier From)
		{
			this.From = From;
		}

		public override Types GetType()
		{
			return Types.BearedOff;
		}
	}
}
