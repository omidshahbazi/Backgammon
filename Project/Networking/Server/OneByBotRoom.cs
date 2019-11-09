//#define BOT_IS_BLACK
using Networking.Common;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.ASCIISerializer;
using Networking.Server.Data;

namespace Networking.Server
{
	class OneByBotRoom : Room
	{
		private string botPlayerInfo;

		protected override Player WhitePlayer
		{
#if BOT_IS_BLACK
			get { return (PLayerCount == 1 ? Players[0] : null); }
#else
			get { return null; }
#endif
		}

		protected override Player BlackPlayer
		{
#if BOT_IS_BLACK
			get { return null; }
#else
			get { return (PLayerCount == 1 ? Players[0] : null); }
#endif
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
#if BOT_IS_BLACK
			ISerializeObject obj = BotPlayerInfoMaker.Make(WhitePlayer.ID);
#else
			ISerializeObject obj = BotPlayerInfoMaker.Make(BlackPlayer.ID);
#endif

			if (obj != null)
				botPlayerInfo = obj.Content;

			base.Initialize();
		}

		protected override int CreateGame()
		{
#if BOT_IS_BLACK
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Bet, WhitePlayer.Version);
#else
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Bet, BlackPlayer.Version);
#endif
		}

		protected override void InitializeGame()
		{
#if BOT_IS_BLACK
			DatabaseLayer.InitializeGame(GameID, WhitePlayer.ID, Constants.NULL_USER_ID, BotPlayerInfo);
#else
			DatabaseLayer.InitializeGame(GameID, Constants.NULL_USER_ID, BlackPlayer.ID, BotPlayerInfo);
#endif
		}

		protected override void HandleGetGameData(Player Player)
		{
			++ReadyPlayerCount;

			base.HandleGetGameData(Player);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

#if BOT_IS_BLACK
			SendBuffer.WriteInt32((int)PlayerColors.White);
#else
			SendBuffer.WriteInt32((int)PlayerColors.Black);
#endif

			Send(Player, SendBuffer);
		}

		protected override void ScheduleCheckTurnTime()
		{
#if BOT_IS_BLACK
			if (Simulator.Frame.Board.TurnColor == PlayerColors.Black)
#else
			if (Simulator.Frame.Board.TurnColor == PlayerColors.White)
#endif
			{
				float actTime = Configs.Random.Next(4, TurnTime);

				if (Simulator.Frame.Board.BlackPlayer.MoveCount == 0)
					actTime = 0;

				int turnNumber = Simulator.Frame.Board.TurnNumber;

				ScheduleWokerFor(actTime, () =>
				{
					CheckTurnTime(turnNumber);
				});
			}
			else
				base.ScheduleCheckTurnTime();
		}
	}
}