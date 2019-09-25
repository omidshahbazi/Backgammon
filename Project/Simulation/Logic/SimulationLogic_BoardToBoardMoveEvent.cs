using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BoardToBoardMoveEvent(BoardToBoardMoveEvent Event)
		{
			PointData[] possibleTargetPoints = Logic.GetPossibleBoardToBoard(board, Event.From);

			if (possibleTargetPoints == null)
				return;

			PointData toPoint = Utilities.FindPoint(possibleTargetPoints, Event.To);

			if (toPoint == null)
				return;

			PointData fromPoint = Utilities.FindPoint(board, Event.From);

			if (toPoint.CheckerCount == 1 && toPoint.Color != board.TurnColor)
			{
				if (toPoint.Color == PlayerColors.White)
				{
					++board.WhiteBarCheckerCount;

					mutations.Add(new BoardToBarMoveMutation(Event.From));
				}
				else if (toPoint.Color == PlayerColors.Black)
				{
					++board.BlackBarCheckerCount;

					mutations.Add(new BoardToBarMoveMutation(Event.From));
				}
			}

			--fromPoint.CheckerCount;
			++toPoint.CheckerCount;
			toPoint.Color = fromPoint.Color;

			mutations.Add(new BoardToBoardMoveMutation(Event.From, Event.To));
		}
	}
}