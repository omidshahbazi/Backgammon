using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class BoardData : DataBase
	{
		class PlayerData????

		public PointData[] Points;

		public int WhiteInitialDice1;
		public int WhiteInitialDice2;

		public int BlackInitialDice1;
		public int BlackInitialDice2;

		public PlayerColors TurnColor;
		public int TurnDice1;
		public int TurnDice2;

		public int WhiteBarCheckerCount;
		public int BlackBarCheckerCount;
		public int BearedOffWhiteCheckersCount;
		public int BearedOffBlackCheckersCount;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.BeginVisitArray(Points);
			if (Points != null)
				for (int i = 0; i < Points.Length; ++i)
				{
					Visitor.BeginVisitArrayElement();

					Points[i].Visit(Visitor);

					Visitor.EndVisitArrayElement();
				}
			Visitor.EndVisitArray();

			Visitor.VisitInt32(WhiteInitialDice1);
			Visitor.VisitInt32(WhiteInitialDice2);

			Visitor.VisitInt32(BlackInitialDice1);
			Visitor.VisitInt32(BlackInitialDice2);

			Visitor.VisitInt32((int)TurnColor);
			Visitor.VisitInt32(TurnDice1);
			Visitor.VisitInt32(TurnDice2);

			Visitor.VisitInt32(WhiteBarCheckerCount);
			Visitor.VisitInt32(BlackBarCheckerCount);
			Visitor.VisitInt32(BearedOffWhiteCheckersCount);
			Visitor.VisitInt32(BearedOffBlackCheckersCount);
		}
	}
}