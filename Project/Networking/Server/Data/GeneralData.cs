using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class GeneralData
	{
		public static uint GetStartGameDelay(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("StartGameDelay");
		}

		public static RewardInfo GetInitialResource(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return null;

			obj = obj.Get<ISerializeObject>("InitialResource");
			if (obj == null)
				return null;

			RewardInfo reward = new RewardInfo();
			reward.Deserialize(obj);
			return reward;
		}

		private static ISerializeObject GetGeneralObject(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			return obj.Get<ISerializeObject>("General");
		}
	}
}