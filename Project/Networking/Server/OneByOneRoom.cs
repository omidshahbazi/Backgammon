using Networking.Server.Data;

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

		public OneByOneRoom(Application Application, int TableID, int TurnTime) :
			base(Application, TableID, TurnTime)
		{
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByOne, TableID, WhitePlayer.Version);
		}

		protected override void InitializeGame()
		{
			DatabaseLayer.InitializeGame(GameID, WhitePlayer.ID, BlackPlayer.ID, "");
		}
	}
}