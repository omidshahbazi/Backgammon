using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class ChatPackData
	{
		public static CostInfo GetChatPackCost(int SplitTestGroupID, int PackID)
		{
			ISerializeObject obj = GetChatPackData(SplitTestGroupID, PackID);
			if (obj == null)
				return null;

			obj = obj.Get<ISerializeObject>("Cost");
			if (obj == null)
				return null;

			CostInfo cost = new CostInfo();
			cost.Deserialize(obj);
			return cost;
		}

		public static RewardInfo GetChatPackReward(int SplitTestGroupID, int PackID)
		{
			ISerializeObject obj = GetChatPackData(SplitTestGroupID, PackID);
			if (obj == null)
				return null;

			obj = obj.Get<ISerializeObject>("Reward");
			if (obj == null)
				return null;

			RewardInfo cost = new RewardInfo();
			cost.Deserialize(obj);
			return cost;
		}

		private static ISerializeObject GetChatPackData(int SplitTestGroupID, int PackID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			ISerializeArray arr = obj.Get<ISerializeArray>("ChatPack");
			if (arr == null)
				return null;

			for (uint i = 0; i < arr.Count; ++i)
			{
				obj = arr.Get<ISerializeObject>(i);

				if (PackID != obj.Get<int>("ID"))
					continue;

				return obj;
			}

			return null;
		}
	}
}