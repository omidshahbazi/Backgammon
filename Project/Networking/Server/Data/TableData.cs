using GameFramework.ASCIISerializer;

namespace Networking.Server.Data
{
	static class TableData
	{
		public static uint GetXP(int SplitTestGroupID, uint Bet)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, Bet);
			if (obj == null)
				return 0;

			return obj.Get<uint>("XP");
		}

		public static float GetTurnTime(int SplitTestGroupID, uint Bet)
		{
			ISerializeObject obj = GetTableObject(SplitTestGroupID, Bet);
			if (obj == null)
				return 0;

			return obj.Get<float>("TurnTime");
		}

		private static ISerializeObject GetTableObject(int SplitTestGroupID, uint Bet)
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

				if (tableObj.Get<int>("Bet") != Bet)
					continue;

				return tableObj;
			}

			return null;
		}
	}
}