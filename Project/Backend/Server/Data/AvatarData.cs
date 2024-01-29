using GameFramework.ASCIISerializer;

namespace Networking.Server.Data
{
	static class AvatarData
	{
		public static int[] GetAvatars(int SplitTestGroupID)
		{
			ISerializeObject obj = GameData.GetSplitTestGroupInitialDataObject(SplitTestGroupID);
			if (obj == null)
				return null;

			ISerializeArray arr = obj.Get<ISerializeArray>("Avatar");
			if (arr == null)
				return null;

			return arr.GetRange<int>();
		}
	}
}