using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BarToBoardMoveEvent(BarToBoardMoveEvent Event)
		{
			//PointData[] possibleTargetPoints = Logic.GetPossibleTargetPoints(board, Event.From);
			//PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);

			//if (toPoint == null)
			//	return;

			//PointData fromPoint = Utilities.FindPoint(board, Event.From);

			//if (toPoint.CheckerCount == 1 && toPoint.Color != board.TurnColor)
			//{
			//	if (toPoint.Color == PlayerColors.White)
			//	{
			//		++board.OnBarWhiteCheckerCount;

			//		// mutation
			//	}
			//	else if (toPoint.Color == PlayerColors.Black)
			//	{
			//		++board.OnBarBlackCheckerCount;

			//		// mutation
			//	}
			//}

			//--fromPoint.CheckerCount;
			//++toPoint.CheckerCount;
			//toPoint.Color = fromPoint.Color;

			//mutations.Add(new BoardToBoardMoveMutation(Event.From, Event.To));
		}
	}
}