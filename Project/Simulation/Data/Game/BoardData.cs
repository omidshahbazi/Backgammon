using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public class BoardData : DataBase
	{
		public const int POINT_COUNT = 24;

		public PointData[] Points;

		public PlayerData WhitePlayer;
		public PlayerData BlackPlayer;

		public PlayerColors TurnColor;

		public DiceData TurnDice;

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

			WhitePlayer.Visit(Visitor);
			BlackPlayer.Visit(Visitor);

			Visitor.VisitInt32((int)TurnColor);

			TurnDice.Visit(Visitor);
		}
	}
}