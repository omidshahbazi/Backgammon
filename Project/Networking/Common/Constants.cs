
using BeardedManStudios.Forge.Networking;

namespace Networking.Common
{
	public static class Constants
	{
		public const string SERVER_HEADER_TEXT = @"Backgammon Server";

		public const int BINARY_FRAME_GROUP_ID = MessageGroupIds.START_OF_GENERIC_IDS + 1;

		public const int NULL_PLAYER_ID = -1;
	}
}
