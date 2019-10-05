#define BYPASS_QUERIES
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
			obj.Set("id", new Random().Next(1, 1000));
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

				goto DoLog;
			}

			ISerializeArray arr = database.ExecuteWithReturnISerializeArray("SELECT id, username, password, status, split_test_group_id FROM users WHERE username=@Username", "Username", Username);
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

		public static int CreateGame(int UserID1, int UserID2, GameTypes Type)
		{
#if BYPASS_QUERIES
			return new Random().Next(1, 1000);
#else
			database.Execute("INSERT INTO games(user_id_1, user_id_2, type, white_user_id, black_user_id, winner_user_id, reason, start_time, end_time, replay_data) VALUES(@UserID1, @UserID2, @Type, NULL, NULL, NULL, NULL, NOW(), NULL, NULL)",
				"Type", (int)Type,
				"UserID1", UserID1,
				"UserID2", UserID2);

			return database.LastInsertID;
#endif
		}

		public static void CloseGame(int GameID, int WhiteUserID, int BlackUserID, int WinnerUserID, GameFinishReasons Reason, byte[] ReplayData)
		{
#if !BYPASS_QUERIES
			database.Execute("UPDATE games SET white_user_id=@WhiteUserID, black_user_id=@BlackUserID, winner_user_id=@WinnerUserID, reason=@Reason, end_time=NOW(), replay_data=@ReplayData WHERE id=@ID",
				"ID", GameID,
				"WhiteUserID", WhiteUserID,
				"BlackUserID", BlackUserID,
				"BlackUserID", WinnerUserID,
				"Reason", (int)Reason,
				"ReplayData", ReplayData);
#endif
		}

		public static void AddReward(int UserID, RewardInfo Reward)
		{
#if !BYPASS_QUERIES
			int additionalLevel = 0;

			database.Execute("UPDATE users_resource SET coin=coin+@Coin, xp=xp+@XP, level=level+@Level WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Reward.Coin,
				"XP", Reward.XP,
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

		private static void FillRequiredDataForNewUser(int UserID)
		{
#if !BYPASS_QUERIES
			database.Execute("INSERT INTO users_resource(user_id, coin, xp, level) VALUES(@UserID, @Coin, 0, 1)",
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
