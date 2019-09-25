using Simulation.Common;
using Simulation.Data.Game;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class Logic
	{
		private class PointDataList : List<PointData>
		{ }

		public static PointData[] GetPossibleBoardToBoard(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			if (fromPoint.Color != Board.TurnColor)
				return null;

			if (fromPoint.Color == PlayerColors.White)
			{
				if (Board.BearedOffWhiteCheckersCount != 0)
					return null;
			}
			else if (fromPoint.Color == PlayerColors.Black)
			{
				if (Board.BearedOffBlackCheckersCount != 0)
					return null;
			}

			PointDataList possiblePoints = new PointDataList();

			GetPossibleBoardToBoard(Board.Points, fromPoint, Board.TurnDice1, possiblePoints);
			GetPossibleBoardToBoard(Board.Points, fromPoint, Board.TurnDice2, possiblePoints);
			GetPossibleBoardToBoard(Board.Points, fromPoint, Board.TurnDice1 + Board.TurnDice2, possiblePoints);

			return possiblePoints.ToArray();
		}

		private static void GetPossibleBoardToBoard(PointData[] Points, PointData FromPoint, int Count, PointDataList PossiblePoints)
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
