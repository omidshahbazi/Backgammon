using Networking.Common;
using Simulation.Data.Game;

namespace Networking.Server
{
	class OneByOneRoom : Room
	{
		protected override Player WhitePlayer
		{
			get { return Players[0]; }
		}

		protected override Player BlackPlayer
		{
			get { return Players[1]; }
		}

		public override string BotPlayerInfo
		{
			get { return null; }
		}

		public OneByOneRoom(Application Application, uint TableEnterance) :
			base(Application, TableEnterance)
		{
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByOne);
		}

		protected override void HandleGetGameData(Player Player)
		{
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