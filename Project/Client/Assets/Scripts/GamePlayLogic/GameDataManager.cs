using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Shop;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using GameFramework.BinarySerializer;
using GameFramework.Common.FileLayer;
using Networking.Common;
using System;
using UnityEngine;
using static Assets.Scripts.GamePlayLogic.Tables.TablesDataManager;

namespace Assets.Scripts.GamePlayLogic
{

    public class GameManager : MonoBehaviorSingleton<GameManager>
    {
        public class DailyReward
        {
            public bool IsClaimed
            {
                get;
                private set;
            }

            public RewardInfo Reward
            {
                get;
                private set;
            }

            public long NextClaimTime
            {
                get;
                private set;
            }



            private Action<DailyReward> onComplete = null;

            public DailyReward(bool isClaimed, RewardInfo reward, long nextClaimTime)
            {
                IsClaimed = isClaimed;
                Reward = reward;
                NextClaimTime = nextClaimTime;
            }

            public DailyReward()
            {

            }

            private void Network_OnDailyRewardReady(bool IsClaimed, int Dice1, int Dice2, RewardInfo Reward, long NextClaimTime)
            {
                RequestManager.Instance.Network.OnDailyRewardReady -= Network_OnDailyRewardReady;
                DailyReward dt = new DailyReward(IsClaimed, Reward, NextClaimTime);
                onComplete.Invoke(dt);
                onComplete = null;

            }

            public void UpdateDailyRewardData(Action<DailyReward> OnComplete)
            {
                RequestManager.Instance.Network.GetDailyReward();

                RequestManager.Instance.Network.OnDailyRewardReady += Network_OnDailyRewardReady;
                onComplete = OnComplete;
            }
        }

        public DailyReward DailyRewardInfo
        {
            get;
            private set;
        }

        public Table SelectedTable
        {
            get;
            private set;
        }

        public bool IsGameDataReady
        {
            get;
            private set;
        }


        public float WaitForMatch
        {
            get;
            private set;
        }

        public float WaitForRestoreSession
        {
            get;
            private set;
        }


        public float StartGameDelay
        {
            get;
            private set;
        }

        public float StartTurnDelay
        {
            get;
            private set;
        }

        public bool ShowPrices
        {
            get;
            private set;
        }

        public bool GreaterDiceFirst
        {
            get;
            private set;
        }

        public int SetFrameRate
        {
            get
            {
                return Application.targetFrameRate;
            }

            set
            {
                Application.targetFrameRate = value;
            }
        }

     
        public void DeserializeData()
        {
            GameAnalyticsManager.Instance.SendEvent("Whole Game data desrialize Begin");
            Debug.Log("Whole Game data desrialize Begin");
            ISerializeObject OBJ = GameDataManager.initialDataObject;
            if (OBJ.IsContains("Table"))
                TablesDataManager.Instance.FillTables(OBJ.Get<ISerializeArray>("Table"));
            if (OBJ.IsContains("General"))
            {
                GameAnalyticsManager.Instance.SendEvent("General Data desrialize Begin");
                Debug.Log("General Data desrialize Begin");

                ISerializeObject GeneralOBJ = OBJ.Get<ISerializeObject>("General");
                if (GeneralOBJ.IsContains("WaitForMatch"))
                    WaitForMatch = GeneralOBJ.Get<float>("WaitForMatch");
                if (GeneralOBJ.IsContains("StartTurnDelay"))
                    StartTurnDelay = GeneralOBJ.Get<float>("StartTurnDelay");
                if (GeneralOBJ.IsContains("StartGameDelay"))
                    StartGameDelay = GeneralOBJ.Get<float>("StartGameDelay");
                if (GeneralOBJ.IsContains("ShowGambleFeatures"))
                    ShowPrices = GeneralOBJ.Get<bool>("ShowGambleFeatures");
                if (GeneralOBJ.IsContains("GreaterDiceFirst"))
                    GreaterDiceFirst = GeneralOBJ.Get<bool>("GreaterDiceFirst");
                if(GeneralOBJ.IsContains("WaitForRestoreSession"))
                  WaitForRestoreSession =  GeneralOBJ.Get<float>("WaitForRestoreSession");
                GameAnalyticsManager.Instance.SendEvent("General Data desrialize end");
                Debug.Log("General Data desrialize end");


            }

            if (OBJ.IsContains("Shop"))
            {
                ISerializeArray arr = OBJ.Get<ISerializeArray>("Shop");
                if (arr != null)
                {
                    for (uint i = 0; i < arr.Count; ++i)
                    {
                        ISerializeObject levelObj = arr.Get<ISerializeObject>(i);
                        if (levelObj.Get<int>("Market") != (int)ProjectConfigs.Instance.market)
                            continue;

                        if (levelObj.IsContains("Pack"))
                            ShopManager.Instance.FillPacks(levelObj.Get<ISerializeArray>("Pack"));

                    }
                }
            }

            if (OBJ.IsContains("Chat"))
            {
                ISerializeArray arr = OBJ.Get<ISerializeArray>("Chat");
                ChatManager.Instance.DeserializeSimpleChat(arr);
            }
            IsGameDataReady = true;
            GameAnalyticsManager.Instance.SendEvent("Whole Game data desrialize End");
            Debug.Log("Whole Game data desrialize End");

        }

