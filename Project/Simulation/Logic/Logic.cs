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

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount != 0)
				return null;

			return GetPossibleMoves(Board, fromPoint.Color, fromPoint.Index, true);
		}

		public static PointData[] GetPossibleBarToBoardMoves(BoardData Board, PlayerColors Color)
		{
			return GetPossibleMoves(Board, Color, SimulationUtilities.GetStartIndex(Color), false);
		}

		public static PointData[] GetPossibleBearedOffs(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			if (fromPoint.Color != Board.TurnColor)
				return null;

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);
			if (player == null)
				return null;

			if (player.BarCheckerCount != 0)
				return null;

			if (GetInBaseCheckerCount(Board, player.Color) + player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return null;

			int targetPointIndex = fromPoint.Index + (Board.TurnDice.Dice1 * SimulationUtilities.GetDirection(player.Color));

			if (0 <= targetPointIndex && targetPointIndex < ConfigData.POINT_COUNT)
				return null;

			PointDataList possiblePoints = new PointDataList();

			GetPossibleBearedOffs(fromPoint, 1, Board.TurnDice.Dice1, possiblePoints);
			GetPossibleBearedOffs(fromPoint, 1, Board.TurnDice.Dice2, possiblePoints);

			return possiblePoints.ToArray();
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

			if (player.BarCheckerCount == 0 || moveCount != 0)
				if (player.BarCheckerCount < maxMoves)
				{
					PointDataList possiblePoints = new PointDataList();

					for (int i = 0; i < Board.Points.Length; ++i)
					{
						PointData point = Board.Points[i];

						if (point.Color != Color)
							continue;

						GetPossibleMoves(Board, Color, i, true, possiblePoints);
					}

					moveCount += possiblePoints.Count;
				}

			return Math.Min(moveCount, maxMoves);
		}

		public static int GetOutOfBaseCheckerCount(BoardData Board, PlayerColors Color)
		{
			return (ConfigData.PLAYER_CHECKER_COUNT - GetInBaseCheckerCount(Board, Color));
		}

		public static int GetInBaseCheckerCount(BoardData Board, PlayerColors Color)
		{
			int fromIndex;
			int toIndex;
			SimulationUtilities.GetBaseIndecies(Color, out fromIndex, out toIndex);

			int count = 0;

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				if (point.Color != Color)
					continue;

				if (point.Index < fromIndex || toIndex < point.Index)
					continue;

				count += point.CheckerCount;
			}

			return count;
		}

		public static PointData[] GetPossibleMoves(BoardData Board, Identifier FromIdentifier, int Count)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount != 0)
				return null;

			if (fromPoint.Color != Board.TurnColor)
				return null;

			PointDataList possiblePoints = new PointDataList();

			GetPossibleMoves(Board.Points, fromPoint.Color, fromPoint.Index, Count, false, possiblePoints);

			return possiblePoints.ToArray();
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

			//int barCheckerCount = player.BarCheckerCount;
			//if (barCheckerCount != 0)
			//	return;

			if (Color != Board.TurnColor)
				return;

			int iteration = 1;
			if (player.MoveCount >= 2)
				iteration = SimulationUtilities.GetMoveCount(Board.TurnDice) / 2;

			PointData fromPoint = Board.Points[StartIndex];

			bool isBarToBoardMode = player.BarCheckerCount != 0;
			int checkerCount = isBarToBoardMode ? fromPoint.CheckerCount : player.BarCheckerCount;

			for (int i = 0; i < iteration; ++i)
			{
				int dice1 = Board.TurnDice.Dice1 * (i + 1);
				int dice2 = Board.TurnDice.Dice2 * (i + 1);

				bool isDice1Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice1, isBarToBoardMode, PossiblePointDataList);
				if (isDice1Open && --checkerCount == 0)
					break;

				bool isDice2Open = GetPossibleMoves(Board.Points, Color, StartIndex, dice2, isBarToBoardMode, PossiblePointDataList);

				if (UseSumOfDices && (isDice1Open || isDice2Open) && player.MoveCount >= 2)
					GetPossibleMoves(Board.Points, Color, StartIndex, dice1 + dice2, isBarToBoardMode, PossiblePointDataList);
			}
		}

		private static bool GetPossibleMoves(PointData[] Points, PlayerColors Color, int StartIndex, int Count, bool IsBarToBoardMode, PointDataList PossiblePoints)
		{
			if (Count == 0)
				return false;

			if (IsBarToBoardMode)
				--Count;

			PointData fromPoint = Points[StartIndex];
			if (fromPoint.CheckerCount == 0)
				return false;

			int targetPointIndex = StartIndex + (Count * SimulationUtilities.GetDirection(Color));

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

		private static bool GetPossibleBearedOffs(PointData FromPoint, int CheckerCount, int Count, PointDataList PossiblePointDataList)
		{
			if (Count == 0)
				return false;

			if (FromPoint.CheckerCount < CheckerCount)
				return false;

			int targetPointIndex = FromPoint.Index + (Count * SimulationUtilities.GetDirection(FromPoint.Color));

			if (0 <= targetPointIndex && targetPointIndex < ConfigData.POINT_COUNT)
				return false;

			PossiblePointDataList.Add(FromPoint);

			return true;
		}
	}
}
