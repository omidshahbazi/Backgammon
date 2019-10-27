using System;

namespace Networking.Admin
{
	class Application
	{
		private Network network = null;

		public Application()
		{
			network = new Network();

			network.OnConnected += Network_OnConnected;
			network.OnConnectionLost += Network_OnConnectionLost;
			network.OnConnectionRestored += Network_OnConnectionRestored;
			network.OnStatusDataReady += Network_OnStatusDataReady;

			//network.Connect("193.176.243.149", 433);
			network.Connect("127.0.0.1", 433);
		}

		public void Update()
		{
		}

		private void Network_OnConnected()
		{
			network.GetStatus();
		}

		private void Network_OnConnectionLost()
		{
		}

		private void Network_OnConnectionRestored()
		{
		}

		private void Network_OnStatusDataReady(string Data)
		{
			Console.WriteLine(Data);
		}
	}
}
