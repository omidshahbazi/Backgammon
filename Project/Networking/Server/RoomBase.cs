using System.Collections.Generic;
using System.Diagnostics;
using BeardedManStudios.Forge.Networking;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;

namespace Networking.Server
{
	class RoomBase : LogicObjects
	{
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
			private set;
		}

		protected Player BlackPlayer
		{
			get;
			private set;
		}

		protected Simulator Simulator
		{
			get;
			private set;
		}

		public RoomBase(Application Application, int GameID) :
			base(Application)
		{
			SendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);

			Players = new PlayerList();

			Simulator = new Simulator();
			Simulator.Reset(GameID);
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
				Send(Player, Buffer);
			}
		}

		public void AddPlayer(Player Player)
		{
			Players.Add(Player);
		}

		public void HandlePlayerDisconnection(Player Player)
		{
			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.RESIGN);

			SendToAll(Player);
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

		protected virtual void HandleGetGameData(Player Player)
		{
			if (WhitePlayer == null)
			{
				WhitePlayer = Players[0];

				if (Players.Count > 1)
					BlackPlayer = Players[1];
			}

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			if (Player == WhitePlayer)
				SendBuffer.WriteInt32((int)PlayerColors.White);
			else
				SendBuffer.WriteInt32((int)PlayerColors.Black);

			Send(Player, SendBuffer);
		}

		protected virtual void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			Simulator.SendEvent(Event);

			Debug.Assert(ClientHash == Simulator.Hash);

			SendToAll(Buffer, Player);
		}

		protected void FinishGame(PlayerColors Winner)
		{

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
	}

	class RoomList : List<RoomBase>
	{ }
}