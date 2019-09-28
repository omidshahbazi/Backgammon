using Networking.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Test
{
	class Program
	{
		private static Network network = null;

		static void Main(string[] args)
		{
			network = new Network();
			network.Connect();

			network.OnConnected += Network_OnConnected;

			while (true)
			{
				Thread.Sleep(1);

				network.Service();
			}
		}

		private static void Network_OnConnected()
		{
			network.Authenticate("omid", "", "ali");
		}
	}
}
