using Networking.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;
using Networking.Server.Data;

namespace Networking.Server
{
	class OneByBotRoom : Room
	{
		private string botPlayerInfo;

		protected override Player WhitePlayer
		{
			get { return (PLayerCount == 1 ? Players[0] : null); }
		}

		protected override Player BlackPlayer
		{
			get { return null; }
		}

		public override string BotPlayerInfo
		{
			get { return botPlayerInfo; }
		}

		public OneByBotRoom(Application Application, uint Bet, float TurnTime) :
			base(Application, Bet, TurnTime)
		{
		}

		public override void Initialize()
		{
			ISerializeObject obj = BotPlayerInfoMaker.Make(WhitePlayer.ID);

			if (obj != null)
				botPlayerInfo = obj.Content;

			base.Initialize();
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Bet, WhitePlayer.Version);
		}

		protected override void HandleGetGameData(Player Player)
		{
			++ReadyPlayerCount;

			base.HandleGetGameData(Player);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			SendBuffer.WriteInt32((int)PlayerColors.White);

			Send(Player, SendBuffer);
		}

		protected override void ScheduleCheckTurnTime()
		{
			if (Simulator.Frame.Board.TurnColor == PlayerColors.Black)
				ScheduleWokerFor(Configs.Random.Next(4, TurnTime), CheckTurnTime);
			else
				base.ScheduleCheckTurnTime();
		}
	}
}