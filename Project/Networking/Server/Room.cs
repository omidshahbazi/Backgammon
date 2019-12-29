#define SERIALIZE_FULL_STEP
//#define DEBUG_LOG
using System.Collections.Generic;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using Simulation.Data.Serialization;
using GameFramework.Common.Timing;
using Networking.Server.Data;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	abstract class Room : LogicObjects
	{
		private SessionSerializer serializer = null;
		private bool isPlayingAsBot = false;
		private bool isFinished = false;

		private int whitePlayerNoMoveTurnCount = 0;
		private int blackPlayerNoMoveTurnCount = 0;

		public int TableID
		{
			get;
			private set;
		}

		protected float TurnTime
		{
			get;
			private set;
		}

		protected BufferStream SendBuffer
		{
			get;
			private set;
		}

		protected PlayerList Players
		{
			get;
			private set;
		}

		protected abstract Player WhitePlayer
		{
			get;
		}

		protected abstract Player BlackPlayer
		{
			get;
		}

		protected int ReadyPlayerCount
		{
			get;
			set;
		}

		public abstract string BotPlayerInfo
		{
			get;
		}

		protected Simulator Simulator
		{
			get;
			private set;
		}

		public int GameID
		{
			get;
			private set;
		}

		public uint PLayerCount
		{
			get { return (uint)Players.Count; }
		}

		public int Seed
		{
			get;
			private set;
		}

		public Room(Application Application, int TableID, float TurnTime) :
			base(Application)
		{
			serializer = new SessionSerializer();

			this.TableID = TableID;
			this.TurnTime = TurnTime;

			SendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);

			Players = new PlayerList();
		}

		public virtual void Initialize()
		{
			GameID = CreateGame();
			Seed = GameID;

			Simulator = new Simulator();
			Simulator.Reset(Seed);
			Simulator.OnBoardToBoardMove += HandleOnBoardToBoardMove;
			Simulator.OnBarToBoardMove += HandleOnBarToBoardMove;
			Simulator.OnBoardToBarMove += HandleOnBoardToBarMove;
			Simulator.OnBearedOff += HandleOnBearOff;
			Simulator.OnTurnChanged += HandleOnTurnChanged;
			Simulator.OnGameFinished += HandleOnGameFinished;

			serializer.SerializeConfigState(Simulator.Config);
			serializer.SerializeInitialState(Simulator.Frame);

			InitializeGame();
		}

		public void HandleRequest(BufferStream Buffer, Player Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Room.GET_GAME_DATA)
			{
				HandleGetGameData(Player);
			}
			else if (command == Commands.Room.GET_FRAMES_DATA)
			{
				HandleGetFramesData(Player);
			}
			else if (command == Commands.Room.BOARD_TO_BOARD_MOVE)
			{
				int clientHash = Buffer.ReadInt32();
				Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());
				Identifier toIdentifier = new Identifier(Buffer.ReadInt32());
				HandleSimulationEvent(clientHash, new BoardToBoardMoveEvent(fromIdentifier, toIdentifier), Player, Buffer);
			}
			else if (command == Commands.Room.BAR_TO_BOARD_MOVE)
			{
				int clientHash = Buffer.ReadInt32();
				PlayerColors color = (PlayerColors)Buffer.ReadInt32();
				Identifier toIdentifier = new Identifier(Buffer.ReadInt32());
				HandleSimulationEvent(clientHash, new BarToBoardMoveEvent(color, toIdentifier), Player, Buffer);
			}
			else if (command == Commands.Room.BEAR_OFF)
			{
				int clientHash = Buffer.ReadInt32();
				Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());
				HandleSimulationEvent(clientHash, new BearOffEvent(fromIdentifier), Player, Buffer);
			}
			else if (command == Commands.Room.FINISH_TURN)
			{
				int clientHash = Buffer.ReadInt32();
				PlayerColors color = (PlayerColors)Buffer.ReadInt32();
				HandleSimulationEvent(clientHash, new FinishTurnEvent(color), Player, Buffer);
			}
			else if (command == Commands.Room.RESIGN)
			{
				HandleResign(Player);
			}
			else if (command == Commands.Room.SEND_CHAT)
			{
				SendToAll(Buffer, Player);
			}
		}

		public void HandlePlayerDisconnection(Player Player)
		{
			if (!isFinished)
				HandleGameFinisher(Player, GameFinishReasons.Disconnect);

			Players.Remove(Player);
		}

		public void AddPlayer(Player Player)
		{
			Players.Add(Player);
		}

		public bool ContainsPlayer(Player Player)
		{
			for (int i = 0; i < Players.Count; ++i)
			{
				if (Players[i].ID == Player.ID)
					return true;
			}

			return false;
		}

		public void GetStatistics(ISerializeObject Object)
		{
			Object.Set("TableID", TableID);
			Object.Set("GameID", GameID);
			Object.Set("PlayerCount", PLayerCount);

			for (int i = 0; i < Players.Count; ++i)
				Object.Set("Player " + i, Players[i].ID);
		}

		protected abstract int CreateGame();

		protected abstract void InitializeGame();

		protected virtual void HandleGetGameData(Player Player)
		{
#if DEBUG_LOG
			Log("HandleGetGameData " + Player.ID);
#endif

			if (ReadyPlayerCount == Players.Count)
			{
				ScheduleWokerFor(GeneralData.GetStartGameDelay(Player.SplitTestGroupID), () =>
				{
					SendStartTurn();
				});
			}
		}

		protected virtual void SimulateEvent(EventBase Event)
		{
			Simulator.SendEvent(Event);

#if SERIALIZE_FULL_STEP
			serializer.SerializeFullStep(Simulator.Frame);
#else
			serializer.SerializeStep(Simulator.Frame);
#endif

			if (Event.GetType() == EventBase.Types.FinishTurn)
				SendStartTurn();
		}

		public void HandleResign(Player Player)
		{
			if (isFinished)
				return;

			HandleGameFinisher(Player, GameFinishReasons.Resign);
		}

		protected virtual void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			if (Player == WhitePlayer)
				whitePlayerNoMoveTurnCount = 0;
			else if (Player == BlackPlayer)
				blackPlayerNoMoveTurnCount = 0;

			SimulateEvent(Event);

			if (ClientHash != Simulator.Frame.Hash)
			{
				HandleGameFinisher(Player, GameFinishReasons.Mismatch);

				return;
			}

			SendToAll(Buffer, Player);
		}

		protected void HandleFinishGame(PlayerColors WinnerColor, GameFinishReasons Reason)
		{
#if DEBUG_LOG
			Log("HandleFinishGame " + WinnerColor + " " + Reason);
#endif

			isFinished = true;

			Player winnerPlayer = null;
			if (WinnerColor == PlayerColors.White)
				winnerPlayer = WhitePlayer;
			else if (WinnerColor == PlayerColors.Black)
				winnerPlayer = BlackPlayer;

			RewardInfo reward = GetWinnerPrize(winnerPlayer);

			if (winnerPlayer != null)
				AddWinnerReward(winnerPlayer, reward);

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_GAME);
			SendBuffer.WriteInt32((int)WinnerColor);
			SendBuffer.WriteInt32((int)Reason);
			SendBuffer.WriteString(reward.Serialize().Content);
			SendToAll();

			ScheduleWokerFor(0.1F, () =>
			{
				DatabaseLayer.CloseGame(GameID, (winnerPlayer == null ? Constants.NULL_USER_ID : winnerPlayer.ID), Reason, serializer.Data);
				Application.Lobby.RemoveRoom(this);
			});
		}

		protected void HandleGameFinisher(Player Player, GameFinishReasons Reason)
		{
			Player winnerPlayer = null;
			PlayerColors color = PlayerColors.White;

			if (Player == WhitePlayer)
				winnerPlayer = BlackPlayer;
			else if (Player == BlackPlayer)
				winnerPlayer = WhitePlayer;

			if (winnerPlayer == WhitePlayer)
				color = PlayerColors.White;
			else if (winnerPlayer == BlackPlayer)
				color = PlayerColors.Black;

			HandleFinishGame(color, Reason);
		}

		protected void SendToAll(Player Except = null)
		{
			SendToAll(SendBuffer, Except);
		}

		protected void SendToAll(BufferStream Buffer, Player Except = null)
		{
			for (int i = 0; i < Players.Count; ++i)
				if (Except == null || Players[i].NetworkingPlayer != Except.NetworkingPlayer)
					Send(Players[i], Buffer);
		}

		protected void PlayAsBot(PlayerData Player)
		{
			isPlayingAsBot = true;

			SmartBotUtilities.PlayOneTurn(Simulator, Configs.Random, Player, serializer, true);

			if (!isFinished)
				SimulateEvent(new FinishTurnEvent(Simulator.Frame.Board.TurnColor));

			isPlayingAsBot = false;
		}

		protected virtual void ScheduleCheckTurnTime()
		{
			int turnNumber = Simulator.Frame.Board.TurnNumber;

			ScheduleWokerFor(TurnTime, () =>
			{
				CheckTurnTime(turnNumber);
			});
		}

		protected void CheckTurnTime(int ForTurnNumber)
		{
			if (Simulator.Frame.Board.TurnNumber == ForTurnNumber)
			{
				PlayerData player = Utilities.GetPlayer(Simulator.Frame.Board, Simulator.Frame.Board.TurnColor);

				PlayAsBot(player);

				if (isFinished)
				{
					Application.Lobby.RemoveRoom(this);
					return;
				}

				if (player.Color == PlayerColors.White && WhitePlayer != null)
				{
					++whitePlayerNoMoveTurnCount;

					if (whitePlayerNoMoveTurnCount == GeneralData.GetFinishGameIfNoMoveForTurns(WhitePlayer.SplitTestGroupID))
						HandleGameFinisher(WhitePlayer, GameFinishReasons.NoMove);
				}
				else if (player.Color == PlayerColors.Black && BlackPlayer != null)
				{
					++blackPlayerNoMoveTurnCount;

					if (blackPlayerNoMoveTurnCount == GeneralData.GetFinishGameIfNoMoveForTurns(BlackPlayer.SplitTestGroupID))
						HandleGameFinisher(BlackPlayer, GameFinishReasons.NoMove);
				}
			}
		}

		protected Player GetOpponent(Player Player)
		{
			if (WhitePlayer == Player)
				return BlackPlayer;

			return WhitePlayer;
		}

		private void HandleGetFramesData(Player Player)
		{
			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_FRAMES_DATA);

