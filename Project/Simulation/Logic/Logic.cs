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

			PointDataList possiblePointDataList = new PointDataList();

			for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
			{
				if (fromPoint.CheckerCount == 0)
					continue;

				int targetPointIndex = fromPoint.Index + (Board.TurnDice.Moves[i] * SimulationUtilities.GetDirection(fromPoint.Color));

				if (0 <= targetPointIndex && targetPointIndex < ConfigData.POINT_COUNT)
					continue;

				possiblePointDataList.Add(fromPoint);
			}

			return possiblePointDataList.ToArray();
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
			return GetInBaseCheckerCount(Board, Color, Color);
		}

		public static int GetInBaseOpponentCheckerCount(BoardData Board, PlayerColors Color)
		{
			return GetInBaseCheckerCount(Board, Color, (Color == PlayerColors.White ? PlayerColors.Black : PlayerColors.White));
		}

		private static int GetInBaseCheckerCount(BoardData Board, PlayerColors BaseColor, PlayerColors CheckerColor)
		{
			int fromIndex;
			int toIndex;
			SimulationUtilities.GetBaseIndecies(BaseColor, out fromIndex, out toIndex);

			int count = 0;

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				if (point.Color != CheckerColor)
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

			PointDataList possiblePointDataList = new PointDataList();

			GetPossibleMove(Board.Points, fromPoint.Color, fromPoint.Index, Count, false, possiblePointDataList);

			return possiblePointDataList.ToArray();
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

			if (Color != Board.TurnColor)
				return;

			PointData fromPoint = Board.Points[StartIndex];

			bool isBarToBoardMode = player.BarCheckerCount != 0;
			int checkerCount = isBarToBoardMode ? fromPoint.CheckerCount : player.BarCheckerCount;

			for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i], isBarToBoardMode, PossiblePointDataList);

			if (!UseSumOfDices)
				return;

			if (Board.TurnDice.IsPair)
			{
				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					if (i == 0)
					{
						if (GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i], isBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, StartIndex, i * Board.TurnDice.Moves[i], isBarToBoardMode, PossiblePointDataList))
						continue;
				}
			}
			else
			{
				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					if (i == 0)
					{
						if (GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i], isBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i - 1] + Board.TurnDice.Moves[i], isBarToBoardMode, PossiblePointDataList))
						continue;
				}

				for (int i = Board.TurnDice.Moves.Length - 1; i >= 0; --i)
				{
					if (i == Board.TurnDice.Moves.Length - 1)
					{
						if (GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i], isBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, StartIndex, Board.TurnDice.Moves[i + 1] + Board.TurnDice.Moves[i], isBarToBoardMode, PossiblePointDataList))
						continue;
				}
			}
		}

		private static bool GetPossibleMove(PointData[] Points, PlayerColors Color, int StartIndex, int Count, bool IsBarToBoardMode, PointDataList PossiblePointDataList)
		{
			PointData point = GetPossibleMove(Points, Color, StartIndex, Count, IsBarToBoardMode);

			if (point == null)
				return false;

			PossiblePointDataList.Add(point);

			return true;
		}

		private static PointData GetPossibleMove(PointData[] Points, PlayerColors Color, int StartIndex, int Count, bool IsBarToBoardMode)
		{
			if (Count == 0)
				return null;

			if (IsBarToBoardMode)
				--Count;

			PointData fromPoint = Points[StartIndex];
			if (fromPoint.CheckerCount == 0)
				return null;

			int targetPointIndex = StartIndex + (Count * SimulationUtilities.GetDirection(Color));

			if (targetPointIndex < 0 || Points.Length <= targetPointIndex)
				return null;

			PointData targetPoint = Points[targetPointIndex];

			if (targetPoint.CheckerCount != 0)
			{
				if (targetPoint.Color != Color)
				{
					if (targetPoint.CheckerCount > 1)
						return null;
				}
			}

			return targetPoint;
		}
	}
}
