using Simulation.Common;
using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public class BoardToBoardMoveEvent : EventBase
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

		public BoardToBoardMoveEvent(Identifier From, Identifier To)
		{
			this.From = From;
			this.To = To;
		}

		public override Types GetType()
		{
			return Types.BoardToBoardMove;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitIdentifier(From);
			Visitor.VisitIdentifier(To);
		}
	}
}
