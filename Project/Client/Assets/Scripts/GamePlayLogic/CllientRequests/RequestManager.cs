using System;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.GamePlayLogic.LeaderBoard;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.PushNotification;
using ClientUtilities.ResourceManager;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using AdTracker.SDK;

namespace Assets.Scripts.GamePlayLogic.RequestManagers
{
    public delegate void OnGameDataReady(PlayerColors Color);
    public delegate void UserAuthenticatedReslult(AuthenticateResults Result, int ID);
    public delegate void InitialDataFilled();
    public delegate void MatchFound();
    public class RequestManager : MonoBehaviorSingleton<RequestManager>
    {
        private const float PING_INTERVAL_INSIDE_THE_GAME = 2.0F;
        public event UserAuthenticatedReslult OnAuthenticated = null;
        public event OnGameDataReady OnGameDataReady = null;
        public event MatchFound OnMatchFound = null;

        public Network Network
        {
            get;
            private set;
        }

        public AdTrackerSDK AdTracker
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
        private ScheduleObj restartHandler;

        public void InitilizeNetwork()
        {
            UnityEngine.Screen.sleepTimeout = UnityEngine.SleepTimeout.NeverSleep;
            if (Network == null)
            {
                UnityEngine.Application.runInBackground = true;
                Network = new Network();
                if (UnityEngine.Debug.isDebugBuild)
                {
                    Instantiate(GameResourceManager.Instance.LoadPrefab("IngameDebugConsole"));
                    Network.IsDebugMode = true;
                }

                AddNetworkListeners();
            }

            if (!Network.IsConnected)
                Network.Connect();
        }


        private void Update()
        {
            if (isConnectionDestroyed)
                return;
            try
            {
                if (Network != null)// && Network.IsConnected
                    Network.Service();
#if UNITY_EDITOR
                if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Insert))
                    Network_OnConnectionLost();

                if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
                    Network.Disconnect();

#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

        }

        protected override void OnDestroy()
        {
            DisconectNetwork();
            isConnectionDestroyed = true;
            base.OnDestroy();
        }

