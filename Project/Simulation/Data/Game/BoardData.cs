using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class BoardData : DataBase
	{
		public PointData[] Points;
		public PlayerColors TurnColor;
		public int Dice1;
		public int Dice2;
		public int OnBarWhiteCheckerCount;
		public int OnBarBlackCheckerCount;
		public int BearedOffWhiteCheckersCount;
		public int BearedOffBlackCheckersCount;

		public override void Visit(IVisitor Visitor)
		{
			base.Visit(Visitor);

			Visitor.BeginVisitArray(Points);
			if (Points != null)
				for (int i =0;i < Points.Length; ++i)
				{
					Visitor.BeginVisitArrayElement();

					Points[i].Visit(Visitor);

					Visitor.EndVisitArrayElement();
				}
			Visitor.EndVisitArray();

			Visitor.VisitInt32((int)TurnColor);
			Visitor.VisitInt32(Dice1);
			Visitor.VisitInt32(Dice2);

			Visitor.VisitInt32(OnBarWhiteCheckerCount);
			Visitor.VisitInt32(OnBarBlackCheckerCount);
			Visitor.VisitInt32(BearedOffWhiteCheckersCount);
			Visitor.VisitInt32(BearedOffBlackCheckersCount);
		}
	}
}