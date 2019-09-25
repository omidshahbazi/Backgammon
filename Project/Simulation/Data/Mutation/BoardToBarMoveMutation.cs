using Simulation.Common;

namespace Simulation.Data.Mutation
{
	public class BoardToBarMoveMutation : MutationBase
	{
		public Identifier From
		{
			get;
			private set;
		}

		public BoardToBarMoveMutation(Identifier From)
		{
			this.From = From;
		}

		public override Types GetType()
		{
			return Types.BoardToBarMove;
		}
	}
}
