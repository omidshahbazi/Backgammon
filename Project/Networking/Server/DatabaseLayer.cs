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

		public static AuthenticateResult Authenticate(ref string Username, string Password, out int ID)
		{
#if BYPASS_QUERIES
			ID = new Random().Next(1, 1000);
			return AuthenticateResult.Passed;
#else
			ID = Constants.NULL_PLAYER_ID;

			int pass = EncryptPassword(Password);

			if (string.IsNullOrEmpty(Username))
			{
				Username = "Player " + Configs.Random.Next(1000, 10000);

				database.Execute("INSERT INTO users(username, password, status) VALUES(@Username, @Password, @Status)", "Username", Username, "Password", pass, "Status", (int)UserStatus.Normal);

				ID = database.LastInsertID;

				return AuthenticateResult.Passed;
			}

			DataTable table = database.ExecuteWithReturn("SELECT id, password, status FROM users WHERE username=@Username", "Username", Username);
			if (table.Rows.Count == 0)
				return AuthenticateResult.IncorrectUsername;

			DataRow row = table.Rows[0];

			if (System.Convert.ToInt32(row["status"]) == (int)UserStatus.Banned)
				return AuthenticateResult.Banned;

			if (pass != System.Convert.ToInt32(row["password"]))
				return AuthenticateResult.IncorrectPassword;

			ID = System.Convert.ToInt32(row["id"]);

			return AuthenticateResult.Passed;
#endif
		}

		public static int CreateGame(int UserID1, int UserID2, GameTypes Type)
		{
#if BYPASS_QUERIES
			return new Random().Next(1, 1000);
#else
			database.Execute("INSERT INTO games(user_id_1, user_id_2, type, start_time, end_time) VALUES(@UserID1, @Type, @UserID2, NOW(), NOW())",
				"Type", (int)Type,
				"UserID1", UserID1,
				"UserID2", UserID2);

			return database.LastInsertID;
#endif
		}

		public static void CloseGame(int GameID, int WhitePlayerID, int BlackPlayerID, int WinnerPlayerID, GameFinishReasons Reason, byte[] ReplayData)
		{

		}

		public static void LogAuthentication(int UserID, AuthenticateResult Result, string IP, int RTT)
		{

		}

		public static void LogDisconnection(int UserID)
		{
		}

		public static void AddReward(int UserID, RewardInfo Reward)
		{

		}

		public static void GetCost(int UserID, CostInfo Cost)
		{

		}

		private static int EncryptPassword(string Password)
		{
			return (int)CRC32.CalculateHash(Encoding.UTF8.GetBytes(Password));
		}
	}
}
