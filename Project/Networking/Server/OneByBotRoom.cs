using Networking.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	class OneByBotRoom : Room
	{
		private string botPlayerInfo;

		protected override Player WhitePlayer
		{
			get { return Players[0]; }
		}

		protected override Player BlackPlayer
		{
			get { return null; }
		}

		public override string BotPlayerInfo
		{
			get { return botPlayerInfo; }
		}

		public OneByBotRoom(Application Application, uint TableEnterance, float TurnTime) :
			base(Application, TableEnterance, TurnTime)
		{
		}

		public override void Initialize()
		{
			ISerializeObject obj = BotPlayerInfoMaker.Make(WhitePlayer.ID);

			if (obj != null)
				botPlayerInfo = obj.Content;

			base.Initialize();
		}

		protected override void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			Simulator.SendEvent(Event);

			SerializeStep();

			if (ClientHash != Simulator.Frame.Hash)
			{
				HandleGameFinisher(Player, GameFinishReasons.Mismatch);

				return;
			}

			if (Event.GetType() == EventBase.Types.FinishTurn)
				HandleBotTurn();
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Enterance);
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

		protected override void AddWinnerReward(Player WinnerPlayer, RewardInfo Reward)
		{
			DatabaseLayer.AddReward(WinnerPlayer.ID, Reward);
		}

		private void HandleBotTurn()
		{
			PlayerData player = Utilities.GetPlayer(Simulator.Frame.Board, Simulator.Frame.Board.TurnColor);

			BotUtilities.PlayOneTurn(Simulator, Configs.Random, player);

			SendFinishTurnEvent();
		}

		private void SendBoardToBoardMoveEvent(MoveInfo Info)
		{
			Simulator.SendEvent(new BoardToBoardMoveEvent(Info.From.ID, Info.To.ID));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BOARD_TO_BOARD_MOVE);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32(Info.From.ID);
			SendBuffer.WriteInt32(Info.To.ID);

			SendToAll(SendBuffer);
		}

		private void SendBarToBoardMoveEvent(MoveInfo Info)
		{
			Simulator.SendEvent(new BarToBoardMoveEvent(Info.To.Color, Info.To.ID));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BAR_TO_BOARD_MOVE);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)Info.To.Color);
			SendBuffer.WriteInt32(Info.To.ID);

			SendToAll(SendBuffer);
		}

		private void SendBearOffEvent(MoveInfo Info)
		{
			Simulator.SendEvent(new BearOffEvent(Info.From.ID));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BEAR_OFF);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32(Info.From.ID);

			SendToAll(SendBuffer);
		}
	}
}