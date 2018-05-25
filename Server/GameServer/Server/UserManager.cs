using System;
using System.Collections.Generic;

namespace GameServer.Server
{
	class UserManager
	{
		private static UserManager instance = null;

		public static UserManager Instance
		{
			get
			{
				if (instance == null)
					instance = new UserManager();

				return instance;
			}
		}

		private List<ClientInstance> clients = new List<ClientInstance>();

		public void AddUser(ClientInstance Client)
		{
			if (clients.Contains(Client))
				return;

			clients.Add(Client);
		}

		public void RemoveUser(ClientInstance Client)
		{
			clients.Remove(Client);
		}

		public ClientInstance GetByID(int ID)
		{
			for (int i = 0; i < clients.Count; ++i)
				if (clients[i].ID == ID && clients[i].ConnectionState == Photon.SocketServer.ConnectionState.Connected)
					return clients[i];

			return null;
		}

		public bool IsOnlineWithDifferentInstance(ClientInstance Client)
		{
			for (int i = 0; i < clients.Count; ++i)
			{
				ClientInstance instance = clients[i];

				if (instance.ID == Client.ID && instance != Client && instance.ConnectionState == Photon.SocketServer.ConnectionState.Connected)
					return true;
			}

			return false;

		}
	}
}