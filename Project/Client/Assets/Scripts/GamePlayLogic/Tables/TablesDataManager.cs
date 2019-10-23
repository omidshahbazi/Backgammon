using System;
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

            public Table(string name, string spriteName, ushort enterance, ushort xP, ushort turnTime) : this()
            {
                Name = name;
                SpriteName = spriteName;
                Enterance = enterance;
                XP = xP;
                TurnTime = turnTime;
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
                if (obj.Contains("Name"))
                    name = obj.Get<string>("Name");
                if (obj.Contains("SpriteName"))
                    spriteName = obj.Get<string>("SpriteName");
                if (obj.Contains("Bet"))
                    enterance = obj.Get<ushort>("Bet");
                if(obj.Contains("XP"))
                    xp = obj.Get<ushort>("XP");
                if (obj.Contains("TurnTime"))
                    turnTime = obj.Get<ushort>("TurnTime");

                Tables[i] = new Table(name,spriteName,enterance,xp,turnTime);
            }
        }
    }
}