        public void UpdateDailyReward(Action OnComplete)
        {
            if (DailyRewardInfo == null)
                DailyRewardInfo = new DailyReward();
            DailyRewardInfo.UpdateDailyRewardData(
                (DailyReward d) =>
                {
                    DailyRewardInfo = d;
                    OnComplete?.Invoke();
                    if (!d.IsClaimed)
                        UserInfoManager.Instance.UpdateUserInfo();
                });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }

    static class GameDataManager
    {
        private const string INITIAL_DATA_FILE_NAME = "InitialData.bin";
        private const string STRINGS_FILE_NAME = "Strings.bin";

        private static uint initialDataHash = 0;
        public static ISerializeObject initialDataObject
        {
            get;
            private set;
        }



        private static uint stringsHash = 0;
        public static ISerializeObject stringsObject
        {
            get;
            private set;
        }


        private static int dataCount = 0;
        private static Action onFinished = null;


        static GameDataManager()
        {
            FileSystem.DataPath = Application.dataPath + "\\..\\MemoryCard\\";

            Debug.Log(Application.dataPath + "Application Data path");
            Debug.Log(FileSystem.DataPath + "File System Path");
            GameAnalyticsManager.Instance.SendEvent("Game data manager Constructor called");
            Debug.Log("Game data manager Constructor called");
            BufferStream buffer = null;
            if (FileSystem.FileExists(INITIAL_DATA_FILE_NAME))
            {
                GameAnalyticsManager.Instance.SendEvent("Initial data exist");
                Debug.Log("Initial data exist");
                buffer = new BufferStream(FileSystem.ReadBytes(INITIAL_DATA_FILE_NAME));
                initialDataHash = buffer.ReadUInt32();
                initialDataObject = Creator.Create<ISerializeObject>(buffer.ReadString());
            }

            if (FileSystem.FileExists(STRINGS_FILE_NAME))
            {
                GameAnalyticsManager.Instance.SendEvent("String files exist");
                Debug.Log("String files exist");
                buffer = new BufferStream(FileSystem.ReadBytes(STRINGS_FILE_NAME));
                stringsHash = buffer.ReadUInt32();
                stringsObject = Creator.Create<ISerializeObject>(buffer.ReadString());
            }
        }

        public static void Update(Action OnFinished = null)
        {
            GameAnalyticsManager.Instance.SendEvent("Game data manager Update Called");
            Debug.Log("Game data manager Update Called");
            dataCount = 0;

            onFinished = OnFinished;
            RequestManager.Instance.Network.OnInitialDataReady += Network_OnInitialDataReady;
            RequestManager.Instance.Network.OnStringsReady += Network_OnStringsReady;

            RequestManager.Instance.Network.GetInitialData(initialDataHash);
            RequestManager.Instance.Network.GetStrings(stringsHash);
        }

        public static string GetString(string Key)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return string.Empty;


