using GameFramework.ASCIISerializer;

namespace Networking.Server.Data
{
	static class TableData
	{
		public static uint GetBet(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("Bet");
		}

		public static uint GetUnlockLevel(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("UnlockLevel");
		}

		public static uint GetPrize(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("Prize");
		}

		public static uint GetXP(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("XP");
		}

		public static float GetTurnTime(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<float>("TurnTime");
		}

		private static ISerializeObject GetTableObject(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			ISerializeArray arr = obj.Get<ISerializeArray>("Table");
			if (arr == null)
				return null;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject tableObj = arr.Get<ISerializeObject>(i);

				if (tableObj.Get<int>("ID") != TableID)
					continue;

				return tableObj;
			}

			return null;
		}
	}
}