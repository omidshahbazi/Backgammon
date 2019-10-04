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

		private static string ResourcesPath
		{
			get { return Configs.ExecutingPath + "Resources\\"; }
		}

		public static ISerializeObject VersionObject
		{
			get;
			private set;
		}

		public static ISerializeArray SplitTestArray
		{
			get;
			private set;
		}

		public static GroupsBufferMap SplitTestGroupsInitialDataBuffer
		{
			get;
			private set;
		}

		public static GroupsSerializeObjectMap SplitTestGroupsInitialDataObject
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
			SplitTestGroupsInitialDataBuffer = new GroupsBufferMap();
			SplitTestGroupsInitialDataObject = new GroupsSerializeObjectMap();

			ISerializeObject data = ReadSerializeObjectFromFile("GameConfig.json");
			VersionObject = data.Get<ISerializeObject>("Version");
			SplitTestArray = data.Get<ISerializeArray>("SplitTest");

			List<int> activeIDs = new List<int>();
			for (uint i = 0; i < SplitTestArray.Count; ++i)
			{
				ISerializeObject splitTestObj = SplitTestArray.Get<ISerializeObject>(i);

				int id = splitTestObj.Get<int>("ID");

				ISerializeObject groupObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("Filename"));

				BufferStream buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
				buffer.WriteString(groupObj.Content);

				SplitTestGroupsInitialDataBuffer[id] = buffer;
				SplitTestGroupsInitialDataObject[id] = groupObj;

				if (splitTestObj.Get<bool>("IsActive"))
					activeIDs.Add(id);
			}

			ActiveSplitTestGroupsID = activeIDs.ToArray();
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