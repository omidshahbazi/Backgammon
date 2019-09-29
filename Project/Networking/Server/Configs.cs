using Zorvan.Framework.Common.Utilities;

namespace Networking.Server
{
	static class Configs
	{
		public static string DatabaseAddress = "localhost";
		public static string DatabaseUsername = "root";
		public static string DatabasePassword = "!QAZ2wsx";
		public static string DatabaseName = "backgammon";

		public const int SEND_BUFFER_SIZE = 32;

		public static readonly Random Random = new Random();
	}
}