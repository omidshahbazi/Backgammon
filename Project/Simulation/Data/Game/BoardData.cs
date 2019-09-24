using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class BoardData : DataBase
	{
		public PointData[] Points;
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
		}
	}
}