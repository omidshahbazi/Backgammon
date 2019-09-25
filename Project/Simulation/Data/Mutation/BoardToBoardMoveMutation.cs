using Simulation.Common;

namespace Simulation.Data.Mutation
{
	public class BoardToBoardMoveMutation : MutationBase
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

		public BoardToBoardMoveMutation(Identifier From, Identifier To)
		{
			this.From = From;
			this.To = To;
		}

		public override Types GetType()
		{
			return Types.BoardToBoardMove;
		}
	}
}
