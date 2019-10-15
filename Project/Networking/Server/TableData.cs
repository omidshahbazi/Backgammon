using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	static class TableData
	{
		public static uint GetXP(int SplitTestGroupID, uint Enterance)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, Enterance);
			if (obj == null)
				return 0;

			return obj.Get<uint>("XP");
		}

		public static float GetTurnTime(int SplitTestGroupID, uint Enterance)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, Enterance);
			if (obj == null)
				return 0;

			return obj.Get<float>("TurnTime");
		}

		private static ISerializeObject GetTableObject(int SplitTestGroupID, uint Enterance)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			ISerializeArray arr = obj.Get<ISerializeArray>("Table");
			if (arr == null)
				return null;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject tableObj = arr.Get<ISerializeObject>(i);

				if (tableObj.Get<int>("Enterance") != Enterance)
					continue;

				return tableObj;
			}

			return null;
		}
	}
}