using System.Collections.Generic;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using Simulation.Data.Serialization;
using GameFramework.Common.Timing;

namespace Networking.Server
{
	abstract class Room : LogicObjects
	{
		private const float START_GAME_DELAY = 2;

		private SessionSerializer serializer = null;
		private int lastScheduledTurnNumber = 0;

		protected uint Enterance
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

		public Room(Application Application, uint Enterance, float TurnTime) :
			base(Application)
		{
			serializer = new SessionSerializer();

			this.Enterance = Enterance;
			this.TurnTime = TurnTime;

			SendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);

			Players = new PlayerList();
		}

		public virtual void Initialize()
		{
			GameID = CreateGame();

			Simulator = new Simulator();
			Simulator.Reset(GameID);
			Simulator.OnGameFinished += HandleOnGameFinished;

			serializer.SerializeConfigState(Simulator.Config);
			serializer.SerializeInitialState(Simulator.Frame);

			DatabaseLayer.InitializeGame(GameID, WhitePlayer.ID, (BlackPlayer == null ? Constants.NULL_USER_ID : BlackPlayer.ID), BotPlayerInfo);

			lastScheduledTurnNumber = Simulator.Frame.Board.TurnNumber;
		}

		public void HandleRequest(BufferStream Buffer, Player Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Room.GET_GAME_DATA)
			{
				HandleGetGameData(Player);
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
				HandleGameFinisher(Player, GameFinishReasons.Resign);
			}
			else if (command == Commands.Room.SEND_CHAT)
			{
				SendToAll(Buffer, Player);
			}
		}

		public void HandlePlayerDisconnection(Player Player)
		{
			HandleGameFinisher(Player, GameFinishReasons.Disconnect);
		}

		public void AddPlayer(Player Player)
		{
			Players.Add(Player);
		}

		public bool ContainsPlayer(Player Player)
		{
			for (int i = 0; i < Players.Count; ++i)
			{
				if (Players[i].NetworkingPlayer.IPEndPointHandle == Player.NetworkingPlayer.IPEndPointHandle)
					return true;
			}

			return false;
		}

		protected abstract int CreateGame();

		protected virtual void HandleGetGameData(Player Player)
		{
			if (ReadyPlayerCount == Players.Count)
				ScheduleWokerFor(START_GAME_DELAY, StartTurn);
		}

		protected virtual void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			Simulator.SendEvent(Event);

			SerializeStep();

			if (ClientHash != Simulator.Frame.Hash)
			{
				HandleGameFinisher(Player, GameFinishReasons.Mismatch);

				return;
			}

			SendToAll(Buffer, Player);

			if (Event.GetType() == EventBase.Types.FinishTurn)
				StartTurn();
		}

		protected void HandleFinishGame(PlayerColors WinnerColor, GameFinishReasons Reason)
		{
			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_GAME);
			SendBuffer.WriteInt32((int)WinnerColor);
			SendBuffer.WriteInt32((int)Reason);

			SendToAll();

			Player winnerPlayer = null;
			if (WinnerColor == PlayerColors.White)
				winnerPlayer = WhitePlayer;
			else if (WinnerColor == PlayerColors.Black)
				winnerPlayer = WhitePlayer;

			DatabaseLayer.CloseGame(GameID, (winnerPlayer == null ? Constants.NULL_USER_ID : winnerPlayer.ID), Reason, serializer.Data);
		}

		protected void HandleGameFinisher(Player Player, GameFinishReasons Reason)
		{
			Player winnerPlayer = null;
			PlayerColors color = PlayerColors.White;

			if (Reason == GameFinishReasons.Normal)
				winnerPlayer = Player;
			else
			{
				if (Player == WhitePlayer)
					winnerPlayer = BlackPlayer;
				else if (Player == BlackPlayer)
					winnerPlayer = WhitePlayer;
			}

			if (winnerPlayer == WhitePlayer)
				color = PlayerColors.White;
			else if (winnerPlayer == BlackPlayer)
				color = PlayerColors.Black;

			HandleFinishGame(color, Reason);

			if (winnerPlayer != null)
				AddWinnerReward(winnerPlayer, GetWinnerReward(winnerPlayer));
		}

		protected abstract void AddWinnerReward(Player WinnerPlayer, RewardInfo Reward);

		protected void SerializeStep()
		{
			serializer.SerializeFullStep(Simulator.Frame);
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

		private void StartTurn()
		{
			if (Simulator.Frame.Board.TurnNumber != lastScheduledTurnNumber)
				return;

			PlayerColors turnColor = Simulator.Frame.Board.TurnColor;
			PlayerData playerData = (turnColor == PlayerColors.White ? Simulator.Frame.Board.WhitePlayer : Simulator.Frame.Board.BlackPlayer);

			BotUtilities.PlayOneTurn(Simulator, Configs.Random, playerData);

			int hash = Simulator.Frame.Hash;

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			SendBuffer.WriteInt32(hash);
			SendBuffer.WriteInt32((int)turnColor);

			Player player = (turnColor == PlayerColors.White ? WhitePlayer : BlackPlayer);
			HandleSimulationEvent(hash, new FinishTurnEvent(turnColor), player, SendBuffer);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.START_TURN);
			SendBuffer.WriteInt32((int)Simulator.Frame.Board.TurnColor);

			double startTurnTime = Time.CurrentEpochTime;
			double endTurnTime = startTurnTime + TurnTime;
			SendBuffer.WriteFloat64(startTurnTime);
			SendBuffer.WriteFloat64(endTurnTime);

			SendToAll(SendBuffer);

			lastScheduledTurnNumber = Simulator.Frame.Board.TurnNumber;

			ScheduleWokerFor(TurnTime, StartTurn);
		}

		private RewardInfo GetWinnerReward(Player Player)
		{
			return new RewardInfo((uint)((Enterance * 2) * 0.8F), TableData.GetXP(Player.SplitTestGroupID, Enterance));
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
	}

	class RoomList : List<Room>
	{ }
}