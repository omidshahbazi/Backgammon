using Simulation.Common.Visitor;
using Simulation.Data.Game;

namespace Simulation.Data.Event
{
	public class FinishTurnEvent : EventBase
	{
		public PlayerColors Color
		{
			get;
			private set;
		}

		public FinishTurnEvent(PlayerColors Color)
		{
			this.Color = Color;
		}

		public override Types GetType()
		{
			return Types.FinishTurn;
		}

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitInt32((int)Color);
		}
	}
}
