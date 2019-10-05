using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	static class LevelData
	{
		public static int GetLevelCap(int SplitTestGroupID, int Level)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return -1;

			ISerializeArray levelArr = obj.Get<ISerializeArray>("Level");
			if (levelArr == null)
				return -1;

			for (uint i = 0; i < levelArr.Count; ++i)
			{
				ISerializeObject levelObj = levelArr.Get<ISerializeObject>(i);

				if (levelObj.Get<int>("Level") != Level)
					continue;

				return levelObj.Get<int>("Cap");
			}

			return -1;
		}
	}
}