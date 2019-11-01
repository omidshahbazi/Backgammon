//#define BYPASS_QUERIES
using System.Data;
using Networking.Common;
using GameFramework.DatabaseManaged;
using GameFramework.ASCIISerializer;
using GameFramework.Analytics;
using System;

namespace Networking.Server.Data
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
		private static MySQLDatabase database = null;
		private static Analytics analytics = null;
#endif

		public static void Initialize()
		{
#if !BYPASS_QUERIES
			database = new MySQLDatabase(Configs.DatabaseConfig.Address, Configs.DatabaseConfig.Username, Configs.DatabaseConfig.Password, Configs.DatabaseConfig.Name);

			DatabaseGenerator.UpdateStructure(database);

			MySQLDatabase databaseAnalytics = new MySQLDatabase(Configs.DatabaseConfig.Address, Configs.DatabaseConfig.Username, Configs.DatabaseConfig.Password, Configs.DatabaseConfig.Name + "_analytics");

			analytics = new Analytics(databaseAnalytics);
			analytics.UpdateDatabaseStructure();
#endif
		}

		public static void AddResourceEvent<P, RT, FT>(int UserID, P Place, RT ResourceType, FT FlowType, uint Amount, int Progress = 0)
		{
#if !BYPASS_QUERIES
			analytics.AddResourceEvent(UserID, Place, ResourceType, FlowType, Amount, Progress);
#endif
		}

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

			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT id, status, split_test_group_id FROM users WHERE device_id=@DeviceID LIMIT 1", "DeviceID", DeviceID);

			ISerializeObject obj = null;
			if (arr == null || arr.Count == 0)
			{
				string username = "Player " + Configs.Random.Next(1000, 10000);

				Languages language = GameData.GetDefaultLanguage(Market);

				id = ExecuteInsert("INSERT INTO users(device_id, username, avatar, language, status, split_test_group_id, register_time) VALUES(@DeviceID, @Username, 0, @Language, @Status, 0, NOW())", "DeviceID", DeviceID, "Username", username, "Language", (int)language, "Status", (int)UserStatus.Normal);

				int splitTestGroupID = GameData.ActiveSplitTestGroupsID[id % GameData.ActiveSplitTestGroupsID.Length];

				Execute("UPDATE users SET split_test_group_id=@SplitTestGroupID WHERE id=@ID", "ID", id, "SplitTestGroupID", splitTestGroupID);

				FillRequiredDataForNewUser(id, splitTestGroupID);

				arr = ExecuteWithReturnISerializeArray("SELECT id, status, split_test_group_id FROM users WHERE id=@ID LIMIT 1", "ID", id);

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

			Execute("INSERT INTO users_login(user_id, market, version, ip, rtt, result, disconected_count, start_time, end_time) VALUES(@UserID, @Market, @Version, @IP, @RTT, @Result, 0, NOW(), NOW())",
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
			DataTable table = ExecuteWithReturnDataTable("SELECT id FROM users_login WHERE user_id=@UserID ORDER BY id DESC LIMIT 1", "UserID", UserID);

			if (table.Rows.Count == 0)
				return;

			Execute("UPDATE users_login SET disconected_count=disconected_count+1, end_time=NOW() WHERE id=@ID", "ID", table.Rows[0]["id"]);
#endif
		}

		public static void SetUserInfo(int UserID, string Username, int Avatar)
		{
#if !BYPASS_QUERIES
			Execute("UPDATE users SET username=@Username, avatar=@Avatar WHERE id=@ID", "ID", UserID, "Username", Username, "Avatar", Avatar);
#endif
		}

		public static ISerializeObject GetMigrateCode(int UserID)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();
			obj.Set("code", "SandboxCode");
			return obj;
#else
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT code FROM users_migrate_code WHERE user_id=@UserID AND userd_by_user_id=@UsedByUserID LIMIT 1",
				"UserID", UserID,
				"UsedByUserID", Constants.NULL_USER_ID);

			if (arr == null || arr.Count == 0)
			{
				string code = Configs.Random.Next(10000, 100000).ToString("X");

				int id = ExecuteInsert("INSERT INTO users_migrate_code(user_id, code, used_by_user_id, created_time) VALUES(@UserID, @Code, @UsedByUserID, NOW())",
							"UserID", UserID,
							"Code", code,
							"UsedByUserID", Constants.NULL_USER_ID);

				arr = ExecuteWithReturnISerializeArray("SELECT code FROM users_migrate_code WHERE id=@ID", "ID", id);
			}

			if (arr != null && arr.Count != 0)
				return arr.Get<ISerializeObject>(0);

			return null;
