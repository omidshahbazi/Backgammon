using Simulation.Common;
using Simulation.Data.Game;
using System.Collections.Generic;

namespace Simulation.Data.Serialization
{
	public static class DiffFinder
	{
		public class DiffInfo
		{
			public DataBase A
			{
				get;
				private set;
			}

			public DataBase B
			{
				get;
				private set;
			}

			public DiffInfo(DataBase A, DataBase B)
			{
				this.A = A;
				this.B = B;
			}
		}

		public class DiffInfoList : List<DiffInfo>
		{ }

		public static void Find(BoardData A, BoardData B, DiffInfoList Diffs)
		{
			if (A == null && B == null)
				return;

			if ((A == null || B == null) && A != B)
			{
				AddDiff(A, B, Diffs);
				return;
			}

			HasherVisitor hasher = new HasherVisitor();

			A.Visit(hasher);
			int aHash = hasher.Value;

			hasher.Reset();
			B.Visit(hasher);
			int bHash = hasher.Value;

			if (aHash == bHash)
				return;

			AddDiff(A, B, Diffs);

			if (A.Points != null && B.Points != null && A.Points.Length == B.Points.Length)
				for (int i = 0; i < A.Points.Length; ++i)
					Find(A.Points[i], B.Points[i], Diffs);

			Find(A.WhitePlayer, B.WhitePlayer, Diffs);
			Find(A.BlackPlayer, B.BlackPlayer, Diffs);

			Find(A.TurnDice, B.TurnDice, Diffs);
		}

		private static void Find(PointData A, PointData B, DiffInfoList Diffs)
		{
			if (A == null && B == null)
				return;

			if ((A == null || B == null) && A != B)
			{
				AddDiff(A, B, Diffs);
				return;
			}

			HasherVisitor hasher = new HasherVisitor();

			A.Visit(hasher);
			int aHash = hasher.Value;

			hasher.Reset();
			B.Visit(hasher);
			int bHash = hasher.Value;

			if (aHash == bHash)
				return;

			AddDiff(A, B, Diffs);
		}

		private static void Find(PlayerData A, PlayerData B, DiffInfoList Diffs)
		{
			if (A == null && B == null)
				return;

			if ((A == null || B == null) && A != B)
			{
				AddDiff(A, B, Diffs);
				return;
			}

			HasherVisitor hasher = new HasherVisitor();

			A.Visit(hasher);
			int aHash = hasher.Value;

			hasher.Reset();
			B.Visit(hasher);
			int bHash = hasher.Value;

			if (aHash == bHash)
				return;

			AddDiff(A, B, Diffs);

			Find(A.InitialDice, B.InitialDice, Diffs);
		}

		private static void Find(DiceData A, DiceData B, DiffInfoList Diffs)
		{
			if (A == null && B == null)
				return;

			if ((A == null || B == null) && A != B)
			{
				AddDiff(A, B, Diffs);
				return;
			}

			HasherVisitor hasher = new HasherVisitor();

			A.Visit(hasher);
			int aHash = hasher.Value;

			hasher.Reset();
			B.Visit(hasher);
			int bHash = hasher.Value;

			if (aHash == bHash)
				return;

			AddDiff(A, B, Diffs);
		}

		private static void AddDiff(DataBase A, DataBase B, DiffInfoList Diffs)
		{
			Diffs.Add(new DiffInfo(A, B));
		}
	}
}
