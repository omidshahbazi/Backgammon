using Simulation.Common;
using Simulation.Data.Game;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class Logic
	{
		private class PointDataList : List<PointData>
		{ }

		public static PointData[] GetPossibleBoardToBoardMoves(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null || fromPoint.CheckerCount == 0)
				return null;

			return GetPossibleMoves(Board, fromPoint.Color, fromPoint.Index);
		}

		public static PointData[] GetPossibleBarToBoardMoves(BoardData Board, PlayerColors Color)
		{
			return GetPossibleMoves(Board, Color, SimulationUtilities.GetStartIndex(Color));
		}

		private static PointData[] GetPossibleMoves(BoardData Board, PlayerColors Color, int StartIndex)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount == 0)
				return null;

			if (Color != Board.TurnColor)
				return null;

			PointDataList possiblePoints = new PointDataList();

			int iteration = (Board.TurnDice.Dice1 == Board.TurnDice.Dice2 ? 2 : 1);

			for (int i = 0; i < iteration; ++i)
			{
				int dice1 = Board.TurnDice.Dice1 * (i + 1);
				int dice2 = Board.TurnDice.Dice2 * (i + 1);

				bool isDice1Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice1, possiblePoints);
				bool isDice2Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice2, possiblePoints);

				if (isDice1Open || isDice2Open)
					GetPossibleMoves(Board.Points, Color, StartIndex, dice1 + dice2, possiblePoints);
			}

			return possiblePoints.ToArray();
		}

		private static bool GetPossibleMoves(PointData[] Points, PlayerColors Color, int Index, int Count, PointDataList PossiblePoints)
		{
			int targetPointIndex = Index + (Count * (Color == PlayerColors.White ? 1 : -1));

			if (targetPointIndex < 0 || Points.Length <= targetPointIndex)
				return false;

			PointData targetPoint = Points[targetPointIndex];

			if (targetPoint.CheckerCount != 0)
			{
				if (targetPoint.Color != Color)
				{
					if (targetPoint.CheckerCount > 1)
						return false;
				}
			}

			PossiblePoints.Add(targetPoint);
			return true;
		}
	}
}
