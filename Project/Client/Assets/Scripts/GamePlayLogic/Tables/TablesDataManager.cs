using System;
using Assets.Scripts.ClientUtilities.Extensions;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Networking.Client;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;

namespace Assets.Scripts.GamePlayLogic.Tables
{
    public class TablesDataManager : MonoBehaviorSingleton<TablesDataManager>
    {
        public struct Table
        {
            public string Name
            {
                get;
                private set;
            }

            public string SpriteName
            {
                get;
                private set;
            }

            public ushort Enterance
            {
                get;
                private set;
            }
            public ushort XP
            {
                get;
                private set;
            }

            public ushort TurnTime
            {
                get;
                private set;
            }

            public ushort  Prize
            {
                get;
                private set;
            }

            public ushort UnlockLevel
            {
                get;
                private set;
            }
            public ushort ID
            {
                get;
                private set;
            }


            public Table(string name, string spriteName, ushort ID, ushort enterance, ushort xP, ushort turnTime, ushort prize ,ushort unlockLevel)
            {
                Name = name;
                SpriteName = spriteName;
                Enterance = enterance;
                XP = xP;
                TurnTime = turnTime;
                Prize = prize;
                UnlockLevel = unlockLevel;
                this.ID = ID;
            }
        }


        public Table[] Tables
        {
            get;
            private set;
        }

        private void InitilizeTables(uint Count)
        {
            if (Tables != null && Tables.Length != 0)
                return;
            Tables = new Table[Count];
        }

        public void FillTables(ISerializeArray Array)
        {
            GameAnalyticsManager.Instance.SendEvent("Table Data desrialize Begin");

            if (Array == null || Array.Count == 0)
                return;

            InitilizeTables(Array.Count);
            if (Tables == null || Tables.Length == 0)
                return;

            for(uint i= 0; i<Tables.Length;++i)
            {
                ISerializeObject obj = Array.Get<ISerializeObject>(i);
                string name = string.Empty;
                string spriteName = string.Empty;
                ushort enterance = ushort.MinValue;
                ushort xp = ushort.MinValue;
                ushort turnTime = ushort.MinValue;
                ushort prize = ushort.MinValue;
                ushort unlockLevel = ushort.MinValue;
                ushort ID = ushort.MinValue;
                if (obj.IsContains("Name"))
                    name = obj.Get<string>("Name");
                if (obj.IsContains("SpriteName"))
                    spriteName = obj.Get<string>("SpriteName");
                if (obj.IsContains("Bet"))
                    enterance = obj.Get<ushort>("Bet");
                if(obj.IsContains("XP"))
                    xp = obj.Get<ushort>("XP");
                if (obj.IsContains("TurnTime"))
                    turnTime = obj.Get<ushort>("TurnTime");
                if (obj.IsContains("Prize"))
                    prize = obj.Get<ushort>("Prize");
                if(obj.IsContains("UnlockLevel"))
                    unlockLevel = obj.Get<ushort>("UnlockLevel");
                if (obj.IsContains("ID"))
                    ID = obj.Get<ushort>("ID");
                Tables[i] = new Table(name,spriteName,ID,enterance,xp,turnTime,prize,unlockLevel);
            }

            GameAnalyticsManager.Instance.SendEvent("Table Data desrialize end");

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}