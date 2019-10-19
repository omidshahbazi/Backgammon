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
			public const byte SET_USER_INFO = 3;
			public const byte GET_USER_INFO = 4;
			public const byte GET_MIGRATE_CODE = 5;
			public const byte APPLY_MIGRATE_CODE = 6;
			public const byte SET_PUSH_ID = 7;
			public const byte GET_INITIAL_DATA = 8;
			public const byte JOIN_TO_ROOM = 9;
			public const byte CANCEL_JOIN_TO_ROOM = 10;
			public const byte GET_LEADERBOARD = 11;
			public const byte PURCHASE_FINISHED = 12;
			public const byte GET_GAME_REPLAY_DATA = 13;

			//Dailyreward **based on backgammon king*** commands
			//Friend commands
			//invitation code
		}

		public static class Room
		{
			public const byte GET_GAME_DATA = 1;
			public const byte START_TURN = 2;
			public const byte BOARD_TO_BOARD_MOVE = 3;
			public const byte BAR_TO_BOARD_MOVE = 4;
			public const byte BOARD_TO_BAR_MOVE = 5;
			public const byte BEAR_OFF = 6;
			public const byte FINISH_TURN = 7;
			public const byte RESIGN = 8;
			public const byte FINISH_GAME = 9;
			public const byte SEND_CHAT = 10;
		}
	}

	public enum VersionCheckResults
	{
		UnderMaintenance = 0,
		OK = 1,
		NewerVersionAvailable = 2,
		UpdateNeeded = 3
	}

	public enum AuthenticateResults
	{
		Passed = 0,
		Banned = 1,
		Deleted = 2
	}

	public enum MigrateResults
	{
		Invalid = 0,
		Done
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