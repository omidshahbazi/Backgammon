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
			network.OnInitialDataReady += Network_OnInitialDataReady;
			network.OnJoinedToRoom += Network_OnJoinedToRoom;

			while (true)
			{
				Thread.Sleep(1);

				network.Service();
			}
		}

		private static void Network_OnJoinedToRoom(int GameID, int OtherPlayerID)
		{
			Console.WriteLine(OtherPlayerID);
		}

		private static void Network_OnInitialDataReady(string Data)
		{
		}

		private static void Network_OnAuthenticationRespond(AuthenticateResult Result, int ID, string Username)
		{
			network.JoinToRoom(100, false);
		}

		private static void Network_OnConnected()
		{
			network.Authenticate("", "");
		}
	}
}
