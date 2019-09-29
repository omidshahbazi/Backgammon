namespace Networking.Common
{
	public static class Commands
	{
		public static class Category
		{
			public const byte LOBBY = 1;
			public const byte ROOM = 2;
		}

		public static class Lobby
		{
			public const byte AUTHENTICATE = 1;
			public const byte JOIN_TO_ROOM = 2;
		}

		public static class Room
		{
			public const byte GET_INITIAL_DATA = 1;
			public const byte MOVE_CHECKER = 2;

			public const byte RESIGN = 11;
		}
	}
}