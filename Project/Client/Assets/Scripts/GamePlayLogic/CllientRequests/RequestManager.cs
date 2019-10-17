using System;
using ClientUtilities.Singleton;
using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;

namespace Assets.Scripts.GamePlayLogic.RequestManager
{
	public class RequestManager : MonoBehaviorSingleton<RequestManager>
	{
		public Network Network
		{
			get;
			private set;
		}

		private bool isConnectionDestroyed = false;

		public void InitilizeNetwork()
		{
			if (Network == null)
			{
				Network = new Network();
				AddNetworkListeners();
			}

			if (!Network.IsConnected)
				Network.Connect();
		}

		private void Update()
		{
			if (isConnectionDestroyed)
				return;
			if (Network != null && Network.IsConnected)
				Network.Service();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			isConnectionDestroyed = true;
			DisconectNetwork();
		}

		private void AddNetworkListeners()
		{
			UnityEngine.Debug.Assert(Network != null, "Network instance is null");
			Network.OnConnected += Network_OnConnected;
			Network.OnAuthenticationRespond += Network_OnAuthenticationRespond;
			Network.OnJoinedToRoom += Network_OnJoinedToRoom;
			Network.OnGameDataReady += Network_OnGameDataReady;
			Network.OnBoardToBoardMoved += Network_OnBoardToBoardMoved;
			Network.OnTurnStarted += Network_OnTurnStarted;
			Network.OnTurnFinished += Network_OnTurnFinished;
			Network.OnGameFinished += Network_OnGameFinished;
		}


		private void DisconectNetwork()
		{
			UnityEngine.Debug.Assert(Network != null, "Network instance is null");
			Network.Disconnect();
			RemoveNetworkListeners();
			Network = null;
		}

		private void RemoveNetworkListeners()
		{
			UnityEngine.Debug.Assert(Network != null, "Network instance is null");
			Network.OnConnected -= Network_OnConnected;
			Network.OnAuthenticationRespond -= Network_OnAuthenticationRespond;
			Network.OnJoinedToRoom -= Network_OnJoinedToRoom;
			Network.OnGameDataReady -= Network_OnGameDataReady;
			Network.OnBoardToBoardMoved -= Network_OnBoardToBoardMoved;
			Network.OnTurnStarted -= Network_OnTurnStarted;
			Network.OnTurnFinished -= Network_OnTurnFinished;
			Network.OnGameFinished -= Network_OnGameFinished;
		}

		private void Network_OnConnected()
		{
			UnityEngine.Debug.Log("Connection Established");
			UnityEngine.Debug.Log("Authentication Begins");
			//To do correct the parameters later 
			Network.Authenticate(new Random().Next(100).ToString(), Markets.Windows, 11);

		}


		private void Network_OnAuthenticationRespond(AuthenticateResults Result, int ID, string Username)
		{
			switch (Result)
			{
				case AuthenticateResults.Passed:
					UnityEngine.Debug.Log("Authentication Passed" + Result + " " + Username + " " + ID);
					break;
				case AuthenticateResults.Banned:
					UnityEngine.Debug.Log("Authentication Banned");
					break;
				case AuthenticateResults.Deleted:
					UnityEngine.Debug.Log("Authentication Deleted");

					break;
				default:
					break;
			}
		}

		private void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
		{
			Console.WriteLine("Network_OnJoinedToRoom " + OtherPlayerInfo);

			Network.GetGameData();
		}

		private void Network_OnGameDataReady(PlayerColors Color)
		{
			UnityEngine.Debug.Log("Network_OnGameDataReady " + Color);
		}

		private void Network_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason)
		{
			UnityEngine.Debug.Log("Network_OnGameFinished " + WinnerColor + " " + Reason);

		}

		private void Network_OnTurnFinished(int Hash, PlayerColors Color)
		{
			UnityEngine.Debug.Log("Network_OnTurnFinished " + Hash + " " + Color);
		}

		private void Network_OnTurnStarted(PlayerColors Color, double StartTime, double EndTime)
		{
			UnityEngine.Debug.Log("Network_OnTurnStarted " + Color + " " + StartTime + " " + EndTime);
		}

		private void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			UnityEngine.Debug.Log("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);
		}
	}
}