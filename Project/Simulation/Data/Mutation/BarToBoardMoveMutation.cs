using Simulation.Common;

namespace Simulation.Data.Mutation
{
	public class BarToBoardMoveMutation : MutationBase
	{
		public Identifier To
		{
			get;
			private set;
		}

		public BarToBoardMoveMutation(Identifier To)
		{
			this.To = To;
		}

		public override Types GetType()
		{
			return Types.BarToBoardMove;
		}
	}
}
