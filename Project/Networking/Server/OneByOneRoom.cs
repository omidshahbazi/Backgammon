using Networking.Common;
using Networking.Server.Data;
using Simulation.Data.Game;

namespace Networking.Server
{
	class OneByOneRoom : Room
	{
		protected override Player WhitePlayer
		{
			get { return (PLayerCount == 2 ? Players[0] : null); }
		}

		protected override Player BlackPlayer
		{
			get { return (PLayerCount == 2 ? Players[1] : null); }
		}

		public override string BotPlayerInfo
		{
			get { return null; }
		}

		public OneByOneRoom(Application Application, uint Bet, float TurnTime) :
			base(Application, Bet, TurnTime)
		{
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByOne, Bet, WhitePlayer.Version);
		}

		protected override void HandleGetGameData(Player Player)
		{
			++ReadyPlayerCount;

			base.HandleGetGameData(Player);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			if (Player == WhitePlayer)
				SendBuffer.WriteInt32((int)PlayerColors.White);
			else
				SendBuffer.WriteInt32((int)PlayerColors.Black);

			Send(Player, SendBuffer);
		}
	}
}