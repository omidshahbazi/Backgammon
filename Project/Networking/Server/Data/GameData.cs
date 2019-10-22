using Networking.Common;
using System.IO;
using GameFramework.BinarySerializer;
using System.Collections.Generic;
using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;
using System.Text;

namespace Networking.Server.Data
{
	static class GameData
	{
		public class GroupHashMap : Dictionary<int, uint>
		{ }

		public class GroupBufferMap : Dictionary<int, BufferStream>
		{ }

		public class GroupSerializeObjectMap : Dictionary<int, ISerializeObject>
		{ }

		private static GroupHashMap splitTestGroupsInitialDataHash;
		private static GroupBufferMap splitTestGroupsInitialDataBuffer;
		private static GroupSerializeObjectMap splitTestGroupsInitialDataObject;

		private static GroupHashMap splitTestGroupsStringsHash;
		private static GroupBufferMap splitTestGroupsStringsBuffer;
		private static GroupSerializeObjectMap splitTestGroupsStringsObject;

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
			splitTestGroupsInitialDataHash = new GroupHashMap();
			splitTestGroupsInitialDataBuffer = new GroupBufferMap();
			splitTestGroupsInitialDataObject = new GroupSerializeObjectMap();

			splitTestGroupsStringsHash = new GroupHashMap();
			splitTestGroupsStringsBuffer = new GroupBufferMap();
			splitTestGroupsStringsObject = new GroupSerializeObjectMap();

			ISerializeObject data = ReadSerializeObjectFromFile("GameConfig.json");
			VersionObject = data.Get<ISerializeObject>("Version");
			ISerializeArray splitTestArray = data.Get<ISerializeArray>("SplitTest");

			List<int> activeIDs = new List<int>();
			for (uint i = 0; i < splitTestArray.Count; ++i)
			{
				ISerializeObject splitTestObj = splitTestArray.Get<ISerializeObject>(i);

				int id = splitTestObj.Get<int>("ID");

				ISerializeObject groupObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("InitialDataFilename"));
				uint hash = CRC32.CalculateHash(Encoding.UTF8.GetBytes(groupObj.Content));

				BufferStream buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
				buffer.WriteInt32((int)DataHashStatus.UpdateAvailable);
				buffer.WriteUInt32(hash);
				buffer.WriteString(groupObj.Content);

				splitTestGroupsInitialDataHash[id] = hash;
				splitTestGroupsInitialDataBuffer[id] = buffer;
				splitTestGroupsInitialDataObject[id] = groupObj;

				groupObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("StringsFilename"));
				hash = CRC32.CalculateHash(Encoding.UTF8.GetBytes(groupObj.Content));

				buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_STRINGS);
				buffer.WriteInt32((int)DataHashStatus.UpdateAvailable);
				buffer.WriteUInt32(hash);
				buffer.WriteString(groupObj.Content);

				splitTestGroupsStringsHash[id] = hash;
				splitTestGroupsStringsBuffer[id] = buffer;
				splitTestGroupsStringsObject[id] = groupObj;

				if (splitTestObj.Get<bool>("IsActive"))
					activeIDs.Add(id);
			}

			ActiveSplitTestGroupsID = activeIDs.ToArray();
		}

		public static uint GetSplitTestGroupsInitialDataHash(int ID)
		{
			if (splitTestGroupsInitialDataHash.ContainsKey(ID))
				return splitTestGroupsInitialDataHash[ID];

			return 0;
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

		public static uint GetSplitTestGroupsStringsHash(int ID)
		{
			if (splitTestGroupsStringsHash.ContainsKey(ID))
				return splitTestGroupsStringsHash[ID];

			return 0;
		}

		public static BufferStream GetSplitTestGroupsStringsBuffer(int ID)
		{
			if (splitTestGroupsStringsBuffer.ContainsKey(ID))
				return splitTestGroupsStringsBuffer[ID];

			return null;
		}

		public static ISerializeObject GetSplitTestGroupsStringsObject(int ID)
		{
			if (splitTestGroupsStringsObject.ContainsKey(ID))
				return splitTestGroupsStringsObject[ID];

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