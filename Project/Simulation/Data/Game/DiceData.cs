using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class DiceData : DataBase
	{
		public int Dice1;
		public int Dice2;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.VisitInt32(Dice1);
			Visitor.VisitInt32(Dice2);
		}
	}
}