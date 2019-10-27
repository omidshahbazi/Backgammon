#define ALSO_SIMULTATE
using System;
using System.Threading;
using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using System.Diagnostics;

#if ALSO_SIMULTATE
using Simulation.Data.Event;
using Simulation.Logic;
#endif

namespace Test
{
	class Program
	{
		private static Network network = null;

#if ALSO_SIMULTATE
		private static Simulator simulator = null;
#endif

		static void Main(string[] args)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

			network = new Network();

			network.OnConnected += Network_OnConnected;
			network.OnAuthenticationRespond += Network_OnAuthenticationRespond;
			network.OnJoinedToRoom += Network_OnJoinedToRoom;
			network.OnGameDataReady += Network_OnGameDataReady;
			network.OnBoardToBoardMoved += Network_OnBoardToBoardMoved;
			network.OnBarToBoardMoved += Network_OnBarToBoardMoved;
			network.OnBoardToBarMoved += Network_OnBoardToBarMoved;
			network.OnBearedOff += Network_OnBearedOff;
			network.OnTurnStarted += Network_OnTurnStarted;
			network.OnTurnFinished += Network_OnTurnFinished;
			network.OnGameFinished += Network_OnGameFinished;

			network.Connect();

			//network.PacketLossSimulation = 0.5F;
			//network.LatencySimulation = 500;

#if ALSO_SIMULTATE
			simulator = new Simulator();
#endif

			while (true)
			{
				Thread.Sleep(10);

				network.Service();
			}
		}

		private static void Network_OnConnected()
		{
			Console.WriteLine("Network_OnConnected");

			network.Authenticate(Guid.NewGuid().ToString(), Markets.Windows, 11);
		}

		private static void Network_OnAuthenticationRespond(AuthenticateResults Result, int ID)
		{
			Console.WriteLine("Network_OnAuthenticationRespond " + Result + " " + ID);

			network.GetDailyReward();

			network.JoinToRoom(500, true);
		}

		private static void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
		{
			Console.WriteLine("Network_OnJoinedToRoom " + GameID + " " + OtherPlayerInfo);

			network.GetGameData();

#if ALSO_SIMULTATE
			simulator.Reset(GameID);
#endif
		}

		private static void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			Console.WriteLine("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);

#if ALSO_SIMULTATE
			SendEvent(new BoardToBoardMoveEvent(FromIdentifier, ToIdentifier), Hash);
#endif
		}

		private static void Network_OnBarToBoardMoved(int Hash, PlayerColors Color, Identifier ToIdentifier)
		{
			Console.WriteLine("Network_OnBarToBoardMoved " + Hash + " " + Color + " " + (int)ToIdentifier);

#if ALSO_SIMULTATE
			SendEvent(new BarToBoardMoveEvent(Color, ToIdentifier), Hash);
#endif
		}

		private static void Network_OnBoardToBarMoved(int Hash, PlayerColors Color, Identifier ToIdentifier)
		{
			Console.WriteLine("Network_OnBoardToBarMoved " + Hash + " " + Color + " " + (int)ToIdentifier);
		}

		private static void Network_OnBearedOff(int Hash, Identifier FromIdentifier)
		{
			Console.WriteLine("Network_OnBearedOff " + Hash + " " + (int)FromIdentifier);

#if ALSO_SIMULTATE
			SendEvent(new BearOffEvent(FromIdentifier), Hash);
#endif
		}

		private static void Network_OnTurnStarted(PlayerColors Color, double StartTime, double EndTime)
		{
			Console.WriteLine("Network_OnTurnStarted " + Color + " " + StartTime + " " + EndTime);
		}

		private static void Network_OnTurnFinished(int Hash, PlayerColors Color)
		{
			Console.WriteLine("Network_OnTurnFinished " + Hash + " " + Color);

#if ALSO_SIMULTATE
			SendEvent(new FinishTurnEvent(Color), Hash);
#endif
		}

		private static void Network_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, RewardInfo Reward)
		{
			Console.WriteLine("Network_OnGameFinished " + WinnerColor + " " + Reason);
		}

		private static void Network_OnGameDataReady(PlayerColors Color)
		{
			Console.WriteLine("Network_OnGameDataReady " + Color);
		}

#if ALSO_SIMULTATE
		private static void SendEvent(EventBase Event, int SimulatedHash)
		{
			simulator.SendEvent(Event);

			System.Diagnostics.Debug.Assert(simulator.Frame.Hash == SimulatedHash);
		}
#endif
	}
}
