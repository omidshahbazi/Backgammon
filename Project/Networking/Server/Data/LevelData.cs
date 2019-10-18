using GameFramework.ASCIISerializer;

namespace Networking.Server.Data
{
	static class LevelData
	{
		public static uint GetLevelCount(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			ISerializeArray arr = obj.Get<ISerializeArray>("Level");
			if (arr == null)
				return 0;

			return arr.Count;
		}

		public static uint GetLevelCap(int SplitTestGroupID, int Level)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			ISerializeArray arr = obj.Get<ISerializeArray>("Level");
			if (arr == null)
				return 0;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject levelObj = arr.Get<ISerializeObject>(i);

				if (levelObj.Get<int>("Level") != Level)
					continue;

				return levelObj.Get<uint>("Cap");
			}

			return 0;
		}
	}
}