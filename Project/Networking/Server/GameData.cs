using Networking.Common;
using System.IO;
using GameFramework.BinarySerializer;
using System.Collections.Generic;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	static class GameData
	{
		public class GroupsBufferMap : Dictionary<int, BufferStream>
		{ }

		public class GroupsSerializeObjectMap : Dictionary<int, ISerializeObject>
		{ }

		private static GroupsBufferMap splitTestGroupsInitialDataBuffer;
		private static GroupsSerializeObjectMap splitTestGroupsInitialDataObject;

		private static string ResourcesPath
		{
			get { return Configs.ExecutingPath + "Resources\\"; }
		}

		public static ISerializeObject VersionObject
		{
			get;
			private set;
		}

		public static int[] ActiveSplitTestGroupsID
		{
			get;
			private set;
		}

		static GameData()
		{
			splitTestGroupsInitialDataBuffer = new GroupsBufferMap();
			splitTestGroupsInitialDataObject = new GroupsSerializeObjectMap();

			ISerializeObject data = ReadSerializeObjectFromFile("GameConfig.json");
			VersionObject = data.Get<ISerializeObject>("Version");
			ISerializeArray splitTestArray = data.Get<ISerializeArray>("SplitTest");

			List<int> activeIDs = new List<int>();
			for (uint i = 0; i < splitTestArray.Count; ++i)
			{
				ISerializeObject splitTestObj = splitTestArray.Get<ISerializeObject>(i);

				int id = splitTestObj.Get<int>("ID");

				ISerializeObject groupObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("Filename"));

				BufferStream buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
				buffer.WriteString(groupObj.Content);

				splitTestGroupsInitialDataBuffer[id] = buffer;
				splitTestGroupsInitialDataObject[id] = groupObj;

				if (splitTestObj.Get<bool>("IsActive"))
					activeIDs.Add(id);
			}

			ActiveSplitTestGroupsID = activeIDs.ToArray();
		}

		public static BufferStream GetSplitTestGroupsInitialDataBuffer(int ID)
		{
			if (splitTestGroupsInitialDataBuffer.ContainsKey(ID))
				return splitTestGroupsInitialDataBuffer[ID];

			return null;
		}

		public static ISerializeObject GetSplitTestGroupsInitialDataObject(int ID)
		{
			if (splitTestGroupsInitialDataObject.ContainsKey(ID))
				return splitTestGroupsInitialDataObject[ID];

			return null;
		}

		private static string ReadTextFromFile(string Filename)
		{
			return File.ReadAllText(ResourcesPath + Filename);
		}

		private static ISerializeObject ReadSerializeObjectFromFile(string Filename)
		{
			return Creator.Create<ISerializeObject>(ReadTextFromFile(Filename));
		}
	}
}