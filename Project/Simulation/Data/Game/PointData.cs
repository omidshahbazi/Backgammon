using Simulation.Common;
using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class PointData : DataBase
	{
        public Identifier ID;
        public int CheckerCount;
		public PlayerColors Color;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitInt32(CheckerCount);
			Visitor.VisitInt32((int)Color);
            Visitor.VisitIdentifier(ID);
		}
	}
}