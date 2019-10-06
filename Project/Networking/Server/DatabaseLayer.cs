//#define BYPASS_QUERIES
using System.Data;
using GameFramework.Common.Utilities;
using System.Text;
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
			Banned = 1
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

		public static ISerializeObject Authenticate(string Username, string Password, string IP, int RTT)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();
			obj.Set("id", Configs.Random.Next(1, 1000));
			obj.Set("username", Username);
			obj.Set("split_test_group_id", 0);
			obj.Set("result", AuthenticateResult.Passed);
			return obj;
#else
			int id = Constants.NULL_PLAYER_ID;
			AuthenticateResult result = AuthenticateResult.IncorrectUsername;
			ISerializeObject obj = null;

			int pass = EncryptPassword(Password);

			if (string.IsNullOrEmpty(Username))
			{
				Username = "Player " + Configs.Random.Next(1000, 10000);

				database.Execute("INSERT INTO users(username, password, status, split_test_group_id) VALUES(@Username, @Password, @Status, 0)", "Username", Username, "Password", pass, "Status", (int)UserStatus.Normal);

				id = database.LastInsertID;

				database.Execute("UPDATE users SET split_test_group_id=@SplitTestGroupID WHERE id=@ID", "ID", id, "SplitTestGroupID", GameData.ActiveSplitTestGroupsID[id % GameData.ActiveSplitTestGroupsID.Length]);

				result = AuthenticateResult.Passed;

				FillRequiredDataForNewUser(id);
			}

			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT id, username, password, status, split_test_group_id FROM users WHERE username=@Username LIMIT 1", "Username", Username);
			if (arr.Count == 0)
			{
				result = AuthenticateResult.IncorrectUsername;
				goto ReturnResult;
			}

			obj = arr.Get<ISerializeObject>(0);
			id = obj.Get<int>("id");

			if (obj.Get<int>("status") == (int)UserStatus.Banned)
			{
				result = AuthenticateResult.Banned;
				goto DoLog;
			}

			if (obj.Get<int>("password") != pass)
			{
				result = AuthenticateResult.IncorrectPassword;
				goto DoLog;
			}

			result = AuthenticateResult.Passed;

		DoLog:
			database.Execute("INSERT INTO logins_log(user_id, ip, rtt, result, start_time, end_time) VALUES(@UserID, @IP, @RTT, @Result, NOW(), NOW())",
				"UserID", id,
				"IP", IP,
				"RTT", RTT,
				"Result", (int)result);

		ReturnResult:
			if (result == AuthenticateResult.IncorrectUsername)
			{
				obj = Creator.Create<ISerializeObject>();
				obj.Set("result", result);
			}
			else
			{
				obj.Remove("password");
				obj.Remove("status");
			}

			return obj;
