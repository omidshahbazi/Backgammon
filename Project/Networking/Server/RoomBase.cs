using System.Collections.Generic;
using System.Diagnostics;
using BeardedManStudios.Forge.Networking;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using Simulation.Data.Serialization;

namespace Networking.Server
{
	abstract class RoomBase : LogicObjects
	{
		private SessionSerializer serializer = null;

		protected int GameID
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

		protected Player WhitePlayer
		{
			get;
			set;
		}

		protected Player BlackPlayer
		{
			get;
			set;
		}

		protected Simulator Simulator
		{
			get;
			private set;
		}

		public RoomBase(Application Application, int GameID) :
			base(Application)
		{
			serializer = new SessionSerializer();

			this.GameID = GameID;

			SendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);

			Players = new PlayerList();

			Simulator = new Simulator();
			Simulator.Reset(GameID);
			Simulator.OnGameFinished += HandleOnGameFinished;

			serializer.SerializeConfigState(Simulator.Config);
			serializer.SerializeInitialState(Simulator.Frame);
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
		}

		public void HandlePlayerDisconnection(Player Player)
		{
			HandleGameFinisher(Player, GameFinishReasons.Disconnect);
		}

		public void AddPlayer(Player Player)
		{
			Players.Add(Player);
		}

		public bool ContainsPlayer(NetworkingPlayer Player)
		{
			for (int i = 0; i < Players.Count; ++i)
			{
				if (Players[i].NetworkingPlayer.IPEndPointHandle == Player.IPEndPointHandle)
					return true;
			}

			return false;
		}

		protected abstract void HandleGetGameData(Player Player);

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

			DatabaseLayer.CloseGame(
				GameID,
				WhitePlayer.ID,
				(BlackPlayer == null ? Constants.NULL_PLAYER_ID : BlackPlayer.ID),
				(winnerPlayer == null ? Constants.NULL_PLAYER_ID : winnerPlayer.ID),
				Reason,
				serializer.Data);
		}

		protected virtual void HandleGameFinisher(Player Player, GameFinishReasons Reason)
		{
			Player winnerPlayer = null;

			if (Player == WhitePlayer)
			{
				winnerPlayer = BlackPlayer;
				HandleFinishGame(PlayerColors.Black, GameFinishReasons.Mismatch);
			}
			else if (Player == BlackPlayer)
			{
				winnerPlayer = WhitePlayer;
				HandleFinishGame(PlayerColors.White, GameFinishReasons.Mismatch);
			}
		}

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

		private void HandleOnGameFinished(PlayerColors WinnerColor, int Score)
		{
			HandleFinishGame(WinnerColor, GameFinishReasons.Normal);
		}
	}

	class RoomList : List<RoomBase>
	{ }
}