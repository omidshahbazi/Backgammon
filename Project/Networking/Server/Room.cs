using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using Networking.Common;
using Zorvan.Framework.BinarySerializer;

namespace Networking.Server
{
	class Room : LogicObjects
	{
		public const int MAX_PLAYER_COUNT = 2;

		private BufferStream sendBuffer = null;
		private PlayerList players = null;

		private NetworkingPlayer whitePlayer = null;
		private NetworkingPlayer blackPlayer = null;

		public bool IsFull
		{
			get { return players.Count == MAX_PLAYER_COUNT; }
		}

		public Room(Application Application) :
			base(Application)
		{
			sendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);

			players = new PlayerList();
		}

		public void HandleRequest(BufferStream Buffer, Player Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Room.MOVE_CHECKER)
			{
				Send(Player, Buffer);
			}
			else if (command == Commands.Room.RESIGN)
			{
				Send(Player, Buffer);
			}
		}

		public void AddPlayer(Player Player)
		{
			players.Add(Player);

			sendBuffer.Reset();

			throw new System.Exception();
		}

		public void HandlePlayerDisconnection(NetworkingPlayer Player)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.RESIGN);

			if (Player == whitePlayer)
			{
				if (blackPlayer != null)
					Send(blackPlayer, sendBuffer);
			}
			else
			{
				if (whitePlayer != null)
					Send(whitePlayer, sendBuffer);
			}
		}

		public bool ContainsPlayer(NetworkingPlayer Player)
		{
			for (int i = 0; i < players.Count; ++i)
			{
				if (players[i].NetworkingPlayer.IPEndPointHandle == Player.IPEndPointHandle)
					return true;
			}

			return false;
		}

		private void SendToAll(NetworkingPlayer Except = null)
		{
			for (int i = 0; i < players.Count; ++i)
				if (players[i].NetworkingPlayer != Except)
					Send(players[i], sendBuffer);
		}

		public override string ToString()
		{
			return (whitePlayer == null ? "[No Player]" : whitePlayer.IPEndPointHandle.ToString()) + " vs. " + (blackPlayer == null ? "[No Player]" : blackPlayer.IPEndPointHandle.ToString());
		}
	}

	class RoomList : List<Room>
	{ }
}