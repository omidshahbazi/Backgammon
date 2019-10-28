using System;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.ResourceManager;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Assets.Scripts.GamePlayLogic.RequestManagers
{
	public delegate void OnGameDataReady(PlayerColors Color);
	public delegate void UserAuthenticatedReslult(AuthenticateResults Result, int ID);
	public delegate void InitialDataFilled();
    public delegate void MatchFound();
	public class RequestManager : MonoBehaviorSingleton<RequestManager>
	{
		public event UserAuthenticatedReslult OnAuthenticated = null;
		public event OnGameDataReady OnGameDataReady = null;
        public event MatchFound OnMatchFound = null;

		public Network Network
		{
			get;
			private set;
		}

		public bool IsAuthenticated
		{
			get;
			private set;
		}

		private bool isConnectionDestroyed = false;

		public void InitilizeNetwork()
		{
			if (Network == null)
            {

                GameAnalyticsManager.Instance.Initilize();
                Instantiate(GameResourceManager.Instance.LoadPrefab("IngameDebugConsole"));
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
			Network.OnBoardToBarMoved += Network_OnBoardToBarMoved;
			Network.OnBearedOff += Network_OnBearedOff;
			Network.OnBarToBoardMoved += Network_OnBarToBoardMoved;
			Network.OnTurnStarted += Network_OnTurnStarted;
			Network.OnTurnFinished += Network_OnTurnFinished;
			Network.OnGameFinished += Network_OnGameFinished;
         
        
			//Network.OnInitialDataReady += Network_OnInitialDataReady;
		}

 
     

        private void RemoveNetworkListeners()
		{
			UnityEngine.Debug.Assert(Network != null, "Network instance is null");
			Network.OnConnected -= Network_OnConnected;

			Network.OnAuthenticationRespond -= Network_OnAuthenticationRespond;
			Network.OnJoinedToRoom -= Network_OnJoinedToRoom;
			Network.OnGameDataReady -= Network_OnGameDataReady;
			Network.OnBoardToBoardMoved -= Network_OnBoardToBoardMoved;
			Network.OnBoardToBarMoved -= Network_OnBoardToBarMoved;
			Network.OnBearedOff -= Network_OnBearedOff;
			Network.OnBarToBoardMoved += Network_OnBarToBoardMoved;
			Network.OnTurnStarted -= Network_OnTurnStarted;
			Network.OnTurnFinished -= Network_OnTurnFinished;
			Network.OnGameFinished -= Network_OnGameFinished;
            //Network.OnInitialDataReady -= Network_OnInitialDataReady;
        }

        //private void Network_OnInitialDataReady(DataHashStatus Status, uint Hash, string Data)
        //{
        //	ISerializeObject Object = Creator.Create<ISerializeObject>(Data);
        //	if (Object.Contains("Table"))
        //	{
        //		TablesManager.Instance.FillTables(Object.Get<ISerializeArray>("Table"));
        //	}
        //}

        private void DisconectNetwork()
		{
			UnityEngine.Debug.Assert(Network != null, "Network instance is null");
			Network.Disconnect();
			RemoveNetworkListeners();
			Network = null;
		}

		private void Network_OnConnected()
		{
			UnityEngine.Debug.Log("Connection Established");
			UnityEngine.Debug.Log("Authentication Begins");
            //To do correct the parameters later 
            Network.Authenticate(UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString(), Market.Instance.market,11);

		}



		private void Network_OnAuthenticationRespond(AuthenticateResults Result, int ID)
		{
			switch (Result)
			{
				case AuthenticateResults.Passed:
					IsAuthenticated = true;
                    //Network.GetInitialData();
                    GameAnalyticsManager.Instance.SetUserID(ID);
                    GameAnalyticsManager.Instance.SendEvent("Authentication Passed");
					GameDataManager.Update(()=>GameManager.Instance.DeserializeData());
                    UserInfoManager.Instance.UpdateUserInfo(ID);

                    UnityEngine.Debug.Log("Authentication Passed" + Result +  + ID);
					break;
				case AuthenticateResults.Banned:
                    GameAnalyticsManager.Instance.SendEvent("Authentication Banned");
                    UnityEngine.Debug.Log("Authentication Banned");
					break;
				case AuthenticateResults.Deleted:
                    GameAnalyticsManager.Instance.SendEvent("Authentication Deleted");

                    UnityEngine.Debug.Log("Authentication Deleted");

					break;
				default:
					break;
			}

			OnAuthenticated?.Invoke(Result, ID);

		}

		private void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
		{
			Console.WriteLine("Network_OnJoinedToRoom " + OtherPlayerInfo);
            UserInfoManager.Instance.UpdateOpponnentInfo(OtherPlayerInfo);
            OnMatchFound?.Invoke();
			Network.GetGameData();
			SimulationManager.Instance.ResetGame(GameID);
		}

		private void Network_OnGameDataReady(PlayerColors Color)
		{
			OnGameDataReady?.Invoke(Color);
			UnityEngine.Debug.Log("Network_OnGameDataReady " + Color);
		}

		private void Network_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, RewardInfo Reward)
		{
			UnityEngine.Debug.Log("Network_OnGameFinished " + WinnerColor + " " + Reason);
		}


		private void Network_OnTurnFinished(int Hash, PlayerColors Color)
		{
			UnityEngine.Debug.Log("Network_OnTurnFinished " + Hash + " " + Color);


			TableManager.Instance.OnChangeTurn();
		}

		private void Network_OnTurnStarted(PlayerColors Color, double StartTime, double EndTime)
		{
			UnityEngine.Debug.Log("Network_OnTurnStarted " + Color + " " + StartTime + " " + EndTime);
		}

		private void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			TableManager.Instance.BoardToBoardMoveEvent(FromIdentifier, ToIdentifier);
			UnityEngine.Debug.Log("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);
		}

		private void Network_OnBoardToBarMoved(int Hash, PlayerColors Color, Identifier FromIdentifier)
		{
			// TableManager.Instance.BarToBoardMove(FromIdentifier);
			UnityEngine.Debug.Log("OnBoardToBarMoved" + Hash + " " + (int)FromIdentifier + " " + Color);

		}

		private void Network_OnBearedOff(int Hash, Identifier FromIdentifier)
		{
			TableManager.Instance.BearOff(FromIdentifier);
			UnityEngine.Debug.Log("Network_OnBearedOff" + Hash + " " + (int)FromIdentifier);

		}

		private void Network_OnBarToBoardMoved(int Hash, PlayerColors Color, Identifier ToIdentifier)
		{
			TableManager.Instance.BarToBoardMove(ToIdentifier);
			UnityEngine.Debug.Log("Network_OnBarToBoardMoved" + Hash + " " + (int)ToIdentifier + " " + Color);

		}

	}
}