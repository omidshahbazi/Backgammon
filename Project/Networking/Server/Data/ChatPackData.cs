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

		public static uint GetChatCount(int SplitTestGroupID, int PackID)
		{
			ISerializeObject obj = GetChatPackData(SplitTestGroupID, PackID);
			if (obj == null)
				return 0;

			ISerializeArray arr = obj.Get<ISerializeArray>("Chat");
			if (arr == null)
				return 0;

			return arr.Count;
		}

		public static uint GetChatPackCount(int SplitTestGroupID)
		{
			ISerializeArray arr = GetChats(SplitTestGroupID);
			if (arr == null)
				return 0;

			return arr.Count;
		}

		public static int GetChatPackID(int SplitTestGroupID, uint PackIndex)
		{
			ISerializeArray arr = GetChats(SplitTestGroupID);
			if (arr == null)
				return 0;

			ISerializeObject obj = arr.Get<ISerializeObject>(PackIndex);
			if (obj == null)
				return 0;

			return obj.Get<int>("ID");
		}

		private static ISerializeArray GetChats(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			return obj.Get<ISerializeArray>("ChatPack");
		}

		private static ISerializeObject GetChatPackData(int SplitTestGroupID, int PackID)
		{
			ISerializeArray arr = GetChats(SplitTestGroupID);
			if (arr == null)
				return null;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject obj = arr.Get<ISerializeObject>(i);

				if (PackID != obj.Get<int>("ID"))
					continue;

				return obj;
			}

			return null;
		}
	}
}