using BeardedManStudios.Forge.Networking;
using System.Collections.Generic;

namespace Networking.Server
{
	class Player
	{
		public NetworkingPlayer NetworkingPlayer
		{
			get;
			set;
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

		public int Version
		{
			get;
			private set;
		}

		public bool IsConnected
		{
			get;
			set;
		}

		public Player(NetworkingPlayer NetworkingPlayer, int ID, int SplitTestGroupID, int Version)
		{
			this.NetworkingPlayer = NetworkingPlayer;
			this.ID = ID;
			this.SplitTestGroupID = SplitTestGroupID;
			this.Version = Version;
			IsConnected = true;
		}
	}

	class PlayerList : List<Player>
	{ }

	class NetworPlayerMap : Dictionary<NetworkingPlayer, Player>
	{ }
}
