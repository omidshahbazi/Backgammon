using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using GameFramework.BinarySerializer;
using GameFramework.Common.FileLayer;
using Networking.Common;
using System;
using UnityEngine;

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

        public float StartGameDelay
        {
            get;
            private set;
        }


        public void DeserializeData()
        {
            GameAnalyticsManager.Instance.SendEvent("Whole Game data desrialize Begin");

            ISerializeObject OBJ = GameDataManager.initialDataObject;
            if (OBJ.IsContains("Table"))
                TablesDataManager.Instance.FillTables(OBJ.Get<ISerializeArray>("Table"));
            if (OBJ.IsContains("General"))
            {
                GameAnalyticsManager.Instance.SendEvent("General Data desrialize Begin");

                ISerializeObject GeneralOBJ = OBJ.Get<ISerializeObject>("General");
                if (GeneralOBJ.IsContains("WaitForMatch"))
                    WaitForMatch = GeneralOBJ.Get<float>("WaitForMatch");
                if (GeneralOBJ.IsContains("StartGameDelay"))
                    StartGameDelay = GeneralOBJ.Get<float>("StartGameDelay");

                GameAnalyticsManager.Instance.SendEvent("General Data desrialize end");

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

            BufferStream buffer = null;
            if (FileSystem.FileExists(INITIAL_DATA_FILE_NAME))
            {
                buffer = new BufferStream(FileSystem.ReadBytes(INITIAL_DATA_FILE_NAME));
                initialDataHash = buffer.ReadUInt32();
                initialDataObject = Creator.Create<ISerializeObject>(buffer.ReadString());
            }

            if (FileSystem.FileExists(STRINGS_FILE_NAME))
            {
                buffer = new BufferStream(FileSystem.ReadBytes(STRINGS_FILE_NAME));
                stringsHash = buffer.ReadUInt32();
                stringsObject = Creator.Create<ISerializeObject>(buffer.ReadString());
            }
        }

        public static void Update(Action OnFinished = null)
        {
            dataCount = 0;

            onFinished = OnFinished;
            RequestManager.Instance.Network.OnInitialDataReady += Network_OnInitialDataReady;
            RequestManager.Instance.Network.OnStringsReady += Network_OnStringsReady;

            RequestManager.Instance.Network.GetInitialData(initialDataHash);
            RequestManager.Instance.Network.GetStrings(stringsHash);
        }

        public static string GetString(string Key)
        {
            return stringsObject.Get<ISerializeObject>(UserInfoManager.Instance.User.Language.ToString()).Get<string>(Key);
        }

        private static void Network_OnInitialDataReady(Networking.Common.DataHashStatus Status, uint Hash, string Data)
        {
            if (Status != Networking.Common.DataHashStatus.OK)
            {
                initialDataHash = Hash;
                initialDataObject = Creator.Create<ISerializeObject>(Data);

                BufferStream buffer = new BufferStream(new byte[sizeof(uint) + (Data.Length * 2)]);
                buffer.WriteUInt32(Hash);
                buffer.WriteString(Data);
                FileSystem.Write(INITIAL_DATA_FILE_NAME, buffer.Buffer);
            }

            if (++dataCount == 2 && onFinished != null)
                onFinished();

        }

        private static void Network_OnStringsReady(Networking.Common.DataHashStatus Status, uint Hash, string Data)
        {
            if (Status != Networking.Common.DataHashStatus.OK)
            {
                stringsHash = Hash;
                stringsObject = Creator.Create<ISerializeObject>(Data);

                BufferStream buffer = new BufferStream(new byte[sizeof(uint) + (Data.Length * 2)]);
                buffer.WriteUInt32(Hash);
                buffer.WriteString(Data);
                FileSystem.Write(STRINGS_FILE_NAME, buffer.Buffer);
            }

            if (++dataCount == 2 && onFinished != null)
                onFinished();
        }
    }
}