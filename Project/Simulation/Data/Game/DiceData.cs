using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class DiceData : DataBase
	{
		public int[] Moves;
		public bool IsPair;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.BeginVisitArray(Moves);
			if (Moves != null)
				for (int i = 0; i < Moves.Length; ++i)
				{
					Visitor.BeginVisitArrayElement();

					Visitor.VisitInt32(Moves[i]);

					Visitor.EndVisitArrayElement();
				}
			Visitor.EndVisitArray();

			Visitor.VisitBool(IsPair);
		}
	}
}