#endif
		}

		public static void LogDisconnection(int UserID)
		{
#if !BYPASS_QUERIES
			DataTable table = database.ExecuteWithReturnDataTable("SELECT id FROM logins_log WHERE user_id=@UserID ORDER BY id DESC LIMIT 1", "UserID", UserID);

			if (table.Rows.Count == 0)
				return;

			database.Execute("UPDATE logins_log SET end_time=NOW() WHERE id=@ID", "ID", table.Rows[0]["id"]);
#endif
		}

		public static long GetLeaderboardStartTime(LeaderboardTypes Type)
		{
			DataTable table = database.ExecuteWithReturnDataTable("SELECT UNIX_TIMESTAMP(start_time) start_time FROM leaderboard_config WHERE type=@Type", "Type", (int)Type);
			if (table.Rows.Count == 0)
			{
				if (Type == LeaderboardTypes.AllTime)
					database.Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, '2019/01/01')", "Type", (int)Type);
				else
					database.Execute("INSERT INTO leaderboard_config(type, start_time) VALUES(@Type, NOW())", "Type", (int)Type);

				return GetLeaderboardStartTime(Type);
			}

			return System.Convert.ToInt64(table.Rows[0]["start_time"]);
		}

		public static ISerializeArray GetLeaderboard(LeaderboardTypes Type, int Count)
		{
			long startTime = GetLeaderboardStartTime(Type);

			return database.ExecuteWithReturnISerializeArray("SELECT u.id, u.username, SUM(l.coin) coin, r.level FROM leaderboard_data l INNER JOIN users u ON l.user_id=u.id INNER JOIN users_resource r ON l.user_id=r.user_id WHERE l.occurs_time BETWEEN FROM_UNIXTIME(@StartTime) AND FROM_UNIXTIME(@StartTime + (@HoursPeriod * 3600)) GROUP BY l.user_id ORDER BY SUM(l.coin) DESC LIMIT @Count",
				"StartTime", startTime,
				"HoursPeriod", Constants.LEADERBOARD_TYPE_HOURS[(int)Type],
				"Count", Count);
		}

		public static int CreateGame(GameTypes Type, uint Enterance)
		{
#if BYPASS_QUERIES
			return Configs.Random.Next(1, 1000);
#else
			database.Execute("INSERT INTO games(type, enterance, white_user_id, black_user_id, bot_user_info, winner_user_id, reason, start_time, end_time, replay_data) VALUES(@Type, @Enterance, NULL, NULL, NULL, NULL, NULL, NOW(), NULL, NULL)",
				"Type", (int)Type,
				"Enterance", Enterance);

			return database.LastInsertID;
#endif
		}

		public static void InitializeGame(int GameID, int WhiteUserID, int BlackUserID, string BotUserInfo)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE games SET white_user_id=@WhiteUserID, black_user_id=@BlackUserID, bot_user_info=@BotUserInfo WHERE id=@ID",
				"ID", GameID,
				"WhiteUserID", WhiteUserID,
				"BlackUserID", BlackUserID,
				"BotUserInfo", BotUserInfo);
#endif
		}

		public static void CloseGame(int GameID, int WinnerUserID, GameFinishReasons Reason, byte[] ReplayData)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE games SET winner_user_id=@WinnerUserID, reason=@Reason, end_time=NOW(), replay_data=@ReplayData WHERE id=@ID",
				"ID", GameID,
				"BlackUserID", WinnerUserID,
				"Reason", (int)Reason,
				"ReplayData", ReplayData);
#endif
		}

		public static void AddReward(int UserID, RewardInfo Reward)
		{
#if !BYPASS_QUERIES
			int xpValue = (int)Reward.XP;
			int additionalLevel = 0;

			ISerializeObject userObj = GetUserInfo(UserID);
			if (userObj == null)
				return;

			int cap = LevelData.GetLevelCap(userObj.Get<int>("split_test_group_id"), userObj.Get<int>("level"));

			int xpSum = userObj.Get<int>("xp") + xpValue;
			if (xpSum >= cap)
			{
				additionalLevel = 1;
				xpValue = xpSum - cap;
			}

			database.Execute("UPDATE users_resource SET coin=coin+@Coin,xp=@XP, level=level+@Level WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Reward.Coin,
				"XP", xpValue,
				"Level", additionalLevel);
#endif
		}

		public static void GetCost(int UserID, CostInfo Cost)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE users_resource SET coin=coin-@Coin WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Cost.Coin);
#endif
		}

		public static ISerializeObject GetUserInfo(int UserID)
		{
#if BYPASS_QUERIES
			ISerializeObject obj = Creator.Create<ISerializeObject>();

			obj.Set("id", UserID);
			obj.Set("username", "");
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

			DataTable gamesTable = database.ExecuteWithReturnDataTable("SELECT reason, winner_user_id FROM games WHERE white_user_id=@UserID OR black_user_id=@UserID", "UserID", UserID);

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

		private static void FillRequiredDataForNewUser(int UserID)
		{
#if !BYPASS_QUERIES
			database.Execute("INSERT INTO users_resource(user_id,coin,xp,level) VALUES(@UserID,@Coin,0,1)",
				"UserID", UserID,
				"Coin", 100);
#endif
		}

		private static int EncryptPassword(string Password)
		{
			return (int)CRC32.CalculateHash(Encoding.UTF8.GetBytes(Password));
		}
	}
}
