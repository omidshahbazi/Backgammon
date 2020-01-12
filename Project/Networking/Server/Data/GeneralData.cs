using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class GeneralData
	{
		public static uint GetWaitForRestoreSession(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("WaitForRestoreSession");
		}

		public static uint GetChanceOfWhiteBot(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("ChanceOfWhiteBot");
		}

		public static int GetMinBotTurnTime(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return System.Math.Max(obj.Get<int>("MinBotTurnTime"), 0);
		}

		public static int GetMaxBotTurnTime(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return System.Math.Max(obj.Get<int>("MaxBotTurnTime"), 0);
		}

		public static uint GetStartGameDelay(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("StartGameDelay");
		}

		public static uint GetStartTurnDelay(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("StartTurnDelay");
		}

		public static uint GetFinishGameIfNoMoveForTurns(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			return obj.Get<uint>("FinishGameIfNoMoveForTurns");
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

		public static bool GetGenerateRandomBotName(int SplitTestGroupID)
		{
			ISerializeObject obj = GetGeneralObject(SplitTestGroupID);
			if (obj == null)
				return true;

			return obj.Get<bool>("GenerateRandomBotName");
		}

		private static ISerializeObject GetGeneralObject(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			return obj.Get<ISerializeObject>("General");
		}
	}
}