#endif
		}

		public static MigrateResults ApplyMigrateCode(int UserID, string Code)
		{
#if !BYPASS_QUERIES
			ISerializeArray migrateArr = ExecuteWithReturnISerializeArray("SELECT id, user_id FROM users_migrate_code WHERE code=@Code AND used_by_user_id=@UsedByUserID LIMIT 1", "Code", Code, "UsedByUserID", Constants.NULL_USER_ID);
			if (migrateArr == null || migrateArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject migrateObj = migrateArr.Get<ISerializeObject>(0);

			int oldUserID = migrateObj.Get<int>("user_id");

			ISerializeArray oldUserArr = ExecuteWithReturnISerializeArray("SELECT device_id FROM users WHERE id=@UserID AND status=@Status LIMIT 1", "UserID", oldUserID, "Status", (int)UserStatus.Normal);
			if (oldUserArr == null || oldUserArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject oldUserObj = oldUserArr.Get<ISerializeObject>(0);

			ISerializeArray newUserArr = ExecuteWithReturnISerializeArray("SELECT device_id FROM users WHERE id=@UserID LIMIT 1", "UserID", UserID);
			if (newUserArr == null || newUserArr.Count == 0)
				return MigrateResults.Invalid;
			ISerializeObject newUserObj = newUserArr.Get<ISerializeObject>(0);

			Execute("UPDATE users SET device_id=@DeviceID WHERE id=@UserID", "UserID", oldUserID, "DeviceID", newUserObj.Get<string>("device_id"));
			Execute("UPDATE users SET device_id=@DeviceID WHERE id=@UserID", "UserID", UserID, "DeviceID", oldUserObj.Get<string>("device_id"));

			Execute("UPDATE users_migrate_code SET used_by_user_id=@UserID WHERE id=@ID", "ID", migrateObj.Get<int>("id"), "UserID", UserID);
#endif
			return MigrateResults.Done;
		}

		public static void SetPushID(int UserID, string PushID)
		{
#if !BYPASS_QUERIES
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT id FROM users_push WHERE user_id=@UserID LIMIT 1", "UserID", UserID);

			if (arr == null || arr.Count == 0)
				Execute("INSERT INTO users_push(user_id, push_id) VALUES(@UserID, @PushID)", "UserID", UserID, "PushID", PushID);
			else
				Execute("UPDATE users_push SET push_id=@PushID WHERE id=@ID", "ID", UserID, "PushID", PushID);
#endif
		}

		public static int CreateGame(GameTypes Type, uint Bet, int Version)
		{
#if BYPASS_QUERIES
			return Configs.Random.Next(1, 1000);
#else
			//return ExecuteInsert("INSERT INTO users_game(type, bet, white_user_id, black_user_id, bot_user_info, winner_user_id, finish_reason, start_time, end_time, version, replay_data) VALUES(@Type, @Bet, @NullUserID, @NullUserID, NULL, @NullUserID, NULL, NOW(), NULL, @Version, NULL)",
			//	"Type", (int)Type,
			//	"Bet", Bet,
			//	"NullUserID", Constants.NULL_USER_ID,
			//	"Version", Version);
			ExecuteInsert("INSERT INTO users_game(type, bet, white_user_id, black_user_id, bot_user_info, winner_user_id, finish_reason, start_time, end_time, version, replay_data) VALUES(@Type, @Bet, @NullUserID, @NullUserID, NULL, @NullUserID, NULL, NOW(), NULL, @Version, NULL)",
			   "Type", (int)Type,
			   "Bet", Bet,
			   "NullUserID", Constants.NULL_USER_ID,
			   "Version", Version);

			return 439;
#endif
		}

		public static void InitializeGame(int GameID, int WhiteUserID, int BlackUserID, string BotUserInfo)
		{
#if !BYPASS_QUERIES
			Execute("UPDATE users_game SET white_user_id=@WhiteUserID, black_user_id=@BlackUserID, bot_user_info=@BotUserInfo WHERE id=@ID",
				"ID", GameID,
				"WhiteUserID", WhiteUserID,
				"BlackUserID", BlackUserID,
				"BotUserInfo", BotUserInfo);
#endif
		}

		public static void CloseGame(int GameID, int WinnerUserID, GameFinishReasons FinishReason, byte[] ReplayData)
		{
#if !BYPASS_QUERIES
			Execute("UPDATE users_game SET winner_user_id=@WinnerUserID, finish_reason=@FinishReason, end_time=NOW(), replay_data=@ReplayData WHERE id=@ID",
				"ID", GameID,
				"WinnerUserID", WinnerUserID,
				"FinishReason", (int)FinishReason,
				"ReplayData", ReplayData);
#endif
		}

		public static long GetLeaderboardStartTime(LeaderboardTypes Type)
		{
#if BYPASS_QUERIES
			return 0;
#else
			DataTable table = ExecuteWithReturnDataTable("SELECT UNIX_TIMESTAMP(start_time) start_time FROM leaderboard_config WHERE type=@Type LIMIT 1", "Type", (int)Type);
			if (table.Rows.Count == 0)
			{
				if (Type == LeaderboardTypes.AllTime)
					Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, '2019/01/01')", "Type", (int)Type);
				else
					Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, NOW())", "Type", (int)Type);

				return GetLeaderboardStartTime(Type);
			}

			return System.Convert.ToInt64(table.Rows[0]["start_time"]);
#endif
		}

		public static ISerializeArray GetLeaderboard(LeaderboardTypes Type, int Count)
		{
#if BYPASS_QUERIES
			return null;
#else
			long startTime = GetLeaderboardStartTime(Type);

			//return ExecuteWithReturnISerializeArray("SELECT s.user_id, u.username, SUM(s.coin) coin, r.level FROM users_score s INNER JOIN users u ON s.user_id=u.id INNER JOIN users_resource r ON s.user_id=r.user_id WHERE s.occurs_time BETWEEN FROM_UNIXTIME(@StartTime) AND FROM_UNIXTIME(@StartTime + (@HoursPeriod * 3600)) GROUP BY s.user_id ORDER BY SUM(s.coin) DESC LIMIT @Count",
			//	"StartTime", startTime,
			//	"HoursPeriod", Constants.LEADERBOARD_TYPE_HOURS[(int)Type],
			//	"Count", Count);

			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT user_id, SUM(coin) coin FROM users_score WHERE occurs_time BETWEEN FROM_UNIXTIME(@StartTime) AND FROM_UNIXTIME(@StartTime + (@HoursPeriod * 3600)) GROUP BY user_id ORDER BY SUM(coin) DESC LIMIT @Count",
				"StartTime", startTime,
				"HoursPeriod", Constants.LEADERBOARD_TYPE_HOURS[(int)Type],
				"Count", Count);

			if (arr == null || arr.Count == 0)
				return null;

			FillBasicUsersInfo(arr, "user_id");

			return arr;
