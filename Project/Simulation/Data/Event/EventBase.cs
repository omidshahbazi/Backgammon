using Simulation.Common.Visitor;

namespace Simulation.Data.Event
{
	public abstract class EventBase : IVisitee
	{
		public enum Types
		{
			BoardToBoardMove = 0,
			BearOff = 1,
			BearedOff = 2,
			FinishTurn = 3
		}

		public abstract new Types GetType();

		public virtual void Visit(IVisitor Visitor)
		{
			Visitor.VisitInt32((int)GetType());
		}
	}
}
