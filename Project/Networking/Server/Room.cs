using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using Netowkring.Common;

namespace Netowkring.Server
{
	class Room : LogicObjects
	{
		public const int MAX_PLAYER_COUNT = 2;

		private BufferStream buffer = null;
		private List<NetworkingPlayer> players = null;

		private NetworkingPlayer whitePlayer = null;
		private NetworkingPlayer blackPlayer = null;

		public bool IsFull
		{
			get { return players.Count == MAX_PLAYER_COUNT; }
		}

		public Room(Application Application) :
			base(Application)
		{
			buffer = new BufferStream(new byte[64]);

			players = new List<NetworkingPlayer>();
		}

		public void HandleRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Room.GET_INITIAL_DATA)
			{
			}
			else if (command == Commands.Room.MOVE_CHECKER)
			{
			}
			else if (command == Commands.Room.RESIGN)
			{
			}
		}

		public void AddPlayer(NetworkingPlayer Player)
		{
			players.Add(Player);
		}

		public void HandlePlayerDisconnection(NetworkingPlayer Player)
		{
			buffer.Reset();
			buffer.WriteBytes(Commands.Category.ROOM, Commands.Room.RESIGN);

			if (Player == whitePlayer)
			{
				if (blackPlayer != null)
					Send(blackPlayer, buffer);
			}
			else
			{
				if (whitePlayer != null)
					Send(whitePlayer, buffer);
			}
		}

		public bool ContainsPlayer(NetworkingPlayer Player)
		{
			for (int i = 0; i < players.Count; ++i)
			{
				if (players[i].IPEndPointHandle == Player.IPEndPointHandle)
					return true;
			}

			return false;
		}

		private void SendToAll(NetworkingPlayer Except = null)
		{
			for (int i = 0; i < players.Count; ++i)
				if (players[i] != Except)
					Send(players[i], buffer);
		}

		public override string ToString()
		{
			return (whitePlayer == null ? "[No Player]" : whitePlayer.IPEndPointHandle.ToString()) + " vs. " + (blackPlayer == null ? "[No Player]" : blackPlayer.IPEndPointHandle.ToString());
		}
	}
}