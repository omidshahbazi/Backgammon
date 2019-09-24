using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public abstract class EventBase : IVisitee
	{
		public enum Types
		{
			Move = 0
		}

		public abstract new Types GetType();

		public virtual void Visit(IVisitor Visitor)
		{
			Visitor.VisitInt32((int)GetType());
		}
	}
}
