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

			PlayerData player = Utilities.GetPlayer(board, fromPoint.Color);
			if (player == null)
				return;

			if (Utilities.GetInBaseCheckerCount(board.Points, player.Color) + player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return;

			if (!SimulationUtilities.ApplyMoveCount(board, player, fromPoint.Index, Utilities.GetBearOffIndex(player.Color), true))
				return;

			++player.BearedOffCheckersCount;
			--fromPoint.CheckerCount;

			SimulationUtilities.UpdateMoveCount(board, player);

			mutations.Add(new BearedOffMutation(Event.From));

			if (player.BearedOffCheckersCount == ConfigData.PLAYER_CHECKER_COUNT)
			{
				PlayerData opponentPlayer = Utilities.GetOpponentPlayer(board, player.Color);
				if (opponentPlayer == null)
					return;

				if (opponentPlayer.BearedOffCheckersCount == 0)
				{
					if (Utilities.GetInBaseOpponentCheckerCount(board.Points, player.Color) == 0)
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