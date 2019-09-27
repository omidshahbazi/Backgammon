using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_FinishTurnEvent(FinishTurnEvent Event)
		{
			if (Event.Color != board.TurnColor)
				return;

			board.TurnColor = (Event.Color == PlayerColors.White ? PlayerColors.Black : PlayerColors.White);

			SimulationUtilities.RandomDices(config, board.TurnDice, mutations);
			//check possible and done moves
			//change mutation dicechanged to trun changed
			???
		}
	}
}