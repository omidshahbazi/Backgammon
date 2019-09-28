using Simulation.Common;
using Simulation.Data.Game;
using System;
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

			return GetPossibleMoves(Board, fromPoint.Color, fromPoint.Index, true);
		}

		public static PointData[] GetPossibleBarToBoardMoves(BoardData Board, PlayerColors Color)
		{
			return GetPossibleMoves(Board, Color, SimulationUtilities.GetStartIndex(Color), false);
		}

		public static int GetTotalPossibleMoveCount(BoardData Board, PlayerColors Color)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);
			if (player == null)
				return 0;

			int maxMoves = SimulationUtilities.GetMoveCount(Board.TurnDice);
			int moveCount = 0;

			if (player.BarCheckerCount != 0)
			{
				moveCount = GetPossibleMoves(Board, Color, SimulationUtilities.GetStartIndex(Color), false).Length;
				moveCount = Math.Min(moveCount, player.BarCheckerCount);
			}

			if (player.BarCheckerCount < maxMoves)
			{
				PointDataList possiblePoints = new PointDataList();

				for (int i = 0; i < Board.Points.Length; ++i)
				{
					PointData point = Board.Points[i];

					if (point.Color != Color)
						continue;

					GetPossibleMoves(Board, Color, i, true);
				}

				moveCount += possiblePoints.Count;
			}

			return Math.Min(moveCount, maxMoves);
		}

		private static PointData[] GetPossibleMoves(BoardData Board, PlayerColors Color, int StartIndex, bool UseSumOfDices)
		{
			PointDataList possiblePoints = new PointDataList();

			GetPossibleMoves(Board, Color, StartIndex, UseSumOfDices, possiblePoints);

			return possiblePoints.ToArray();
		}

		private static void GetPossibleMoves(BoardData Board, PlayerColors Color, int StartIndex, bool UseSumOfDices, PointDataList PossiblePointDataList)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount == 0)
				return;

			if (Color != Board.TurnColor)
				return;

			int iteration = SimulationUtilities.GetMoveCount(Board.TurnDice) / 2;

			for (int i = 0; i < iteration; ++i)
			{
				int dice1 = Board.TurnDice.Dice1 * (i + 1);
				int dice2 = Board.TurnDice.Dice2 * (i + 1);

				bool isDice1Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice1, PossiblePointDataList);
				bool isDice2Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice2, PossiblePointDataList);

				if (UseSumOfDices && (isDice1Open || isDice2Open))
					GetPossibleMoves(Board.Points, Color, StartIndex, dice1 + dice2, PossiblePointDataList);
			}
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