#endif
		}

		public static ISerializeObject GetPurchase(int UserID, string Token)
		{
#if BYPASS_QUERIES
			return null;
#else
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT id FROM users_purchases WHERE user_id=@UserID AND token=@Token LIMIT 1",
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
			ISerializeObject userObj = GetBasicUserInfo(UserID);

			uint instantLevel = userObj.Get<uint>("level");
			uint instantCoin = userObj.Get<uint>("coin");

			Execute("INSERT INTO users_purchase(user_id, pack_id, sku, price, coin, token, is_valid, occurs_time, instant_level, instant_coin) VALUES(@UserID, @PackID, @SKU, @Price, @Coin, @Token, @IsValid, NOW(), @InstantLevel, @InstantCoin)",
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
				AddReward(UserID, new RewardInfo(Coin, 0), Places.Shop);
		}

		public static ISerializeArray GetGamesLogData(int UserID, int Version, int Count)
		{
#if BYPASS_QUERIES
			return null;
#else
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT id, bet, IF(white_user_id=@UserID, black_user_id, white_user_id) opponent_user_id, bot_user_info, winner_user_id=@UserID is_winner, finish_reason, UNIX_TIMESTAMP(start_time) occurs_time, version=@Version is_replay_available FROM users_game WHERE white_user_id=@UserID OR black_user_id=@UserID ORDER BY start_time DESC LIMIT @Count",
				"UserID", UserID,
				"Version", Version,
				"Count", Count);

			if (arr == null || arr.Count == 0)
				return null;

			FillBasicUsersInfo(arr, "opponent_user_id");

			return arr;
#endif
		}

		public static ISerializeObject GetGameData(int GameID)
		{
#if BYPASS_QUERIES
			return null;
#else
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT IF(white_user_id=@UserID, black_user_id, white_user_id) opponent_user_id, bot_user_info FROM users_game WHERE id=@ID", "ID", GameID);

			if (arr == null || arr.Count == 0)
				return null;

			FillBasicUsersInfo(arr, "user_id");

			return arr.Get<ISerializeObject>(0);
#endif
		}

		public static byte[] GetGameReplayData(int GameID, int Version)
		{
#if BYPASS_QUERIES
			return null;
#else
			DataTable table = ExecuteWithReturnDataTable("SELECT replay_data FROM users_game WHERE id=@ID AND version=@Version LIMIT 1", "ID", GameID, "Version", Version);

			if (table == null || table.Rows.Count == 0)
				return null;

			return (byte[])table.Rows[0]["replay_data"];
#endif
		}

		public static void AddFriendshipRequest(int UserID1, int UserID2)
		{
#if !BYPASS_QUERIES
			Execute("INSERT INTO users_friendship(user_id_1, user_id_2, status, occurs_time) VALUES(@UserID1, @UserID2, @Status, NOW())",
				"UserID1", UserID1,
				"UserID2", UserID2,
				"Status", (int)FriendshipStatus.Requested);
#endif
		}

		public static void RemoveFrinedship(int UserID1, int UserID2)
		{
#if !BYPASS_QUERIES
			DataTable table = ExecuteWithReturnDataTable("SELECT id FROM users_friendship WHERE (user_id_1=@UserID1 AND user_id_2=@UserID2) OR (user_id_2=@UserID1 AND user_id_1=@UserID2) LIMIT 1",
				"UserID1", UserID1,
				"UserID2", UserID2);

			if (table == null || table.Rows.Count == 0)
				return;

			Execute("DELETE FROM users_friend WHERE id=@ID", "ID", table.Rows[0]["id"]);
#endif
		}

		public static void AcceptFriendship(int UserID1, int UserID2)
		{
#if !BYPASS_QUERIES
			DataTable table = ExecuteWithReturnDataTable("SELECT id FROM users_friendship WHERE user_id_2=@UserID1 AND user_id_1=@UserID2 AND status=@Status AND LIMIT 1",
				"UserID1", UserID1,
				"UserID2", UserID2,
				"Status", (int)FriendshipStatus.Requested);

			if (table == null || table.Rows.Count == 0)
				return;

			Execute("UPDATE users_friend SET status=@Status WHERE id=@ID",
				"ID", table.Rows[0]["id"],
				"Status", (int)FriendshipStatus.Accepted);
#endif
		}

		public static ISerializeArray GetFriendships(int UserID)
		{
#if BYPASS_QUERIES
			return null;
#else
			ISerializeArray arr = ExecuteWithReturnISerializeArray("SELECT IF(user_id_1=@UserID, user_id_2, user_id_1) friend_user_id, status FROM users_friendship WHERE user_id_1=@UserID OR user_id_2=@UserID", "UserID", UserID);

			if (arr == null || arr.Count == 0)
				return null;

			FillBasicUsersInfo(arr, "friend_user_id");

			return arr;
#endif
		}

		public static ISerializeObject CanClaimDailyReward(int UserID)
		{
#if BYPASS_QUERIES
			return null;
#else
			DataTable table = ExecuteWithReturnDataTable("SELECT id, FLOOR(UNIX_TIMESTAMP(last_claim_time)/86400)<FLOOR(UNIX_TIMESTAMP(NOW())/86400) can_claim, (FLOOR(UNIX_TIMESTAMP(last_claim_time)/86400)+1)*86400 next_claim_time FROM users_daily_reward WHERE user_id=@UserID LIMIT 1", "UserID", UserID);

			ISerializeObject result = Creator.Create<ISerializeObject>();

			if (table == null || table.Rows.Count == 0)
			{
				Execute("INSERT INTO users_daily_reward(user_id, last_claim_time) VALUES(@UserID, NOW())", "UserID", UserID);

				result.Set("can_claim", true);
			}
			else
			{
				DataRow row = table.Rows[0];

				if (Convert.ToBoolean(row["can_claim"]))
				{
					Execute("UPDATE users_daily_reward SET last_claim_time=NOW() WHERE id=@ID", "ID", row["id"]);

					result.Set("can_claim", true);
				}
				else
				{
					result.Set("can_claim", false);
					result.Set("next_claim_time", Convert.ToInt64(row["next_claim_time"]));
				}
			}

			return result;
#endif
		}

		public static void AddReward(int UserID, RewardInfo Reward, Places Place)
		{
#if !BYPASS_QUERIES
			uint xpValue = Reward.XP;
			uint additionalLevel = 0;

			ISerializeObject userObj = GetBasicUserInfo(UserID);
			if (userObj == null)
				return;

			uint cap = LevelData.GetLevelCap(userObj.Get<int>("split_test_group_id"), userObj.Get<int>("level"));

			uint xpSum = userObj.Get<uint>("xp") + xpValue;
			if (xpSum >= cap)
			{
				additionalLevel = 1;
				xpValue = xpSum - cap;
			}

			Execute("UPDATE users_resource SET coin=coin+@Coin, xp=@XP, level=level+@Level WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Reward.Coin,
				"XP", xpValue,
				"Level", additionalLevel);

			if (Reward.Coin != 0)
			{
				Execute("INSERT INTO users_score(user_id, coin, occurs_time) VALUES(@UserID, @Coin, NOW())",
					"UserId", UserID,
					"Coin", Reward.Coin);
			}

			AddRewardToAnalytics(UserID, Reward, Place, userObj.Get<int>("level"));
#endif
		}

		public static bool HasEnoughResource(int UserID, CostInfo Cost)
		{
#if !BYPASS_QUERIES
			DataTable table = ExecuteWithReturnDataTable("SELECT id FROM users_resource WHERE user_id=@UserID AND coin-@Coin>=0 LIMIT 1",
				"UserID", UserID,
				"Coin", Cost.Coin);

			if (table == null || table.Rows.Count == 0)
				return false;
#endif

			return true;
		}

		public static bool GetCost(int UserID, CostInfo Cost, Places Place)
		{
			if (!HasEnoughResource(UserID, Cost))
				return false;

#if !BYPASS_QUERIES
			Execute("UPDATE users_resource SET coin=coin-@Coin WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Cost.Coin);

			if (Cost.Coin != 0)
			{
				Execute("INSERT INTO users_score(user_id, coin, occurs_time) VALUES(@UserID, @Coin, NOW())",
					"UserId", UserID,
					"Coin", Cost.Coin * -1);
			}
#endif

			if (Cost.Coin != 0)
			{
				ISerializeObject userObj = GetBasicUserInfo(UserID);
				if (userObj != null)
					AddCostToAnalytics(UserID, Cost, Place, userObj.Get<int>("level"));
			}

			return true;
		}

		private static void FillRequiredDataForNewUser(int UserID, int SplitTestGroupID)
		{
			RewardInfo reward = GeneralData.GetInitialResource(SplitTestGroupID);

#if !BYPASS_QUERIES
			Execute("INSERT INTO users_resource(user_id, coin, xp, level) VALUES(@UserID, @Coin, @XP, 1)",
				"UserID", UserID,
				"Coin", reward.Coin,
				"XP", reward.XP);

			AddRewardToAnalytics(UserID, reward, Places.Initialize, 1);
#endif
		}

		public static ISerializeObject GetBasicUserInfo(int UserID)
		{
			ISerializeObject obj = Creator.Create<ISerializeObject>();

			FillBasicUserInfo(UserID, obj);

			return obj;
		}

		public static ISerializeObject GetAdvancedUserInfo(int UserID)
		{
			ISerializeObject obj = GetBasicUserInfo(UserID);
			if (obj == null)
				return null;

			FillAdvancedUserInfo(UserID, obj);

			return obj;
		}

		public static bool FillBasicUserInfo(int UserID, ISerializeObject UserObjectOut)
		{
#if BYPASS_QUERIES
			UserObjectOut.Set("id", UserID);
			UserObjectOut.Set("username", "SandboxName");
			UserObjectOut.Set("avatar", 0);
			UserObjectOut.Set("language", 0);
			UserObjectOut.Set("split_test_group_id", 0);
			UserObjectOut.Set("split_test_group_name", GameData.GetSplitTestGroupName(0));
			UserObjectOut.Set("coin", 10000);
			UserObjectOut.Set("xp", 1);
			UserObjectOut.Set("level", 1);
#else
			ISerializeArray userArr = ExecuteWithReturnISerializeArray("SELECT u.id, u.username, u.avatar, u.language, u.split_test_group_id, r.coin, r.xp, r.level FROM users u INNER JOIN users_resource r ON u.id=r.user_id WHERE u.id=@ID LIMIT 1", "ID", UserID);
			if (userArr == null || userArr.Count == 0)
				return false;

			ISerializeObject obj = userArr.Get<ISerializeObject>(0);
			if (obj == null)
				return false;

			int groupID = obj.Get<int>("split_test_group_id");

			UserObjectOut.Set("id", obj.Get<int>("id"));
			UserObjectOut.Set("username", obj.Get<string>("username"));
			UserObjectOut.Set("avatar", obj.Get<int>("avatar"));
			UserObjectOut.Set("language", obj.Get<int>("language"));
			UserObjectOut.Set("split_test_group_id", groupID);
			UserObjectOut.Set("split_test_group_name", GameData.GetSplitTestGroupName(groupID));
			UserObjectOut.Set("coin", obj.Get<int>("coin"));
			UserObjectOut.Set("xp", obj.Get<int>("xp"));
			UserObjectOut.Set("level", obj.Get<int>("level"));
#endif

			return true;
		}

		public static void FillAdvancedUserInfo(int UserID, ISerializeObject UserObjectOut)
		{
#if BYPASS_QUERIES
			UserObjectOut.Set("game_count", 1);
			UserObjectOut.Set("win_count", 1);
			UserObjectOut.Set("win_gammon_count", 1);
			UserObjectOut.Set("lose_gammon_count", 1);
			UserObjectOut.Set("win_backgammon_count", 1);
			UserObjectOut.Set("lose_backgammon_count", 1);
#else
			DataTable gamesTable = ExecuteWithReturnDataTable("SELECT finish_reason, winner_user_id FROM users_game WHERE white_user_id=@UserID OR black_user_id=@UserID", "UserID", UserID);

			int gameCount = gamesTable.Rows.Count;
			UserObjectOut.Set("game_count", gameCount);

			gamesTable.DefaultView.RowFilter = "finish_reason=" + (int)GameFinishReasons.Normal + " OR finish_reason=" + (int)GameFinishReasons.Gammon + " OR finish_reason=" + (int)GameFinishReasons.Backgammon;
			UserObjectOut.Set("win_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "finish_reason=" + (int)GameFinishReasons.Gammon + " AND winner_user_id=" + UserID;
			UserObjectOut.Set("win_gammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "finish_reason=" + (int)GameFinishReasons.Gammon + " AND winner_user_id<>" + UserID;
			UserObjectOut.Set("lose_gammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "finish_reason=" + (int)GameFinishReasons.Backgammon + " AND winner_user_id=" + UserID;
			UserObjectOut.Set("win_backgammon_count", gamesTable.DefaultView.Count);

			gamesTable.DefaultView.RowFilter = "finish_reason=" + (int)GameFinishReasons.Backgammon + " AND winner_user_id<>" + UserID;
			UserObjectOut.Set("lose_backgammon_count", gamesTable.DefaultView.Count);
#endif
		}

		public static void FillBasicUsersInfo(ISerializeArray UsersArray, string UserIDFieldName)
		{
			for (uint i = 0; i < UsersArray.Count; ++i)
			{
				ISerializeObject obj = UsersArray.Get<ISerializeObject>(i);

				ISerializeObject userObj = obj.AddObject("user_info");

				FillBasicUserInfo(obj.Get<int>(UserIDFieldName), userObj);
			}
		}

		public static void FillFullUsersInfo(ISerializeArray UsersArray, string UserIDFieldName)
		{
			for (uint i = 0; i < UsersArray.Count; ++i)
			{
				ISerializeObject obj = UsersArray.Get<ISerializeObject>(i);

				ISerializeObject userObj = obj.AddObject("user_info");

				int userID = obj.Get<int>(UserIDFieldName);

				FillBasicUserInfo(userID, userObj);

				FillAdvancedUserInfo(userID, userObj);
			}
		}

		private static void Execute(string Query, params object[] Parameters)
		{
			database.Execute(Query, Parameters);
		}

		private static int ExecuteInsert(string Query, params object[] Parameters)
		{
			return database.ExecuteInsert(Query, Parameters);
		}

		private static DataTable ExecuteWithReturnDataTable(string Query, params object[] Parameters)
		{
			return database.ExecuteWithReturnDataTable(Query, Parameters);
		}

		private static ISerializeArray ExecuteWithReturnISerializeArray(string Query, params object[] Parameters)
		{
			return database.ExecuteWithReturnISerializeArray(Query, Parameters);
		}

		private static void AddRewardToAnalytics(int UserID, RewardInfo Reward, Places Place, int Level)
		{
			if (Reward.Coin != 0)
				AddResourceEvent(UserID, Place, ResourceTypes.Coin, FlowTypes.Source, Reward.Coin, Level);

			if (Reward.XP != 0)
				AddResourceEvent(UserID, Place, ResourceTypes.XP, FlowTypes.Source, Reward.XP, Level);
		}

		private static void AddCostToAnalytics(int UserID, CostInfo Cost, Places Place, int Level)
		{
			if (Cost.Coin != 0)
				AddResourceEvent(UserID, Place, ResourceTypes.Coin, FlowTypes.Sink, Cost.Coin, Level);
		}
	}
}
