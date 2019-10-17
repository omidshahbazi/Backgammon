using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_FinishTurnEvent(FinishTurnEvent Event)
		{
			if (Event.Color != board.TurnColor)
				return;

			PlayerData player = Utilities.GetPlayer(board, Event.Color);
			if (player == null || player.MoveCount != 0 || player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
				return;

			PlayerData opponentPlayer = Utilities.GetOpponentPlayer(board, Event.Color);
			if (opponentPlayer == null)
				return;

			SimulationUtilities.RandomDices(config, board.TurnDice);

			board.TurnColor = (Event.Color == PlayerColors.White ? PlayerColors.Black : PlayerColors.White);

			opponentPlayer.MoveCount = Logic.GetTotalPossibleMoveCount(board);

			++board.TurnNumber;

			mutations.Add(new TurnChangedMutation(Event.Color));
		}
	}
}