            string text = string.Empty;
            if (stringsObject.Get<ISerializeObject>(UserInfoManager.Instance.User.Language.ToString()).Contains(Key))
                text = stringsObject.Get<ISerializeObject>(UserInfoManager.Instance.User.Language.ToString()).Get<string>(Key);
            else
                text = "EMPTY TEXT";
            return text;
#else
            return stringsObject.Get<ISerializeObject>(UserInfoManager.Instance.User.Language.ToString()).Get<string>(Key);
#endif
        }

        private static void Network_OnInitialDataReady(Networking.Common.DataHashStatus Status, uint Hash, string Data)
        {
            RequestManager.Instance.Network.OnInitialDataReady -= Network_OnInitialDataReady;

            GameAnalyticsManager.Instance.SendEvent("Network_OnInitialDataReady");
            Debug.Log("Network_OnInitialDataReady");
            if (Status != Networking.Common.DataHashStatus.OK)
            {
                GameAnalyticsManager.Instance.SendEvent("Network_OnInitialDataReady status entered ");
                Debug.Log("Network_OnInitialDataReady status entered ");
                initialDataHash = Hash;
                initialDataObject = Creator.Create<ISerializeObject>(Data);

                Debug.Log("Initial Data object created");
                if (initialDataObject == null)
                    GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "Initial Data object is null");
                Debug.Assert(initialDataObject != null, "Initial Data object is null");

                //BufferStream buffer = new BufferStream(new byte[sizeof(uint) + (Data.Length * 2)]);
                //buffer.ResetWrite();
                //Debug.Log("Initial data buffer created");
                //buffer.WriteUInt32(Hash);
                //buffer.WriteString(Data);
                //Debug.Log("Initial data buffer writed");
                //if (buffer.Buffer == null)
                //	GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "Initial Data buffer.Buffer is null");
                //Debug.Assert(buffer.Buffer != null, "Initial Data buffer.Buffer is null");

                ////  FileSystem.Write(INITIAL_DATA_FILE_NAME, buffer.Buffer);

                GameAnalyticsManager.Instance.SendEvent("Network_OnInitialDataReady status is Ok");
                Debug.Log("Network_OnInitialDataReady status is Ok");
            }

            Debug.Log(dataCount + " Data count in initital data ");
            if (++dataCount == 2)
                onFinished?.Invoke();

        }

        private static void Network_OnStringsReady(Networking.Common.DataHashStatus Status, uint Hash, string Data)
        {
            RequestManager.Instance.Network.OnStringsReady -= Network_OnStringsReady;
            GameAnalyticsManager.Instance.SendEvent("Network_OnStringsReady");
            Debug.Log("Network_OnStringsReady");
            if (Status != Networking.Common.DataHashStatus.OK)
            {
                GameAnalyticsManager.Instance.SendEvent("Network_OnStringsReady status entered ");
                Debug.Log("Network_OnStringsReady status entered ");
                stringsHash = Hash;
                stringsObject = Creator.Create<ISerializeObject>(Data);
                Debug.Log("String Object Created");
                if (stringsObject == null)
                    GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "string object is null");
                Debug.Assert(stringsObject != null, "string object is null");

                //BufferStream buffer = new BufferStream(new byte[sizeof(uint) + (Data.Length * 2)]);
                //buffer.ResetWrite();
                //Debug.Log("string buffer created ");
                //buffer.WriteUInt32(Hash);
                //buffer.WriteString(Data);
                //Debug.Log("string data buffer writed");
                //Debug.Assert(buffer.Buffer != null, "string Data buffer.Buffer is null");
                //if (buffer.Buffer == null)
                //	GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "string Data buffer.Buffer is null");
                ////   FileSystem.Write(STRINGS_FILE_NAME, buffer.Buffer);

                GameAnalyticsManager.Instance.SendEvent("Network_OnStringsReady staus is ok");
                Debug.Log("Network_OnStringsReady status is ok");
            }

            Debug.Log(dataCount + " Data count in string data ");
            if (++dataCount == 2)
                onFinished?.Invoke();
        }
    }
}