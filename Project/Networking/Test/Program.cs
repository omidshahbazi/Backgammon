using Networking.Client;
using Networking.Common;
using System;
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
			network.OnAuthenticationRespond += Network_OnAuthenticationRespond;

			while (true)
			{
				Thread.Sleep(1);

				network.Service();
			}
		}

		private static void Network_OnAuthenticationRespond(AuthenticateResult Result, int ID, string Username)
		{
			throw new NotImplementedException();
		}

		private static void Network_OnConnected()
		{
			network.Authenticate("", "");
		}
	}
}
