using Networking.Common;
using Simulation.Data.Game;

namespace Networking.Server
{
	class Room : RoomBase
	{
		public Room(Application Application, int GameID, uint TableEnterance) :
			base(Application, GameID, TableEnterance)
		{
		}

		protected override void HandleGetGameData(Player Player)
		{
			if (WhitePlayer == null)
			{
				WhitePlayer = Players[0];

				if (Players.Count > 1)
					BlackPlayer = Players[1];
			}

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			if (Player == WhitePlayer)
				SendBuffer.WriteInt32((int)PlayerColors.White);
			else
				SendBuffer.WriteInt32((int)PlayerColors.Black);

			Send(Player, SendBuffer);
		}

		protected override void AddWinnerReward(Player WinnerPlayer, RewardInfo Reward)
		{
			DatabaseLayer.AddReward(WinnerPlayer.ID, Reward);
		}
	}
}