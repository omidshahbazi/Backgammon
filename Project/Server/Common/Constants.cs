
using BeardedManStudios.Forge.Networking;

namespace Common
{
    public static class Constants
    {
		public const string SERVER_HEADER_TEXT = @"Backgammon Server";

		//public const string SERVER_IP = "89.42.209.124";
		//public const string HOST_IP = "89.42.209.124";
		public const string SERVER_IP = "127.0.0.1";
		public const string HOST_IP = "127.0.0.1";

		public const int PORT_NUMBER = 85;

		public const int MAX_CONNECTION_COUNT = 2048;

		public const int BINARY_FRAME_GROUP_ID = MessageGroupIds.START_OF_GENERIC_IDS + 1;
	}
}
