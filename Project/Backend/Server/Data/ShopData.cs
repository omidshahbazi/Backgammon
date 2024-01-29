using GameFramework.ASCIISerializer;
using Networking.Common;

namespace Networking.Server.Data
{
	static class ShopData
	{
		public static ISerializeObject GetPack(int SplitTestGroupID, Markets Market, int ID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			ISerializeArray arr = obj.Get<ISerializeArray>("Shop");
			if (arr == null)
				return null;

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject levelObj = arr.Get<ISerializeObject>(i);

				if (levelObj.Get<int>("Market") != (int)Market)
					continue;

				ISerializeArray packArr = levelObj.Get<ISerializeArray>("Pack");

				for (uint j = 0; j < packArr.Count; ++j)
				{
					ISerializeObject packObj = packArr.Get<ISerializeObject>(j);

					if (packObj.Get<int>("ID") != ID)
						continue;

					return packObj;
				}
			}

			return null;
		}
	}
}