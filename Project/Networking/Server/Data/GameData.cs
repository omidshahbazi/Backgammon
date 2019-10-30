using Networking.Common;
using GameFramework.BinarySerializer;
using System.Collections.Generic;
using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;
using System.Text;
using GameFramework.Common.FileLayer;
using System.IO;

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

		public class GroupNameMap : Dictionary<int, string>
		{ }

		private static GroupNameMap splitTestGroupNames;

		private static GroupHashMap splitTestGroupsInitialDataHash;
		private static GroupBufferMap splitTestGroupsInitialDataBuffer;
		private static GroupSerializeObjectMap splitTestGroupsInitialDataObject;

		private static GroupHashMap splitTestGroupsStringsHash;
		private static GroupBufferMap splitTestGroupsStringsBuffer;
		private static GroupSerializeObjectMap splitTestGroupsStringsObject;

		public static string ResourcesPath
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

		public static int[] ActiveSplitTestGroupsName
		{
			get;
			private set;
		}

		public static void Initialize()
		{
			splitTestGroupNames = new GroupNameMap();

			splitTestGroupsInitialDataHash = new GroupHashMap();
			splitTestGroupsInitialDataBuffer = new GroupBufferMap();
			splitTestGroupsInitialDataObject = new GroupSerializeObjectMap();

			splitTestGroupsStringsHash = new GroupHashMap();
			splitTestGroupsStringsBuffer = new GroupBufferMap();
			splitTestGroupsStringsObject = new GroupSerializeObjectMap();

			ISerializeObject data = ReadSerializeObjectFromFile("GameConfig.json");
			VersionObject = data.Get<ISerializeObject>("Version");
			ISerializeArray splitTestArray = data.Get<ISerializeArray>("SplitTest");

			ISerializeObject baseDataObj = null;
			ISerializeObject baseStringsObj = null;

			List<int> activeIDs = new List<int>();
			List<string> activeNames = new List<string>();
			for (uint i = 0; i < splitTestArray.Count; ++i)
			{
				ISerializeObject splitTestObj = splitTestArray.Get<ISerializeObject>(i);

				int id = splitTestObj.Get<int>("ID");

				ISerializeObject dataObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("InitialDataFilename"));
				if (baseDataObj == null)
					baseDataObj = dataObj;
				else
				{
					ISerializeObject baseDataObjTemp = baseDataObj.Clone();
					Creator.Override(dataObj, baseDataObjTemp);
					dataObj = baseDataObjTemp;
				}

				uint hash = CRC32.CalculateHash(Encoding.UTF8.GetBytes(dataObj.Content));

				BufferStream buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
				buffer.WriteInt32((int)DataHashStatus.UpdateAvailable);
				buffer.WriteUInt32(hash);
				buffer.WriteString(dataObj.Content);

				splitTestGroupsInitialDataHash[id] = hash;
				splitTestGroupsInitialDataBuffer[id] = buffer;
				splitTestGroupsInitialDataObject[id] = dataObj;

				ISerializeObject stringsObj = ReadSerializeObjectFromFile(splitTestObj.Get<string>("StringsFilename"));
				if (baseStringsObj == null)
					baseStringsObj = stringsObj;
				else
				{
					ISerializeObject baseStringsObjTemp = baseStringsObj.Clone();
					Creator.Override(stringsObj, baseStringsObjTemp);
					stringsObj = baseStringsObjTemp;
				}

				hash = CRC32.CalculateHash(Encoding.UTF8.GetBytes(stringsObj.Content));

				buffer = new BufferStream(new MemoryStream());
				buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_STRINGS);
				buffer.WriteInt32((int)DataHashStatus.UpdateAvailable);
				buffer.WriteUInt32(hash);
				buffer.WriteString(stringsObj.Content);

				splitTestGroupsStringsHash[id] = hash;
				splitTestGroupsStringsBuffer[id] = buffer;
				splitTestGroupsStringsObject[id] = stringsObj;

				if (splitTestObj.Get<bool>("IsActive"))
					activeIDs.Add(id);

				splitTestGroupNames[id] = splitTestObj.Get<string>("Name");
			}

			ActiveSplitTestGroupsID = activeIDs.ToArray();
		}

		public static Languages GetDefaultLanguage(Markets Market)
		{
			ISerializeObject versionObj = GameData.VersionObject;
			if (versionObj == null)
				return Languages.Persian;

			versionObj = versionObj.Get<ISerializeObject>(Market.ToString());
			if (versionObj == null)
				return Languages.Persian;

			return (Languages)versionObj.Get<int>("DefaultLanguage");
		}

		public static string GetSplitTestGroupName(int ID)
		{
			if (splitTestGroupNames.ContainsKey(ID))
				return splitTestGroupNames[ID];

			return "";
		}

		public static uint GetSplitTestGroupInitialDataHash(int ID)
		{
			if (splitTestGroupsInitialDataHash.ContainsKey(ID))
				return splitTestGroupsInitialDataHash[ID];

			return 0;
		}

		public static BufferStream GetSplitTestGroupInitialDataBuffer(int ID)
		{
			if (splitTestGroupsInitialDataBuffer.ContainsKey(ID))
				return splitTestGroupsInitialDataBuffer[ID];

			return null;
		}

		public static ISerializeObject GetSplitTestGroupInitialDataObject(int ID)
		{
			if (splitTestGroupsInitialDataObject.ContainsKey(ID))
				return splitTestGroupsInitialDataObject[ID];

			return null;
		}

		public static uint GetSplitTestGroupStringsHash(int ID)
		{
			if (splitTestGroupsStringsHash.ContainsKey(ID))
				return splitTestGroupsStringsHash[ID];

			return 0;
		}

		public static BufferStream GetSplitTestGroupStringsBuffer(int ID)
		{
			if (splitTestGroupsStringsBuffer.ContainsKey(ID))
				return splitTestGroupsStringsBuffer[ID];

			return null;
		}

		public static ISerializeObject GetSplitTestGroupStringsObject(int ID)
		{
			if (splitTestGroupsStringsObject.ContainsKey(ID))
				return splitTestGroupsStringsObject[ID];

			return null;
		}

		private static ISerializeObject ReadSerializeObjectFromFile(string Filename)
		{
			return Creator.Create<ISerializeObject>(FileSystem.Read(Filename));
		}
	}
}