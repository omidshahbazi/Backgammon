using System.Data;
using Zorvan.Framework.Common.Utilities;
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

		private static Database database = new Database(Configs.DatabaseAddress, Configs.DatabaseUsername, Configs.DatabasePassword, Configs.DatabaseName);

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

		private static int EncryptPassword(string Password)
		{
			return (int)CRC32.CalculateHash(Encoding.UTF8.GetBytes(Password));
		}
	}
}