#if SERIALIZE_FULL_STEP
			SendBuffer.WriteBool(true);
#else
			SendBuffer.WriteBool(false);
#endif

			byte[] data = serializer.Data;
			SendBuffer.WriteUInt32((uint)data.Length);
			SendBuffer.WriteBytes(data);

			Send(Player, SendBuffer);
		}

		private void SendStartTurn()
		{
#if DEBUG_LOG
			Log("SendStartTurn " + Simulator.Frame.Board.TurnColor);
#endif

			double startTurnTime = Time.CurrentEpochTime;
			double endTurnTime = startTurnTime + TurnTime;

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.START_TURN);
			SendBuffer.WriteInt32((int)Simulator.Frame.Board.TurnColor);
			SendBuffer.WriteFloat64(startTurnTime);
			SendBuffer.WriteFloat64(endTurnTime);
			SendToAll(SendBuffer);

			ScheduleCheckTurnTime();
		}

		private void HandleOnBoardToBoardMove(Identifier From, Identifier To)
		{
			if (!isPlayingAsBot)
				return;

#if DEBUG_LOG
			Log("HandleOnBoardToBoardMove " + Simulator.Frame.Board.TurnColor + " " + From + " " + To);
#endif

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BOARD_TO_BOARD_MOVE);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32(From);
			SendBuffer.WriteInt32(To);

			SendToAll(SendBuffer);
		}

		private void HandleOnBarToBoardMove(Identifier To)
		{
			if (!isPlayingAsBot)
				return;

#if DEBUG_LOG
			Log("HandleOnBarToBoardMove " + Simulator.Frame.Board.TurnColor + " " + To);
#endif

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BAR_TO_BOARD_MOVE);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)Simulator.Frame.Board.TurnColor);
			SendBuffer.WriteInt32(To);

			SendToAll(SendBuffer);
		}

		private void HandleOnBoardToBarMove(Identifier From)
		{
			if (!isPlayingAsBot)
				return;

#if DEBUG_LOG
			Log("HandleOnBoardToBarMove " + Simulator.Frame.Board.TurnColor + " " + From);
#endif

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BOARD_TO_BAR_MOVE);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)Simulator.Frame.Board.TurnColor);
			SendBuffer.WriteInt32(From);

			SendToAll(SendBuffer);
		}

		private void HandleOnBearOff(Identifier From)
		{
			if (!isPlayingAsBot)
				return;

#if DEBUG_LOG
			Log("HandleOnBearOff " + Simulator.Frame.Board.TurnColor + " " + From);
#endif

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BEAR_OFF);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32(From);

			SendToAll(SendBuffer);
		}

		private void HandleOnTurnChanged(PlayerColors Color)
		{
			if (!isPlayingAsBot)
				return;

#if DEBUG_LOG
			Log("HandleOnTurnChanged " + Color);
#endif

			SendBuffer.ResetWrite();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)Color);

			SendToAll(SendBuffer);
		}

		private void HandleOnGameFinished(PlayerColors WinnerColor, int Score)
		{
			if (Score == ConfigData.NORMAL_WIN_SCORE)
				HandleFinishGame(WinnerColor, GameFinishReasons.Normal);
			else if (Score == ConfigData.GAMMON_WIN_SCORE)
				HandleFinishGame(WinnerColor, GameFinishReasons.Gammon);
			else if (Score == ConfigData.BACKGAMMON_WIN_SCORE)
				HandleFinishGame(WinnerColor, GameFinishReasons.Backgammon);
		}

		private void AddWinnerReward(Player WinnerPlayer, RewardInfo Reward)
		{
			DatabaseLayer.AddReward(WinnerPlayer.ID, Reward, Places.WinGame);
		}

		private RewardInfo GetWinnerPrize(Player Player)
		{
			int groupID = 0;

			if (Player != null)
				groupID = Player.SplitTestGroupID;
			else if (WhitePlayer != null)
				groupID = WhitePlayer.SplitTestGroupID;
			else if (BlackPlayer != null)
				groupID = BlackPlayer.SplitTestGroupID;

			return new RewardInfo(TableData.GetPrize(groupID, TableID), TableData.GetXP(groupID, TableID));
		}
	}

	class RoomList : List<Room>
	{ }
}