using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;

namespace Simulation.Logic
{
	public partial class SimulationLogic
	{
		private void Handle_BearedOffEvent(BearOffEvent Event)
		{
			MoveInfo[] possibleMoves = Logic.GetPossibleBearedOffs(board, Event.From);
			if (possibleMoves == null)
				return;

			MoveInfo moveInfo = Utilities.FindInFromPoint(possibleMoves, Event.From);
			if (moveInfo == null)
				return;

			PointData fromPoint = moveInfo.From;

			PlayerData player = SimulationUtilities.GetPlayer(board, fromPoint.Color);
			if (player == null)
				return;

			if (Logic.GetInBaseCheckerCount(board, player.Color) + player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return;

			if (!SimulationUtilities.ApplyMoveCount(player, board.TurnDice, fromPoint.Index, SimulationUtilities.GetBearOffIndex(player.Color), true))
				return;

			++player.BearedOffCheckersCount;
			--fromPoint.CheckerCount;

			mutations.Add(new BearedOffMutation(Event.From));

			if (player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
			{
				PlayerData opponentPlayer = SimulationUtilities.GetOpponentPlayer(board, player.Color);
				if (opponentPlayer == null)
					return;

				if (opponentPlayer.BearedOffCheckersCount == 0)
				{
					if (Logic.GetInBaseOpponentCheckerCount(board, player.Color) == 0)
						mutations.Add(new GameFinishedMutation(player.Color, ConfigData.GAMMON_WIN_SCORE));
					else
						mutations.Add(new GameFinishedMutation(player.Color, ConfigData.BACKGAMMON_WIN_SCORE));
				}
				else
					mutations.Add(new GameFinishedMutation(player.Color, ConfigData.NORMAL_WIN_SCORE));
			}
		}
	}
}