using Simulation.Common;
using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class PointData : DataBase
	{
        public Identifier ID;
		public int Index;
        public int CheckerCount;
		public PlayerColors Color;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitIdentifier(ID);
			Visitor.VisitInt32(Index);
			Visitor.VisitInt32(CheckerCount);
			Visitor.VisitInt32((int)Color);
		}
	}

	public class MoveInfo
	{
		public PointData From;
		public PointData To;
	}
}