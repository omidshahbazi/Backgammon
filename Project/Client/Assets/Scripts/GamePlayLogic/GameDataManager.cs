using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using GameFramework.BinarySerializer;
using GameFramework.Common.FileLayer;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{

    public class GameManager : MonoBehaviorSingleton<GameManager>
    {


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

            IsGameDataReady = true;
            GameAnalyticsManager.Instance.SendEvent("Whole Game data desrialize End");

        }



    }

    static class GameDataManager
    {

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
            if (FileSystem.FileExists("InitialData.bin"))
            {
                buffer = new BufferStream(FileSystem.ReadBytes("InitialData.bin"));
                initialDataHash = buffer.ReadUInt32();
                initialDataObject = Creator.Create<ISerializeObject>(buffer.ReadString());
            }
            if (FileSystem.FileExists("Strings.bin"))
            {
                buffer = new BufferStream(FileSystem.ReadBytes("Strings.bin"));
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
            return stringsObject.Get<string>(Key);
        }

        private static void Network_OnInitialDataReady(Networking.Common.DataHashStatus Status, uint Hash, string Data)
        {
            if (Status != Networking.Common.DataHashStatus.OK)
            {
                initialDataHash = Hash;
                initialDataObject = Creator.Create<ISerializeObject>(Data);
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
            }

            if (++dataCount == 2 && onFinished != null)
                onFinished();
        }
    }
}