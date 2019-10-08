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
			public const byte VERSION_CHECK = 1;
			public const byte AUTHENTICATE = 2;
			public const byte GET_USER_INFO = 3;
			public const byte GET_INITIAL_DATA = 4;
			public const byte JOIN_TO_ROOM = 5;
			public const byte CANCEL_JOIN_TO_ROOM = 6;
			public const byte GET_LEADERBOARD = 7;
			public const byte PURCHASE_FINISHED = 8;

			//Shop commands
			//Dailyreward **based on backgammon king*** commands
			//Friend commands
		}

		public static class Room
		{
			public const byte GET_GAME_DATA = 1;
			public const byte BOARD_TO_BOARD_MOVE = 2;
			public const byte BAR_TO_BOARD_MOVE = 3;
			public const byte BEAR_OFF = 4;
			public const byte FINISH_TURN = 5;
			public const byte RESIGN = 6;
			public const byte FINISH_GAME = 7;
			public const byte SEND_CHAT = 8;
		}
	}

	public enum VersionCheckResults
	{
		UnderMaintenance = 0,
		OK = 1,
		NewerVersionAvailable = 2,
		UpdateNeeded = 3
	}

	public enum AuthenticateResult
	{
		Passed = 0,
		Banned = 1,
		IncorrectUsername = 2,
		IncorrectPassword = 3
	}

	public enum GameFinishReasons
	{
		Normal = 0,
		Gammon = 1,
		Backgammon = 2,
		Resign = 3,
		Disconnect = 4,
		Mismatch = 5
	}

	public enum LeaderboardTypes
	{
		Hourly = 0,
		Daily = 1,
		Weekly = 2,
		AllTime = 3
	}

	public enum Markets
	{
		Windows = 0,
		Cafebazaar = 1
	}
}