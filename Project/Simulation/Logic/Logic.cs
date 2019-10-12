using GameFramework.Common.Extensions;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using System.Collections.Generic;

namespace Simulation.Logic
{
	public static class Logic
	{
		private class PointDataList : List<MoveInfo>
		{ }

		public static MoveInfo[] GetPossibleBoardToBoardMoves(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null || fromPoint.CheckerCount == 0)
				return null;

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount != 0)
				return null;

			return GetPossibleMoves(Board, fromPoint.Color, fromPoint, false);
		}

		public static MoveInfo[] GetPossibleBarToBoardMoves(BoardData Board, PlayerColors Color)
		{
			return GetPossibleMoves(Board, Color, Board.Points[SimulationUtilities.GetStartIndex(Color)], true);
		}

		public static MoveInfo[] GetPossibleBearedOffs(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			PointDataList possiblePointDataList = new PointDataList();

			GetPossibleBearedOffs(Board, fromPoint, possiblePointDataList);

			return possiblePointDataList.ToArray();
		}

		//public static int GetTotalPossibleMoveCount(BoardData Board, PlayerColors Color)
		//{
		//	PlayerData player = SimulationUtilities.GetPlayer(Board, Color);
		//	if (player == null)
		//		return 0;

		//	int maxMoves = SimulationUtilities.GetMoveCount(Board.TurnDice);
		//	int moveCount = 0;

		//	if (player.BarCheckerCount != 0)
		//	{
		//		moveCount = GetPossibleMoves(Board, Color, Board.Points[SimulationUtilities.GetStartIndex(Color)], false).Length;
		//		moveCount = Math.Min(moveCount, player.BarCheckerCount);
		//	}

		//	if (player.BarCheckerCount == 0 || moveCount != 0)
		//		if (player.BarCheckerCount < maxMoves)
		//		{
		//			PointDataList possiblePointDataList = new PointDataList();

		//			for (int i = 0; i < Board.Points.Length; ++i)
		//			{
		//				PointData point = Board.Points[i];

		//				if (point.Color != Color)
		//					continue;

		//				GetPossibleMoves(Board, Color, point, true, possiblePointDataList);
		//			}

		//			moveCount += possiblePointDataList.Count;
		//		}

		//	return Math.Min(moveCount, maxMoves);
		//}

		public static int GetTotalPossibleMoveCount(BoardData Board, PlayerColors Color)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);
			if (player == null)
				return 0;

			int maxMoves = SimulationUtilities.GetMoveCount(Board.TurnDice);

			PointDataList possiblePointDataList = new PointDataList();

			if (player.BarCheckerCount != 0)
				GetPossibleMoves(Board, Color, Board.Points[SimulationUtilities.GetStartIndex(Color)], true, possiblePointDataList);

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				GetPossibleMoves(Board, Color, point, false, possiblePointDataList);

				GetPossibleBearedOffs(Board, point, possiblePointDataList);
			}

			int[] moves = new int[Board.TurnDice.Moves.Length];
			Array.Copy(Board.TurnDice.Moves, moves, moves.Length);

			for (int i = 0; i < possiblePointDataList.Count; ++i)
			{
				MoveInfo info = possiblePointDataList[i];

				int movement = 0;
				bool isBearOff = false;

				if (info.From != null && info.To != null)
					movement = Math.Abs(info.To.Index - info.From.Index);
				else if (info.From != null)
				{
					movement = Math.Abs(SimulationUtilities.GetOutIndex(Color) - info.From.Index);
					isBearOff = true;
				}
				else if (info.To != null)
					movement = Math.Abs(info.To.Index - SimulationUtilities.GetStartIndex(Color));

				int index = -1;
				if (SimulationUtilities.IsMovePossible(moves, movement, isBearOff, out index))
					ArrayUtilities.RemoveAt(ref moves, index);
				else
					possiblePointDataList.RemoveAt(i--);

				//int index = -1;
				//if (!SimulationUtilities.IsMovePossible(moves, movement, isBearOff, out index))
				//	possiblePointDataList.RemoveAt(i--);
			}

			return Math.Min(maxMoves, possiblePointDataList.Count);
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

		public static MoveInfo[] GetPossibleMoves(BoardData Board, Identifier FromIdentifier, int Count)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null)
				return null;

			PlayerData player = SimulationUtilities.GetPlayer(Board, fromPoint.Color);

			int barCheckerCount = player.BarCheckerCount;
			if (barCheckerCount != 0)
				return null;

			PointDataList possiblePointDataList = new PointDataList();

			GetPossibleMove(Board.Points, fromPoint.Color, fromPoint, Count, false, possiblePointDataList);

			return possiblePointDataList.ToArray();
		}

		private static MoveInfo[] GetPossibleMoves(BoardData Board, PlayerColors Color, PointData FromPoint, bool IsBarToBoardMode)
		{
			PointDataList possiblePoints = new PointDataList();

			GetPossibleMoves(Board, Color, FromPoint, IsBarToBoardMode, possiblePoints);

			return possiblePoints.ToArray();
		}

		private static void GetPossibleMoves(BoardData Board, PlayerColors Color, PointData FromPoint, bool IsBarToBoardMode, PointDataList PossiblePointDataList)
		{
			PlayerData player = SimulationUtilities.GetPlayer(Board, Color);

			if (Color != Board.TurnColor)
				return;

			for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i], IsBarToBoardMode, PossiblePointDataList);

			if (IsBarToBoardMode)
				return;

			if (Board.TurnDice.IsPair)
			{
				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					if (i == 0)
					{
						if (GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i], IsBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, FromPoint, i * Board.TurnDice.Moves[i], IsBarToBoardMode, PossiblePointDataList))
						continue;
				}
			}
			else
			{
				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
				{
					if (i == 0)
					{
						if (GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i], IsBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i - 1] + Board.TurnDice.Moves[i], IsBarToBoardMode, PossiblePointDataList))
						continue;
				}

				for (int i = Board.TurnDice.Moves.Length - 1; i >= 0; --i)
				{
					if (i == Board.TurnDice.Moves.Length - 1)
					{
						if (GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i], IsBarToBoardMode) == null)
							break;

						continue;
					}

					if (GetPossibleMove(Board.Points, Color, FromPoint, Board.TurnDice.Moves[i + 1] + Board.TurnDice.Moves[i], IsBarToBoardMode, PossiblePointDataList))
						continue;
				}
			}
		}

		private static bool GetPossibleMove(PointData[] Points, PlayerColors Color, PointData FromPoint, int Count, bool IsBarToBoardMode, PointDataList PossiblePointDataList)
		{
			PointData point = GetPossibleMove(Points, Color, FromPoint, Count, IsBarToBoardMode);

			if (point == null)
				return false;

			PossiblePointDataList.Add(new MoveInfo() { From = (IsBarToBoardMode ? null : FromPoint), To = point });

			return true;
		}

		private static PointData GetPossibleMove(PointData[] Points, PlayerColors Color, PointData FromPoint, int Count, bool IsBarToBoardMode)
		{
			if (Count == 0)
				return null;

			if (!IsBarToBoardMode)
			{
				if (FromPoint.CheckerCount == 0)
					return null;
				else if (Color != FromPoint.Color)
					return null;
			}

			if (IsBarToBoardMode)
				Count += SimulationUtilities.GetDirection(Color);

			int targetPointIndex = FromPoint.Index + (Count * SimulationUtilities.GetDirection(Color));

			if (targetPointIndex < 0 || Points.Length <= targetPointIndex)
				return null;

			PointData targetPoint = Points[targetPointIndex];

			if (targetPoint.Color != Color && targetPoint.CheckerCount > 1)
				return null;

			return targetPoint;
		}

		private static void GetPossibleBearedOffs(BoardData Board, PointData FromPoint, PointDataList PossiblePointDataList)
		{
			if (FromPoint.Color != Board.TurnColor)
				return;

			if (FromPoint.CheckerCount == 0)
				return;

			PlayerData player = SimulationUtilities.GetPlayer(Board, FromPoint.Color);
			if (player == null)
				return;

			if (player.BarCheckerCount != 0)
				return;

			if (GetInBaseCheckerCount(Board, player.Color) + player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return;

			for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
			{
				int targetPointIndex = FromPoint.Index + (Board.TurnDice.Moves[i] * SimulationUtilities.GetDirection(FromPoint.Color));

				if (0 <= targetPointIndex && targetPointIndex < ConfigData.POINT_COUNT)
					continue;

				PossiblePointDataList.Add(new MoveInfo() { From = FromPoint, To = null });
			}
		}
	}
}
