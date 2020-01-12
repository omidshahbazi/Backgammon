using Networking.Common;
using Networking.Server.Data;

namespace Networking.Server
{
	class FriendlyRoom : OneByOneRoom
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

		public FriendlyRoom(Application Application, int TableID, int TurnTime) :
			base(Application, TableID, TurnTime)
		{
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.Freiendly, TableID, WhitePlayer.Version);
		}

		protected override RewardInfo GetWinnerPrize(Player Player)
		{
			return null;
		}
	}
}