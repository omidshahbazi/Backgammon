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
		private PlayerColors botColor;

		protected override Player WhitePlayer
		{
			get { return (botColor == PlayerColors.Black ? (PLayerCount == 1 ? Players[0] : null) : null); }
		}

		protected override Player BlackPlayer
		{
			get { return (botColor == PlayerColors.White ? (PLayerCount == 1 ? Players[0] : null) : null); }
		}

		protected Player RealPlayer
		{
			get { return (botColor == PlayerColors.White ? BlackPlayer : WhitePlayer); }
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
			botColor = (Configs.Random.Next(0, 100) < GeneralData.GetChanceOfWhiteBot(RealPlayer.SplitTestGroupID) ? PlayerColors.White : PlayerColors.Black);

			ISerializeObject obj = BotPlayerInfoMaker.Make(RealPlayer.ID);

			if (obj != null)
				botPlayerInfo = obj.Content;

			base.Initialize();
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Bet, RealPlayer.Version);
		}

		protected override void InitializeGame()
		{
			if (botColor == PlayerColors.White)
				DatabaseLayer.InitializeGame(GameID, Constants.NULL_USER_ID, BlackPlayer.ID, BotPlayerInfo);
			else
				DatabaseLayer.InitializeGame(GameID, WhitePlayer.ID, Constants.NULL_USER_ID, BotPlayerInfo);
		}

		protected override void HandleGetGameData(Player Player)
		{
			++ReadyPlayerCount;

			base.HandleGetGameData(Player);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			SendBuffer.WriteInt32((int)(botColor == PlayerColors.White ? PlayerColors.Black : PlayerColors.White));

			Send(Player, SendBuffer);
		}

		protected override void ScheduleCheckTurnTime()
		{
			if (Simulator.Frame.Board.TurnColor == botColor)
			{
				float actTime = Configs.Random.Next(4, TurnTime);

				PlayerData player = Utilities.GetPlayer(Simulator.Frame.Board, botColor);

				if (player.MoveCount == 0)
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