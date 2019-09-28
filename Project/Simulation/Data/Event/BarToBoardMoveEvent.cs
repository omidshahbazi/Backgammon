using Simulation.Common;
using Simulation.Common.Visitor;
using Simulation.Data.Game;

namespace Simulation.Data.Event
{
	public class BarToBoardMoveEvent : EventBase
	{
		public PlayerColors Color
		{
			get;
			private set;
		}

		public Identifier To
		{
			get;
			private set;
		}

		public BarToBoardMoveEvent(PlayerColors Color, Identifier To)
		{
			this.Color = Color;
			this.To = To;
		}

		public override Types GetType()
		{
			return Types.BearOff;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitInt32((int)Color);
			Visitor.VisitIdentifier(To);
		}
	}
}
