using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class DailyRewardData
	{
		public static bool GetMinimumMaximumCoin(int SplitTestGroupID, int PlayerLevel, out int MinimumCoin, out int MaximumCoin)
		{
			MinimumCoin = 0;
			MaximumCoin = 0;

			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return false;

			ISerializeArray arr = obj.Get<ISerializeArray>("DailyReward");
			if (arr == null)
				return false;

			ISerializeObject levelObj = null;
			for (uint i = 0; i < arr.Count; ++i)
			{
				obj = arr.Get<ISerializeObject>(i);

				if (obj.Get<int>("Level") != PlayerLevel)
					continue;

				levelObj = obj;
			}

			if (levelObj == null)
				return false;

			MinimumCoin = levelObj.Get<int>("MinimumCoin");
			MaximumCoin = levelObj.Get<int>("MaximumCoin");

			return true;
		}
	}
}