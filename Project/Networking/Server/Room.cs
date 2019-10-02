using Networking.Common;
using Simulation.Data.Game;

namespace Networking.Server
{
	class Room : RoomBase
	{
		public Room(Application Application, int GameID) :
			base(Application, GameID)
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

		protected override void HandleGameFinisher(Player Player, GameFinishReasons Reason)
		{
			base.HandleGameFinisher(Player, Reason);

			// add (table enterance * 2) * 0.8 to player
		}
	}
}