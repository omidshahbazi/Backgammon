using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class PlayerData : DataBase
	{
		public DiceData InitialDice;

		public PlayerColors Color;

		public int BarCheckerCount;
		public int BearedOffCheckersCount;

		public int MoveCount;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			InitialDice.Visit(Visitor);

			Visitor.VisitInt32((int)Color);

			Visitor.VisitInt32(BarCheckerCount);
			Visitor.VisitInt32(BearedOffCheckersCount);

			Visitor.VisitInt32(MoveCount);
		}
	}
}