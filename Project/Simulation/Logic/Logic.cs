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

		private static class BoardToBoard
		{
			public static bool FillPossibleMove(BoardData Board, PlayerData Player, PointData FromPoint, PointDataList Moves)
			{
				int movesCount = Moves.Count;

				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
					FillPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i], Moves);

				if (Board.TurnDice.IsPair)
				{
					for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
					{
						if (i == 0)
						{
							if (GetPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i]) == null)
								break;

							continue;
						}

						if (FillPossibleMove(Board.Points, Player, FromPoint, i * Board.TurnDice.Moves[i], Moves))
							continue;
					}
				}
				else
				{
					for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
					{
						if (i == 0)
						{
							if (GetPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i]) == null)
								break;

							continue;
						}

						if (FillPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i - 1] + Board.TurnDice.Moves[i], Moves))
							continue;
					}

					for (int i = Board.TurnDice.Moves.Length - 1; i >= 0; --i)
					{
						if (i == Board.TurnDice.Moves.Length - 1)
						{
							if (GetPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i]) == null)
								break;

							continue;
						}

						if (FillPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i + 1] + Board.TurnDice.Moves[i], Moves))
							continue;
					}
				}

				return (Moves.Count != movesCount);
			}

			public static bool FillPossibleMove(PointData[] Points, PlayerData Player, PointData FromPoint, int Dice, PointDataList Moves)
			{
				MoveInfo info = GetPossibleMove(Points, Player, FromPoint, Dice);

				if (info == null)
					return false;

				Moves.Add(info);

				return true;
			}

			private static MoveInfo GetPossibleMove(PointData[] Points, PlayerData Player, PointData FromPoint, int Dice)
			{
				if (Player.BarCheckerCount != 0)
					return null;

				int targetPointIndex = FromPoint.Index + (Dice * Utilities.GetDirection(FromPoint.Color));

				if (targetPointIndex < 0 || Points.Length <= targetPointIndex)
					return null;

				PointData targetPoint = Points[targetPointIndex];

				if (!Utilities.IsPointOpenToMoveTo(targetPoint, FromPoint.Color))
					return null;

				return new MoveInfo() { From = FromPoint, To = targetPoint };
			}
		}

		private static class BarToBoard
		{
			public static bool FillPossibleMove(BoardData Board, PlayerData Player, PointDataList Moves)
			{
				int movesCount = Moves.Count;

				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
					FillPossibleMove(Board.Points, Player, Board.TurnColor, Board.TurnDice.Moves[i], Moves);

				return (Moves.Count != movesCount);
			}

			public static bool FillPossibleMove(PointData[] Points, PlayerData Player, PlayerColors Color, int Dice, PointDataList Moves)
			{
				MoveInfo info = GetPossibleMove(Points, Player, Color, Dice);

				if (info == null)
					return false;

				if (Player.BarCheckerCount == 0)
					return false;

				Moves.Add(info);

				return true;
			}

			private static MoveInfo GetPossibleMove(PointData[] Points, PlayerData Player, PlayerColors Color, int Dice)
			{
				if (Player.BarCheckerCount == 0)
					return null;

				int fromIndex = Utilities.GetStartIndex(Color) + (Utilities.GetDirection(Color) * -1);

				int targetPointIndex = fromIndex + (Dice * Utilities.GetDirection(Color));

				if (targetPointIndex < 0 || Points.Length <= targetPointIndex)
					return null;

				PointData targetPoint = Points[targetPointIndex];

				if (!Utilities.IsPointOpenToMoveTo(targetPoint, Color))
					return null;

				return new MoveInfo() { From = null, To = targetPoint };
			}
		}

		private static class BearOff
		{
			public static bool FillPossibleMove(BoardData Board, PlayerData Player, PointData FromPoint, PointDataList Moves)
			{
				int movesCount = Moves.Count;

				// Avoid bear off if there's checker befor FromPoint based on current dice
				????

				for (int i = 0; i < Board.TurnDice.Moves.Length; ++i)
					FillPossibleMove(Board.Points, Player, FromPoint, Board.TurnDice.Moves[i], Moves);

				return (Moves.Count != movesCount);
			}

			private static bool FillPossibleMove(PointData[] Points, PlayerData Player, PointData FromPoint, int Dice, PointDataList Moves)
			{
				MoveInfo info = GetPossibleMove(Points, Player, FromPoint, Dice);

				if (info == null)
					return false;

				Moves.Add(info);

				return true;
			}

			private static MoveInfo GetPossibleMove(PointData[] Points, PlayerData Player, PointData FromPoint, int Dice)
			{
				if (Player.BarCheckerCount != 0)
					return null;

				if (Utilities.GetInBaseCheckerCount(Points, FromPoint.Color) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
					return null;

				int targetPointIndex = FromPoint.Index + (Dice * Utilities.GetDirection(FromPoint.Color));

				if (0 <= targetPointIndex && targetPointIndex < ConfigData.POINT_COUNT)
					return null;

				return new MoveInfo() { From = FromPoint, To = null };
			}
		}

		public static MoveInfo[] GetPossibleBoardToBoardMoves(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null || !Utilities.IsPointOpenToMoveFrom(fromPoint, Board.TurnColor))
				return null;

			PlayerData player = Utilities.GetPlayer(Board, Board.TurnColor);
			if (player == null)
				return null;

			PointDataList moves = new PointDataList();

			BoardToBoard.FillPossibleMove(Board, player, fromPoint, moves);

			return moves.ToArray();
		}

		public static MoveInfo[] GetPossibleBarToBoardMoves(BoardData Board)
		{
			PlayerData player = Utilities.GetPlayer(Board, Board.TurnColor);
			if (player == null)
				return null;

			PointDataList moves = new PointDataList();

			BarToBoard.FillPossibleMove(Board, player, moves);

			return moves.ToArray();
		}

		public static MoveInfo[] GetPossibleBearedOffs(BoardData Board, Identifier FromIdentifier)
		{
			PointData fromPoint = Utilities.FindPoint(Board, FromIdentifier);
			if (fromPoint == null || !Utilities.IsPointOpenToMoveFrom(fromPoint, Board.TurnColor))
				return null;

			PlayerData player = Utilities.GetPlayer(Board, Board.TurnColor);
			if (player == null)
				return null;

			PointDataList moves = new PointDataList();

			BearOff.FillPossibleMove(Board, player, fromPoint, moves);

			return moves.ToArray();
		}

		public static int GetTotalPossibleMoveCount(BoardData Board)
		{
			PlayerData player = Utilities.GetPlayer(Board, Board.TurnColor);
			if (player == null)
				return 0;

			int maxMoves = Utilities.GetMoveCount(Board.TurnDice);

			PointDataList moves = new PointDataList();

			BarToBoard.FillPossibleMove(Board, player, moves);

			for (int i = 0; i < Board.Points.Length; ++i)
			{
				PointData point = Board.Points[i];

				if (!Utilities.IsPointOpenToMoveFrom(point, Board.TurnColor))
					continue;

				BoardToBoard.FillPossibleMove(Board, player, point, moves);
			}

			int fromIndex;
			int toIndex;
			Utilities.GetBaseIndecies(Board.TurnColor, out fromIndex, out toIndex);

			int incDir = Utilities.GetDirection(Board.TurnColor);
			if (incDir == 1)
			{
				for (int i = fromIndex; i <= toIndex; ++i)
				{
					PointData point = Board.Points[i];

					if (!Utilities.IsPointOpenToMoveFrom(point, Board.TurnColor))
						continue;

					BearOff.FillPossibleMove(Board, player, point, moves);
				}
			}
			else if (incDir == -1)
			{
				for (int i = toIndex; i >= fromIndex; --i)
				{
					PointData point = Board.Points[i];

					if (!Utilities.IsPointOpenToMoveFrom(point, Board.TurnColor))
						continue;

					BearOff.FillPossibleMove(Board, player, point, moves);
				}
			}




			//int[] moveDices = new int[Board.TurnDice.Moves.Length];
			//Array.Copy(Board.TurnDice.Moves, moveDices, moveDices.Length);

			//for (int i = 0; i < moves.Count; ++i)// ??? check remove lines in the following
			//{
			//	MoveInfo info = moves[i];

			//	int movement = 0;
			//	bool isBearOff = false;

			//	if (info.From != null && info.To != null)
			//		movement = Math.Abs(info.To.Index - info.From.Index);
			//	else if (info.From != null)
			//	{
			//		movement = Math.Abs(SimulationUtilities.GetBearOffIndex(Board.TurnColor) - info.From.Index);
			//		isBearOff = true;
			//	}
			//	else if (info.To != null)
			//		movement = Math.Abs(info.To.Index - SimulationUtilities.GetBarIndex(Board.TurnColor));

			//	int index = -1;
			//	if (SimulationUtilities.IsMovePossible(moveDices, movement, isBearOff, out index))
			//		ArrayUtilities.RemoveAt(ref moveDices, index);
			//	else
			//		moves.RemoveAt(i--);
			//}

			return Math.Min(maxMoves, moves.Count);
		}
	}
}