        private void AddNetworkListeners()
        {
            UnityEngine.Debug.Assert(Network != null, "Network instance is null");
            Network.OnConnected += Network_OnConnected;
            Network.OnConnectionLost += Network_OnConnectionLost;
            Network.OnConnectionFailed += Network_OnConnectionFailed;
            //Network.OnConnectionRestored += Network_OnConnectionRestored;

            Network.OnVersionCheckRespond += Network_OnVersionCheckRespond;
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
            Network.OnRestoreSessionRespond += Network_OnRestoreSessionRespond;
            Network.OnFramesDataReady += Network_OnFramesDataReady;
            Network.OnChatPackBought += Network_OnChatPackBought;
            //Network.OnInitialDataReady += Network_OnInitialDataReady;
        }


        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.LoadScene("BackGammonMainScene");

        }

        private void RemoveNetworkListeners()
        {
            UnityEngine.Debug.Assert(Network != null, "Network instance is null");
            Network.OnConnected -= Network_OnConnected;
            Network.OnConnectionLost -= Network_OnConnectionLost;
            Network.OnConnectionFailed -= Network_OnConnectionFailed;
            //  Network.OnConnectionRestored -= Network_OnConnectionRestored;

            Network.OnAuthenticationRespond -= Network_OnAuthenticationRespond;
            Network.OnJoinedToRoom -= Network_OnJoinedToRoom;
            Network.OnGameDataReady -= Network_OnGameDataReady;
            Network.OnBoardToBoardMoved -= Network_OnBoardToBoardMoved;
            Network.OnBoardToBarMoved -= Network_OnBoardToBarMoved;
            Network.OnBearedOff -= Network_OnBearedOff;
            Network.OnBarToBoardMoved -= Network_OnBarToBoardMoved;
            Network.OnTurnStarted -= Network_OnTurnStarted;
            Network.OnTurnFinished -= Network_OnTurnFinished;
            Network.OnGameFinished -= Network_OnGameFinished;
            Network.OnVersionCheckRespond -= Network_OnVersionCheckRespond;
            Network.OnRestoreSessionRespond -= Network_OnRestoreSessionRespond;
            Network.OnFramesDataReady -= Network_OnFramesDataReady;
            Network.OnChatPackBought -= Network_OnChatPackBought;

        }


        private void DisconectNetwork()
        {
            if (Network != null)
            {

                Network.Disconnect();
                RemoveNetworkListeners();
                Network = null;
            }
        }

        private void Network_OnVersionCheckRespond(VersionCheckResults Result, string Link)
        {
            try
            {
                object state = (VersionCheckResults)Result;
                object url = (string)Link;
                switch (Result)
                {
                    case VersionCheckResults.UnderMaintenance:
                        UIManager.Instance.ShowUI("VersionCheckMenu", state);
                        break;
                    case VersionCheckResults.OK:
                        BeginAuthenticate();
                        break;
                    case VersionCheckResults.NewerVersionAvailable:

                        object onClick = (Action)(() => { BeginAuthenticate(); });
                        UIManager.Instance.ShowUI("VersionCheckMenu", state, url, onClick);
                        break;
                    case VersionCheckResults.UpdateNeeded:

                        UIManager.Instance.ShowUI("VersionCheckMenu", state, url);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void BeginAuthenticate()
        {
            try
            {
                UnityEngine.Debug.Log("Authentication Begins");

                Network.Authenticate(UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString(), ProjectConfigs.Instance.market, ProjectConfigs.Instance.VersionNumber);
                UnityEngine.Debug.Log(UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString() + "USER INFO");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnConnected()
        {
            try
            {
                UnityEngine.Debug.Log("Connection Established");
                UnityEngine.Debug.Log("Version check Begin Begins");
                //To do correct the parameters later 
                //Network.Authenticate(UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString(), ProjectConfigs.Instance.market, ProjectConfigs.Instance.VersionNumber);
                UnityEngine.Debug.Log("Version Number" + ProjectConfigs.Instance.VersionNumber);
                UnityEngine.Debug.Log("Market" + ProjectConfigs.Instance.market);

                Network.VersionCheck(ProjectConfigs.Instance.market, ProjectConfigs.Instance.VersionNumber);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }

        }

        private void Network_OnConnectionLost()
        {
            try
            {
                if (!TableManager.Instance.IsGameStarted)
                {
                    Restart();
                }
                else
                {
                    restartHandler = ScheduleManager.Instance.AddSchedule(Restart, GameManager.Instance.WaitForRestoreSession);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Restart()
        {
            UnityEngine.Debug.Log("Conection Lost");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.LoadScene("ReloadScene");
        }

        //private void Network_OnConnectionRestored()
        //{
        //    try
        //    {
        //       // Network_OnConnected();
        //        Network.RestoreSession();
        //    }
        //    catch (Exception e)
        //    {
        //        UnityEngine.Debug.LogAssertion(e);
        //    }
        //}

        private void Network_OnRestoreSessionRespond(SessionRestoreResults Result)
        {
            switch (Result)
            {
                case SessionRestoreResults.Done:
                    restartHandler.CancelSchedule();
                    Network.GetFramesData();
                    break;
                case SessionRestoreResults.Failed:
                    break;
                default:
                    break;
            }

        }


        private void Network_OnFramesDataReady(bool IsFullStep, byte[] Data)
        {
            SimulationManager.Instance.RestoreFrameData(IsFullStep, Data);
        }


        private void Network_OnConnectionFailed()
        {
            //Network_OnConnectionLost();
        }

        public void Resign()
        {
            try
            {
                Network.Resign();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnAuthenticationRespond(AuthenticateResults Result, int ID, bool IsNewUser)
        {
            try
            {
                switch (Result)
                {
                    case AuthenticateResults.Passed:
                        IsAuthenticated = true;
                        //Network.GetInitialData();
                        GameAnalyticsManager.Instance.SetUserID(ID);
                        GameAnalyticsManager.Instance.SendEvent("Authentication Passed");
                        UserInfoManager.Instance.UpdateUserInfo(ID, OnUserInfoDataComplete);


                        AdTracker = new AdTrackerSDK(UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString(), UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString(), UnityEngine.Application.identifier);
                        UnityEngine.Debug.Assert(AdTracker != null, "Ad Tracker is null");
                        string response = string.Empty;
                        if (IsNewUser)
                        {
                            AdTracker.SendInstallRequest(out response);
                        }
                        GameDataManager.Update(() =>
                        {
                            GameManager.Instance.DeserializeData();
                            GameManager.Instance.SetFrameRate = 30;
                        });


                        PushNotificationManager.Instance.Init();
                        UnityEngine.Debug.Log("Authentication Passed" + Result + +ID);
                        break;
                    case AuthenticateResults.Banned:
                        GameAnalyticsManager.Instance.SendEvent("Authentication Banned");
                        UnityEngine.Debug.Log("Authentication Banned");
                        break;
                    default:
                        break;
                }

                OnAuthenticated?.Invoke(Result, ID);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }

        }

        private void OnUserInfoDataComplete(UserInfo Info)
        {
            try
            {
                GameAnalyticsManager.Instance.Initilize(Info.SplitGroupName);
                GameAnalyticsSDK.GameAnalytics.SetBuildAllPlatforms(ProjectConfigs.Instance.Version);
                GameAnalyticsManager.Instance.SendCustomDimension(Info.SplitGroupName);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
        {
            try
            {
                Console.WriteLine("Network_OnJoinedToRoom " + OtherPlayerInfo);
                UserInfoManager.Instance.UpdateCurrentPlayerInfo(UserInfoManager.Instance.User);
                UserInfoManager.Instance.UpdateOpponnentInfo(OtherPlayerInfo);
                OnMatchFound?.Invoke();
                Network.GetGameData();
                SimulationManager.Instance.ResetGame(GameID);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnGameDataReady(PlayerColors Color)
        {
            try
            {
                Network.PingPeriod = PING_INTERVAL_INSIDE_THE_GAME;
                OnGameDataReady?.Invoke(Color);
                UnityEngine.Debug.Log("Network_OnGameDataReady " + Color);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, RewardInfo Reward)
        {
            try
            {
                Network.PingPeriod = Network.ReconnectionTime;
                SimulationManager.Instance.GameFinished(WinnerColor, Reason, (int)Reward.XP);
                UnityEngine.Debug.Log("Network_OnGameFinished " + WinnerColor + " " + Reason);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }


        private void Network_OnTurnFinished(int Hash, PlayerColors Color)
        {
            try
            {
                UnityEngine.Debug.Log("Network_OnTurnFinished " + Hash + " " + Color);
                TableManager.Instance.OnChangeTurn(true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnTurnStarted(PlayerColors Color, double StartTime, double EndTime)
        {
            try
            {
                UnityEngine.Debug.Log("Network_OnTurnStarted " + Color + " " + StartTime + " " + EndTime);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnBoardToBoardMoved(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
        {
            try
            {
                TableManager.Instance.BoardToBoardMoveEvent(FromIdentifier, ToIdentifier, true);
                UnityEngine.Debug.Log("Network_OnBoardToBoardMoved " + Hash + " " + (int)FromIdentifier + " " + (int)ToIdentifier);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }
        }

        private void Network_OnChatPackBought()
        {
            UserInfoManager.Instance.UpdateUserInfo((UserInfo) => ChatManager.Instance.UpdateChatStates());
        }


        private void Network_OnBoardToBarMoved(int Hash, PlayerColors Color, Identifier FromIdentifier)
        {
            // TableManager.Instance.BarToBoardMove(FromIdentifier);
            UnityEngine.Debug.Log("OnBoardToBarMoved" + Hash + " " + (int)FromIdentifier + " " + Color);

        }

        private void Network_OnBearedOff(int Hash, Identifier FromIdentifier)
        {
            try
            {
                TableManager.Instance.BearOff(FromIdentifier, true);
                UnityEngine.Debug.Log("Network_OnBearedOff" + Hash + " " + (int)FromIdentifier);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }

        }

        private void Network_OnBarToBoardMoved(int Hash, PlayerColors Color, Identifier ToIdentifier)
        {
            try
            {
                TableManager.Instance.BarToBoardMove(ToIdentifier, true);
                UnityEngine.Debug.Log("Network_OnBarToBoardMoved" + Hash + " " + (int)ToIdentifier + " " + Color);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogAssertion(e);
            }

        }
    }
}