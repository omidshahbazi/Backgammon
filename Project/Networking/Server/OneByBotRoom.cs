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
		private int minBotTurnTime = 0;
		private int maxBotTurnTime = 0;

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

		public OneByBotRoom(Application Application, int TableID, int TurnTime) :
			base(Application, TableID, TurnTime)
		{
		}

		public override void Initialize()
		{
			botColor = (Configs.Random.Next(0, 100) < GeneralData.GetChanceOfWhiteBot(RealPlayer.SplitTestGroupID) ? PlayerColors.White : PlayerColors.Black);

			ISerializeObject obj = BotPlayerInfoMaker.Make(RealPlayer.ID);

			if (obj != null)
				botPlayerInfo = obj.Content;

			minBotTurnTime = GeneralData.GetMinBotTurnTime(RealPlayer.SplitTestGroupID);
			maxBotTurnTime = GeneralData.GetMaxBotTurnTime(RealPlayer.SplitTestGroupID);
			if (maxBotTurnTime == 0)
				maxBotTurnTime = TurnTime;
			else
				maxBotTurnTime = System.Math.Min(maxBotTurnTime, TurnTime);

			base.Initialize();
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, TableID, RealPlayer.Version);
		}

		protected override void InitializeGame()
		{
			if (botColor == PlayerColors.White)
				DatabaseLayer.InitializeGame(GameID, Constants.NULL_USER_ID, BlackPlayer.ID, BotPlayerInfo);
			else
				DatabaseLayer.InitializeGame(GameID, WhitePlayer.ID, Constants.NULL_USER_ID, BotPlayerInfo);
		}

		protected override void ScheduleCheckTurnTime()
		{
			BoardData board = Simulator.Frame.Board;

			if (board.TurnColor == botColor)
			{
				float actTime = Configs.Random.Next((int)minBotTurnTime, (int)maxBotTurnTime);

				PlayerData player = Utilities.GetPlayer(board, botColor);

				if (player.MoveCount == 0 ||
					Logic.GetTotalPossibleMoveCount(board) <= player.MoveCount)
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