using Simulation.Common;
using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public class BearedOffEvent : EventBase
	{
		public Identifier From
		{
			get;
			private set;
		}

		public BearedOffEvent(Identifier From)
		{
			this.From = From;
		}

		public override Types GetType()
		{
			return Types.BarToBoardMove;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitIdentifier(From);
		}
	}
}