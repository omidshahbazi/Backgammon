using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
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
			network.OnBoardToBoardMoved += Network_OnBoardToBoardMoved;

			while (true)
			{
				Thread.Sleep(1);

				network.Service();
			}
		}

		private static void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			Console.WriteLine("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);
		}

		private static void Network_OnJoinedToRoom(int GameID, int OtherPlayerID)
		{
			Console.WriteLine("Network_OnJoinedToRoom " + OtherPlayerID);

			network.FinishTurn(1, PlayerColors.Black);
		}

		private static void Network_OnInitialDataReady(string Data)
		{
			Console.WriteLine(Data);
		}

		private static void Network_OnAuthenticationRespond(AuthenticateResult Result, int ID, string Username)
		{
			Console.WriteLine("Network_OnAuthenticationRespond " + Result + " " + Username + " " + ID);

			network.JoinToRoom(100, true);
		}

		private static void Network_OnConnected()
		{
			Console.WriteLine("Network_OnConnected");

			network.Authenticate("", "");
		}
	}
}
