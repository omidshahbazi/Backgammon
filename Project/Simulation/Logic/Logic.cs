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
			if (fromPoint == null || fromPoint.CheckerCount == 0)
				return null;

			if (fromPoint.Color != Board.TurnColor)
				return null;

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);

			if (player.BearedOffCheckersCount != 0)
				return null;

			PointDataList possiblePoints = new PointDataList();

			int iteration = (Board.TurnDice.Dice1 == Board.TurnDice.Dice2 ? 2 : 1);

			for (int i = 0; i < iteration; ++i)
			{
				int dice1 = Board.TurnDice.Dice1 * (i + 1);
				int dice2 = Board.TurnDice.Dice2 * (i + 1);

				bool isDice1Open = GetPossibleMove(Board.Points, fromPoint.Color, fromPoint.Index, dice1, possiblePoints);
				bool isDice2Open = GetPossibleMove(Board.Points, fromPoint.Color, fromPoint.Index, dice2, possiblePoints);

				if (isDice1Open || isDice2Open)
					GetPossibleMove(Board.Points, fromPoint.Color, fromPoint.Index, dice1 + dice2, possiblePoints);
			}

			return possiblePoints.ToArray();
		}

		public static PointData[] GetPossibleBarToBoard(BoardData Board, PlayerColors Color)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount == 0)
				return null;

			if (Color != Board.TurnColor)
				return null;

			if (player.BearedOffCheckersCount != 0)
				return null;

			PointDataList possiblePoints = new PointDataList();

			int startIndex = SimulationUtilities.GetStartIndex(Color);
			int iteration = (Board.TurnDice.Dice1 == Board.TurnDice.Dice2 ? 2 : 1);

			for (int i = 0; i < iteration; ++i)
			{
				int dice1 = Board.TurnDice.Dice1 * (i + 1);
				int dice2 = Board.TurnDice.Dice2 * (i + 1);

				bool isDice1Open = GetPossibleMove(Board.Points, Color, startIndex, dice1, possiblePoints);
				bool isDice2Open = GetPossibleMove(Board.Points, Color, startIndex, dice2, possiblePoints);

				if (isDice1Open || isDice2Open)
					GetPossibleMove(Board.Points, Color, startIndex, dice1 + dice2, possiblePoints);
			}

			return possiblePoints.ToArray();
		}

		private static bool GetPossibleMove(PointData[] Points, PlayerColors Color, int Index, int Count, PointDataList PossiblePoints)
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
