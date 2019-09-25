using Simulation.Common;
using Simulation.Data.Game;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class Logic
	{
		private class PointDataList : List<PointData>
		{ }

		public static PointData[] GetPossibleTargetPoints(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			if (fromPoint.Color != Board.TurnColor)
				return null;

			PointDataList possiblePoints = new PointDataList();

			GetPossibleTargetPoints(Board.Points, fromPoint, Board.Dice1, possiblePoints);
			GetPossibleTargetPoints(Board.Points, fromPoint, Board.Dice2, possiblePoints);
			GetPossibleTargetPoints(Board.Points, fromPoint, Board.Dice1 + Board.Dice2, possiblePoints);

			return possiblePoints.ToArray();
		}

		private static void GetPossibleTargetPoints(PointData[] Points, PointData FromPoint, int Count, PointDataList PossiblePoints)
		{
			int targetPointIndex = FromPoint.Index + Count;

			if (targetPointIndex >= Points.Length)
				return;

			PointData targetPoint = Points[targetPointIndex];

			if (targetPoint.CheckerCount != 0)
			{
				if (targetPoint.Color != FromPoint.Color)
				{
					if (targetPoint.CheckerCount > 1)
						return;
				}
			}

			PossiblePoints.Add(targetPoint);
		}
	}
}
