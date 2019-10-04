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

		public int SplitTestGroupID
		{
			get;
			private set;
		}

		public Player(NetworkingPlayer NetworkingPlayer, int ID, int SplitTestGroupID)
		{
			this.NetworkingPlayer = NetworkingPlayer;
			this.ID = ID;
			this.SplitTestGroupID = SplitTestGroupID;
		}
	}

	class PlayerList : List<Player>
	{ }

	class NetworPlayerMap : Dictionary<NetworkingPlayer, Player>
	{ }
}
