#define BYPASS_QUERIES
using System.Data;
using Networking.Common;
using GameFramework.DatabaseManaged;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	static class DatabaseLayer
	{
		public enum UserStatus
		{
			Normal = 0,
			Banned = 1,
			Deleted = 1
		}

		public enum GameTypes
		{
			OneByOne = 0,
			OneByBot = 1,
			Freiendly = 2
		}

#if !BYPASS_QUERIES
		private static MySQLDatabase database = new MySQLDatabase(Configs.DatabaseConfig.Address, Configs.DatabaseConfig.Username, Configs.DatabaseConfig.Password, Configs.DatabaseConfig.Name);
#endif

		public static ISerializeObject Authenticate(string DeviceID, Markets Market, int Version, string IP, int RTT)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();
			obj.Set("id", 0);
			obj.Set("username", "SandboxName");
			obj.Set("split_test_group_id", 0);
			obj.Set("result", AuthenticateResults.Passed);
			return obj;
#else
			int id = Constants.NULL_USER_ID;
			AuthenticateResults result = AuthenticateResults.Passed;

			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT id, username, status, split_test_group_id FROM users WHERE device_id=@DeviceID LIMIT 1", "DeviceID", DeviceID);

			ISerializeObject obj = null;
			if (arr == null || arr.Count == 0)
			{
				string username = "Player " + Configs.Random.Next(1000, 10000);

				database.Execute("INSERT INTO users(device_id, username, status, split_test_group_id) VALUES(@DeviceID, @Username, @Status, 0)", "DeviceID", DeviceID, "Username", username, "Status", (int)UserStatus.Normal);

				id = database.LastInsertID;

				database.Execute("UPDATE users SET split_test_group_id=@SplitTestGroupID WHERE id=@ID", "ID", id, "SplitTestGroupID", GameData.ActiveSplitTestGroupsID[id % GameData.ActiveSplitTestGroupsID.Length]);

				FillRequiredDataForNewUser(id);

				arr = database.ExecuteWithReturnISerializeArray("SELECT id, username, status, split_test_group_id FROM users WHERE id=@ID LIMIT 1", "ID", id);

				obj = arr.Get<ISerializeObject>(0);
			}
			else
			{
				obj = arr.Get<ISerializeObject>(0);

				id = obj.Get<int>("id");
			}

			int status = obj.Get<int>("status");
			if (status == (int)UserStatus.Banned)
				result = AuthenticateResults.Banned;
			else if (status == (int)UserStatus.Deleted)
				result = AuthenticateResults.Deleted;

			database.Execute("INSERT INTO users_login(user_id, market, version, ip, rtt, result, start_time, end_time) VALUES(@UserID, @Market, @Version, @IP, @RTT, @Result, NOW(), NOW())",
				"UserID", id,
				"Market", (int)Market,
				"Version", Version,
				"IP", IP,
				"RTT", RTT,
				"Result", (int)result);

			obj.Set("result", result);
			obj.Remove("status");

			return obj;
#endif
		}

		public static void LogDisconnection(int UserID)
		{
#if !BYPASS_QUERIES
			DataTable table = database.ExecuteWithReturnDataTable("SELECT id FROM users_login WHERE user_id=@UserID ORDER BY id DESC LIMIT 1", "UserID", UserID);

			if (table.Rows.Count == 0)
				return;

			database.Execute("UPDATE users_login SET end_time=NOW() WHERE id=@ID", "ID", table.Rows[0]["id"]);
#endif
		}

		public static void SetUserInfo(int UserID, string Username)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE users SET username=@Username WHERE id=@ID", "ID", UserID, "Username", Username);
#endif
		}

		public static ISerializeObject GetUserInfo(int UserID)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();

			obj.Set("id", UserID);
			obj.Set("username", "SandboxName");
			obj.Set("split_test_group_id", 0);
			obj.Set("coin", 10000);
			obj.Set("xp", 1);
			obj.Set("level", 1);

			obj.Set("game_count", 1);
			obj.Set("win_count", 1);
			obj.Set("win_gammon_count", 1);
			obj.Set("lose_gammon_count", 1);
			obj.Set("win_backgammon_count", 1);
			obj.Set("lose_backgammon_count", 1);

			return obj;
