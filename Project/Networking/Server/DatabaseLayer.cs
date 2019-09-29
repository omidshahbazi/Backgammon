using Networking.Server;
using Networking.Common;
using System;
using System.Data;

namespace Networking.Server
{
	static class DatabaseLayer
	{
		private enum UserStatus
		{
			Normal = 0,
			Banned = 1
		}

		public enum AuthenticateResult
		{
			Passed = 0,
			Banned = 1,
			IncorrectUsername = 2,
			IncorrectPassword = 3
		}

		private static Database database = new Database(Configs.DatabaseAddress, Configs.DatabaseUsername, Configs.DatabasePassword, Configs.DatabaseName);

		public static AuthenticateResult Authenticate(ref string Username, string Password, out int ID)
		{
			ID = -1;

			int pass = EncryptPassword(Password);

			if (string.IsNullOrEmpty(Username))
			{
				Username = "Player " + new Random().Next(1000, 10000);

				database.Execute("INSERT INTO users(username, password, status) VALUES(@Username, @Password, @Status)", "Username", Username, "Password", pass, "Status", (int)UserStatus.Normal);

				ID = database.LastInsertID;

				return AuthenticateResult.Passed;
			}

			DataTable table = database.ExecuteWithReturn("SELECT id, password, status FROM users WHERE username=@Username", "Username", Username);
			if (table.Rows.Count == 0)
				return AuthenticateResult.IncorrectUsername;

			DataRow row = table.Rows[0];

			if (Convert.ToInt32(row["status"]) == (int)UserStatus.Banned)
				return AuthenticateResult.Banned;

			if (pass != Convert.ToInt32(row["password"]))
				return AuthenticateResult.IncorrectPassword;

			ID = Convert.ToInt32(row["id"]);

			return AuthenticateResult.Passed;
		}

		private static int EncryptPassword(string Password)
		{
			return Password.GetHashCode();
		}
	}
}
