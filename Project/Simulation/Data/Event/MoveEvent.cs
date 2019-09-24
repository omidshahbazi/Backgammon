using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public class MoveEvent : EventBase
	{
		public override Types GetType()
		{
			return Types.Move;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);


		}
	}
}
