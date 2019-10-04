//#define BYPASS_QUERIES
using System.Data;
using GameFramework.Common.Utilities;
using System.Text;
using Networking.Common;

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
		private static Database database = new Database(Configs.DatabaseConfig.Address, Configs.DatabaseConfig.Username, Configs.DatabaseConfig.Password, Configs.DatabaseConfig.Name);
#endif

		public static AuthenticateResult Authenticate(ref string Username, string Password, string IP, int RTT, out int ID)
		{
#if BYPASS_QUERIES
			ID = new Random().Next(1, 1000);
			return AuthenticateResult.Passed;
#else
			ID = Constants.NULL_PLAYER_ID;
			AuthenticateResult result = AuthenticateResult.IncorrectUsername;

			int pass = EncryptPassword(Password);

			if (string.IsNullOrEmpty(Username))
			{
				Username = "Player " + Configs.Random.Next(1000, 10000);

				database.Execute("INSERT INTO users(username, password, status, split_test_group_id) VALUES(@Username, @Password, @Status, 0)", "Username", Username, "Password", pass, "Status", (int)UserStatus.Normal);

				ID = database.LastInsertID;

				database.Execute("UPDATE users SET split_test_group_id=@SplitTestGroupID WHERE id=@ID", "ID", ID, "SplitTestGroupID", GameData.ActiveSplitTestGroupsID[ID % GameData.ActiveSplitTestGroupsID.Length]);

				result = AuthenticateResult.Passed;

				FillRequiredDataForNewUser(ID);

				goto DoLog;
			}

			DataTable table = database.ExecuteWithReturn("SELECT id, password, status FROM users WHERE username=@Username", "Username", Username);
			if (table.Rows.Count == 0)
				return AuthenticateResult.IncorrectUsername;

			DataRow row = table.Rows[0];

			if (System.Convert.ToInt32(row["status"]) == (int)UserStatus.Banned)
			{
				result = AuthenticateResult.Banned;
				goto DoLog;
			}

			if (pass != System.Convert.ToInt32(row["password"]))
			{
				result = AuthenticateResult.IncorrectPassword;
				goto DoLog;
			}

			ID = System.Convert.ToInt32(row["id"]);
			result = AuthenticateResult.Passed;

		DoLog:
			database.Execute("INSERT INTO logins_log(user_id, ip, rtt, result, start_time, end_time) VALUES(@UserID, @IP, @RTT, @Result, NOW(), NOW())",
				"UserID", ID,
				"IP", IP,
				"RTT", RTT,
				"Result", (int)result);

			return result;
#endif
		}

		public static void LogDisconnection(int UserID)
		{
#if !BYPASS_QUERIES
			DataTable table = database.ExecuteWithReturn("SELECT id FROM logins_log WHERE user_id=@UserID ORDER BY id DESC LIMIT 1", "UserID", UserID);

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
			int additionalLevel = 0;

			database.Execute("UPDATE users_resource SET coin=coin+@Coin, xp=xp+@XP, level=level+@Level WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Reward.Coin,
				"XP", Reward.XP,
				"Level", additionalLevel);
		}

		public static void GetCost(int UserID, CostInfo Cost)
		{
			database.Execute("UPDATE users_resource SET coin=coin-@Coin WHERE user_id=@UserID",
				"UserID", UserID,
				"Coin", Cost.Coin);
		}

		private static void FillRequiredDataForNewUser(int UserID)
		{
			database.Execute("INSERT INTO users_resource(user_id, coin, xp, level) VALUES(@UserID, @Coin, 0, 1)",
				"UserID", UserID,
				"Coin", 100);
		}

		private static int EncryptPassword(string Password)
		{
			return (int)CRC32.CalculateHash(Encoding.UTF8.GetBytes(Password));
		}
	}
}
