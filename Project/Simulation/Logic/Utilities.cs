using Simulation.Common;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public static class Utilities
	{
		public static PointData FindPoint(BoardData Board, Identifier Identifier)
		{
			return FindPoint(Board.Points, Identifier);
		}

		public static PointData FindPoint(PointData[] Points, Identifier Identifier)
		{
			for (int i = 0; i < Points.Length; ++i)
			{
				PointData point = Points[i];

				if (point.ID != Identifier)
					continue;

				return point;
			}

			return null;
		}
	}
}
