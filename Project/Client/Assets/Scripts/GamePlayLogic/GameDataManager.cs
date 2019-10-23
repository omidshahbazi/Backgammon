using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Tables;
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

     


        public void DeserializeData()
        {
            if(GameDataManager.initialDataObject.Contains("Table"))
                TablesDataManager.Instance.FillTables(GameDataManager.initialDataObject.Get<ISerializeArray>("Table"));

            IsGameDataReady = true;
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

            if (!FileSystem.DirectoryExists("InitialData.bin"))
                return;
            BufferStream buffer = new BufferStream(FileSystem.ReadBytes("InitialData.bin"));
            initialDataHash = buffer.ReadUInt32();
            initialDataObject = Creator.Create<ISerializeObject>(buffer.ReadString());

            buffer = new BufferStream(FileSystem.ReadBytes("Strings.bin"));
            stringsHash = buffer.ReadUInt32();
            stringsObject = Creator.Create<ISerializeObject>(buffer.ReadString());
        }

        public static void Update(Action OnFinished = null)
        {
            dataCount = 0;

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