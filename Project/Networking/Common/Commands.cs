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
			public const byte GET_INITIAL_DATA = 2;
			public const byte JOIN_TO_ROOM = 3;
			public const byte CANCEL_JOIN_TO_ROOM = 4;
		}

		public static class Room
		{
			public const byte BOARD_TO_BOARD_MOVE = 1;
			public const byte BAR_TO_BOARD_MOVE = 2;
			public const byte BEAR_OFF = 3;
			public const byte FINISH_TURN = 4;
			public const byte RESIGN = 5;
		}
	}

	public enum AuthenticateResult
	{
		Passed = 0,
		Banned = 1,
		IncorrectUsername = 2,
		IncorrectPassword = 3
	}
}