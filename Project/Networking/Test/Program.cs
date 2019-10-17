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
			network.OnJoinedToRoom += Network_OnJoinedToRoom;
			network.OnGameDataReady += Network_OnGameDataReady;
			network.OnBoardToBoardMoved += Network_OnBoardToBoardMoved;
			network.OnTurnStarted += Network_OnTurnStarted;
			network.OnTurnFinished += Network_OnTurnFinished;
			network.OnGameFinished += Network_OnGameFinished;

			while (true)
			{
				Thread.Sleep(10);

				network.Service();
			}
		}

		private static void Network_OnConnected()
		{
			Console.WriteLine("Network_OnConnected");

			network.Authenticate(new Random().Next(100).ToString(), Markets.Windows, 11);
		}

		private static void Network_OnAuthenticationRespond(AuthenticateResults Result, int ID, string Username)
		{
			Console.WriteLine("Network_OnAuthenticationRespond " + Result + " " + Username + " " + ID);

			network.JoinToRoom(500, false);
		}

		private static void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
		{
			Console.WriteLine("Network_OnJoinedToRoom " + OtherPlayerInfo);

			network.GetGameData();
		}

		private static void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			Console.WriteLine("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);
		}

		private static void Network_OnTurnStarted(PlayerColors Color, double StartTime, double EndTime)
		{
			Console.WriteLine("Network_OnTurnStarted " + Color + " " + StartTime + " " + EndTime);
		}

		private static void Network_OnTurnFinished(int Hash, PlayerColors Color)
		{
			Console.WriteLine("Network_OnTurnFinished " + Hash + " " + Color);
		}

		private static void Network_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason)
		{
			Console.WriteLine("Network_OnGameFinished " + WinnerColor + " " + Reason);
		}

		private static void Network_OnGameDataReady(PlayerColors Color)
		{
			Console.WriteLine("Network_OnGameDataReady " + Color);
		}
	}
}
