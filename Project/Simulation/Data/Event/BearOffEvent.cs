using Simulation.Common;
using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public class BearOffEvent : EventBase
	{
		public Identifier From
		{
			get;
			private set;
		}

		public BearOffEvent(Identifier From)
		{
			this.From = From;
		}

		public override Types GetType()
		{
			return Types.BearOff;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitIdentifier(From);
		}
	}
}