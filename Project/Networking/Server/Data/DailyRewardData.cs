using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class DailyRewardData
	{
		public static RewardInfo GetTotalReward(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			obj = obj.Get<ISerializeObject>("DailyReward");
			if (obj == null)
				return null;

			obj = obj.Get<ISerializeObject>("TotalReward");
			if (obj == null)
				return null;

			RewardInfo reward = new RewardInfo();
			reward.Deserialize(obj);
			return reward;
		}
	}
}