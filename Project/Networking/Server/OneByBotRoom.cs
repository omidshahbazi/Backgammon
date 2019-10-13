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

		public OneByBotRoom(Application Application, uint TableEnterance) :
			base(Application, TableEnterance)
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
			BoardData board = Simulator.Frame.Board;
			PlayerColors color = board.TurnColor;
			PlayerData player = Utilities.GetPlayer(board, color);

			MoveInfo[] moves = null;
			while ((moves = Logic.GetPossibleBarToBoardMoves(board)) != null || moves.Length != 0)
				SendBarToBoardMoveEvent(moves[0]);

			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = board.Points[i];

				while ((moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID)) != null || moves.Length != 0)
					SendBoardToBoardMoveEvent(moves[0]);

				while ((moves = Logic.GetPossibleBearedOffs(board, fromPoint.ID)) != null || moves.Length != 0)
					SendBearOffEvent(moves[0]);
			}

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

		private void SendFinishTurnEvent()
		{
			PlayerColors color = Simulator.Frame.Board.TurnColor;
			Simulator.SendEvent(new FinishTurnEvent(color));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)color);

			SendToAll(SendBuffer);
		}
	}
}