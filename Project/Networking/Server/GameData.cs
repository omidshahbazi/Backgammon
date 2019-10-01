using Networking.Common;
using System.IO;
using GameFramework.BinarySerializer;

namespace Networking.Server
{
	static class GameData
	{
		public static BufferStream Data
		{
			get;
			private set;
		}

		static GameData()
		{
			Data = new BufferStream(new MemoryStream());
			Data.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
			Data.WriteString(File.ReadAllText(Configs.ExecutingPath + "GameData.json"));

		}
	}
}