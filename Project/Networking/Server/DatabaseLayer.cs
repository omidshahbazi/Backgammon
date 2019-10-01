using System.Data;
using GameFramework.Common.Utilities;
using System.Text;
using Networking.Common;

namespace Networking.Server
{
	static class DatabaseLayer
	{
		private enum UserStatus
		{
			Normal = 0,
			Banned = 1
		}

		private static Database database = new Database(Configs.DatabaseConfig.Address, Configs.DatabaseConfig.Username, Configs.DatabaseConfig.Password, Configs.DatabaseConfig.Name);

		public static AuthenticateResult Authenticate(ref string Username, string Password, out int ID)
		{
			ID = -1;

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
		}

		public static int CreateGame(int UserID1, int UserID2)
		{
			database.Execute("INSERT INTO games(user_id_1, user_id_2, start_time, end_time) VALUES(@UserID1, @UserID2, NOW(), NOW())",
				"UserID1", UserID1,
				"UserID2", UserID2);

			return database.LastInsertID;
		}

		private static int EncryptPassword(string Password)
		{
			return (int)CRC32.CalculateHash(Encoding.UTF8.GetBytes(Password));
		}
	}
}
