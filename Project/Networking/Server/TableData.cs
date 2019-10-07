using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	static class TableData
	{
		public static uint GetXP(int SplitTestGroupID, uint Enterance)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupsInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return 0;

			ISerializeArray arr = obj.Get<ISerializeArray>("Table");
			if (arr == null)
				return 0;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject levelObj = arr.Get<ISerializeObject>(i);

				if (levelObj.Get<int>("Enterance") != Enterance)
					continue;

				return levelObj.Get<uint>("XP");
			}

			return 0;
		}
	}
}