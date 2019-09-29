using BeardedManStudios.Forge.Networking;
using System.Collections.Generic;

namespace Networking.Server
{
	class Player
	{
		public NetworkingPlayer NetworkingPlayer
		{
			get;
			private set;
		}

		public int ID
		{
			get;
			private set;
		}

		public Player(NetworkingPlayer NetworkingPlayer, int ID)
		{
			this.NetworkingPlayer = NetworkingPlayer;
			this.ID = ID;
		}
	}

	class PlayerList : List<Player>
	{ }

	class NetworPlayerMap : Dictionary<NetworkingPlayer, Player>
	{ }
}
