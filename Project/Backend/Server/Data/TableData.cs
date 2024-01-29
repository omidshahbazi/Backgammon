using GameFramework.ASCIISerializer;
using Networking.Common;

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

		public static RewardInfo GetPrize(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return null;

			RewardInfo reward = new RewardInfo();
			reward.Deserialize(obj.Get<ISerializeObject>("Prize"));

			return reward;
		}

		public static int GetTurnTime(int SplitTestGroupID, int TableID)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, TableID);
			if (obj == null)
				return 0;

			return obj.Get<int>("TurnTime");
		}

		private static ISerializeObject GetTableObject(int SplitTestGroupID, int TableID)
		{
			ISerializeArray arr = GetTablesArray(SplitTestGroupID);
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

		public static ISerializeArray GetTablesArray(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			return obj.Get<ISerializeArray>("Table");
		}
	}
}