#else
			ISerializeArray userArr = database.ExecuteWithReturnISerializeArray("SELECT u.id, u.username, u.split_test_group_id, r.coin, r.xp, r.level FROM users u INNER JOIN users_resource r ON u.id=r.user_id WHERE u.id=@ID LIMIT 1", "ID", UserID);

			if (userArr.Count == 0)
				return null;

			ISerializeObject obj = userArr.Get<ISerializeObject>(0);

			DataTable gamesTable = database.ExecuteWithReturnDataTable("SELECT reason, winner_user_id FROM users_game WHERE white_user_id=@UserID OR black_user_id=@UserID", "UserID", UserID);

			int gameCount = gamesTable.Rows.Count;
			obj.Set("game_count", gameCount);

			gamesTable.DefaultView.RowFilter = "reason=" + (int)GameFinishReasons.Normal + " OR reason=" + (int)GameFinishReasons.Gammon + " OR reason=" + (int)GameFinishReasons.Backgammon;
			obj.Set("win_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "reason=" + (int)GameFinishReasons.Gammon + " AND winner_user_id=" + UserID;
			obj.Set("win_gammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "reason=" + (int)GameFinishReasons.Gammon + " AND winner_user_id<>" + UserID;
			obj.Set("lose_gammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "reason=" + (int)GameFinishReasons.Backgammon + " AND winner_user_id=" + UserID;
			obj.Set("win_backgammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "reason=" + (int)GameFinishReasons.Backgammon + " AND winner_user_id<>" + UserID;
			obj.Set("lose_backgammon_count", gamesTable.DefaultView.Count);

			return obj;
#endif
		}

		public static ISerializeObject GetMigrateCode(int UserID)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();
			obj.Set("code", "SandboxCode");
			return obj;
#else
			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT code FROM users_migrate_code WHERE user_id=@UserID AND userd_by_user_id=@UsedByUserID LIMIT 1",
				"UserID", UserID,
				"UsedByUserID", Constants.NULL_USER_ID);

			if (arr == null || arr.Count == 0)
			{
				string code = Configs.Random.Next(10000, 100000).ToString("X");

				database.Execute("INSERT INTO users_migrate_code(user_id, code, used_by_user_id, created_time) VALUES(@UserID, @Code, @UsedByUserID, NOW())",
					"UserID", UserID,
					"Code", code,
					"UsedByUserID", Constants.NULL_USER_ID);

				arr = database.ExecuteWithReturnISerializeArray("SELECT code FROM users_migrate_code WHERE id=@ID", "ID", database.LastInsertID);
			}

			if (arr != null && arr.Count != 0)
				return arr.Get<ISerializeObject>(0);

			return null;
#endif
		}

		public static MigrateResults ApplyMigrateCode(int UserID, string Code)
		{
#if !BYPASS_QUERIES
			ISerializeArray migrateArr = database.ExecuteWithReturnISerializeArray("SELECT id, user_id FROM users_migrate_code WHERE code=@Code AND used_by_user_id=@UsedByUserID LIMIT 1", "Code", Code, "UsedByUserID", Constants.NULL_USER_ID);
			if (migrateArr == null || migrateArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject migrateObj = migrateArr.Get<ISerializeObject>(0);

			int oldUserID = migrateObj.Get<int>("user_id");

			ISerializeArray oldUserArr = database.ExecuteWithReturnISerializeArray("SELECT device_id FROM users WHERE id=@UserID AND status=@Status LIMIT 1", "UserID", oldUserID, "Status", (int)UserStatus.Normal);
			if (oldUserArr == null || oldUserArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject oldUserObj = oldUserArr.Get<ISerializeObject>(0);

			ISerializeArray newUserArr = database.ExecuteWithReturnISerializeArray("SELECT device_id FROM users WHERE id=@UserID LIMIT 1", "UserID", UserID);
			if (newUserArr == null || newUserArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject newUserObj = newUserArr.Get<ISerializeObject>(0);

			database.Execute("UPDATE users SET device_id=@DeviceID WHERE id=@UserID", "UserID", oldUserID, "DeviceID", newUserObj.Get<string>("device_id"));
			database.Execute("UPDATE users SET device_id=@DeviceID WHERE id=@UserID", "UserID", UserID, "DeviceID", oldUserObj.Get<string>("device_id"));

			database.Execute("UPDATE users_migrate_code SET used_by_user_id=@UserID WHERE id=@ID", "ID", migrateObj.Get<int>("id"), "UserID", UserID);
#endif
			return MigrateResults.Done;
		}

		public static void SetPushID(int UserID, string PushID)
		{
#if !BYPASS_QUERIES
			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT id FROM users_push WHERE user_id=@UserID LIMIT 1", "UserID", UserID);

			if (arr == null || arr.Count == 0)
				database.Execute("INSERT INTO users_push(user_id, push_id) VALUES(@UserID, @PushID)", "UserID", UserID, "PushID", PushID);
			else
				database.Execute("UPDATE users_push SET push_id=@PushID WHERE id=@ID", "ID", UserID, "PushID", PushID);
#endif
		}

		public static long GetLeaderboardStartTime(LeaderboardTypes Type)
		{
#if BYPASS_QUERIES
			return 0;
#else
			DataTable table = database.ExecuteWithReturnDataTable("SELECT UNIX_TIMESTAMP(start_time) start_time FROM leaderboard_config WHERE type=@Type LIMIT 1", "Type", (int)Type);
			if (table.Rows.Count == 0)
			{
				if (Type == LeaderboardTypes.AllTime)
					database.Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, '2019/01/01')", "Type", (int)Type);
				else
					database.Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, NOW())", "Type", (int)Type);

				return GetLeaderboardStartTime(Type);
			}

			return System.Convert.ToInt64(table.Rows[0]["start_time"]);
#endif
		}

		public static ISerializeArray GetLeaderboard(LeaderboardTypes Type, int Count)
		{
#if BYPASS_QUERIES
			ISerializeArray arr = Creator.Create<ISerializeArray>();
			ISerializeObject obj = arr.AddObject();

			obj.Set("id", 0);
			obj.Set("username", "SandboxName");
			obj.Set("split_test_group_id", 0);
			obj.Set("coin", 10000);
			obj.Set("xp", 1);
			obj.Set("level", 1);

			return arr;
#else
			long startTime = GetLeaderboardStartTime(Type);

			return database.ExecuteWithReturnISerializeArray("SELECT u.id, u.username, SUM(l.coin) coin, r.level FROM users_scores l INNER JOIN users u ON l.user_id=u.id INNER JOIN users_resource r ON l.user_id=r.user_id WHERE l.occurs_time BETWEEN FROM_UNIXTIME(@StartTime) AND FROM_UNIXTIME(@StartTime + (@HoursPeriod * 3600)) GROUP BY l.user_id ORDER BY SUM(l.coin) DESC LIMIT @Count",
				"StartTime", startTime,
				"HoursPeriod", Constants.LEADERBOARD_TYPE_HOURS[(int)Type],
				"Count", Count);
#endif
		}

		public static int CreateGame(GameTypes Type, uint Enterance)
		{
#if BYPASS_QUERIES
			return Configs.Random.Next(1, 1000);
#else
			database.Execute("INSERT INTO users_game(type, enterance, white_user_id, black_user_id, bot_user_info, winner_user_id, reason, start_time, end_time, replay_data) VALUES(@Type, @Enterance, NULL, NULL, NULL, NULL, NULL, NOW(), NULL, NULL)",
				"Type", (int)Type,
				"Enterance", Enterance);

			return database.LastInsertID;
#endif
		}

		public static void InitializeGame(int GameID, int WhiteUserID, int BlackUserID, string BotUserInfo)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE users_game SET white_user_id=@WhiteUserID, black_user_id=@BlackUserID, bot_user_info=@BotUserInfo WHERE id=@ID",
				"ID", GameID,
				"WhiteUserID", WhiteUserID,
				"BlackUserID", BlackUserID,
				"BotUserInfo", BotUserInfo);
#endif
		}

		public static void CloseGame(int GameID, int WinnerUserID, GameFinishReasons Reason, byte[] ReplayData)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE users_game SET winner_user_id=@WinnerUserID, reason=@Reason, end_time=NOW(), replay_data=@ReplayData WHERE id=@ID",
				"ID", GameID,
				"BlackUserID", WinnerUserID,
				"Reason", (int)Reason,
				"ReplayData", ReplayData);
#endif
		}

		public static ISerializeObject GetPurchase(int UserID, string Token)
		{
#if BYPASS_QUERIES
			return null;
#else
			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT id FROM users_purchases WHERE user_id=@UserID AND token=@Token LIMIT 1",
				"UserID", UserID,
				"Token", Token);

			if (arr == null)
				return null;

			return arr.Get<ISerializeObject>(0);
#endif
		}

		public static void AddPurchase(int UserID, int PackID, string SKU, uint Price, uint Coin, string Token, bool IsValid)
		{
#if !BYPASS_QUERIES
			ISerializeObject userObj = GetUserInfo(UserID);

			uint instantLevel = userObj.Get<uint>("level");
			uint instantCoin = userObj.Get<uint>("coin");

			database.Execute("INSERT INTO users_purchase(user_id, pack_id, sku, price, coin, token, is_valid, occurs_time, instant_level, instant_coin) VALUES(@UserID, @PackID, @SKU, @Price, @Coin, @Token, @IsValid, NOW(), @InstantLevel, @InstantCoin)",
				"UserID", UserID,
				"PackID", PackID,
				"SKU", SKU,
				"Price", Price,
				"Coin", Coin,
				"Token", Token,
				"IsValid", (IsValid ? 1 : 0),
				"InstantLevel", instantLevel,
				"InstantCoin", instantCoin);
#endif

			if (IsValid)
				AddReward(UserID, new RewardInfo(Coin, 0));
		}

		public static void AddReward(int UserID, RewardInfo Reward)
		{
#if !BYPASS_QUERIES
			uint xpValue = Reward.XP;
			uint additionalLevel = 0;

			ISerializeObject userObj = GetUserInfo(UserID);
			if (userObj == null)
				return;

			uint cap = LevelData.GetLevelCap(userObj.Get<int>("split_test_group_id"), userObj.Get<int>("level"));

			uint xpSum = userObj.Get<uint>("xp") + xpValue;
			if (xpSum >= cap)
			{
				additionalLevel = 1;
				xpValue = xpSum - cap;
			}

			database.Execute("UPDATE users_resource SET coin=coin+@Coin, xp=@XP, level=level+@Level WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Reward.Coin,
				"XP", xpValue,
				"Level", additionalLevel);

			if (Reward.Coin != 0)
			{
				database.Execute("INSERT INTO users_scores(user_id, coin, occurs_time) VALUES(@UserID, @Coin, NOW())",
					"UserId", UserID,
					"Coin", Reward.Coin);
			}
#endif
		}

		public static void GetCost(int UserID, CostInfo Cost)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE users_resource SET coin=coin-@Coin WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Cost.Coin);

			if (Cost.Coin != 0)
			{
				database.Execute("INSERT INTO users_scores(user_id, coin, occurs_time) VALUES(@UserID, @Coin, NOW())",
					"UserId", UserID,
					"Coin", Cost.Coin * -1);
			}
#endif
		}

		private static void FillRequiredDataForNewUser(int UserID)
		{
#if !BYPASS_QUERIES
			database.Execute("INSERT INTO users_resource(user_id, coin, xp, level) VALUES(@UserID, @Coin, 0,1 )",
				"UserID", UserID,
				"Coin", 100);
#endif
		}
	}
}
