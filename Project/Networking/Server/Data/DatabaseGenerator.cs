//#define BYPASS_QUERIES
using GameFramework.DatabaseManaged;
using GameFramework.DatabaseManaged.Generator;
using GameFramework.Common.Utilities;
using System.Text;

namespace Networking.Server.Data
{
	static class DatabaseGenerator
	{
		private static Table LeaderboardConfigTable = new Table("leaderboard_config", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("type", DataType.Int), new Column("start_time", DataType.DateTime));
		private static Table UsersTable = new Table("users", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("device_id", DataType.VarChar), new Column("username", DataType.NVarChar), new Column("avatar", DataType.Int), new Column("language", DataType.Int), new Column("status", DataType.Int), new Column("split_test_group_id", DataType.Int), new Column("register_time", DataType.DateTime));
		private static Table UsersDailyRewardTable = new Table("users_daily_reward", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("last_claim_time", DataType.DateTime));
		private static Table UsersFriendshipTable = new Table("users_friendship", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id_1", DataType.Int), new Column("user_id_2", DataType.Int), new Column("status", DataType.Int), new Column("occurs_time", DataType.DateTime));
		private static Table UsersGameTable = new Table("users_game", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("type", DataType.Int), new Column("bet", DataType.Int), new Column("white_user_id", DataType.Int), new Column("black_user_id", DataType.Int), new Column("bot_user_info", new DataType("VARCHAR", 500)), new Column("winner_user_id", DataType.Int), new Column("finish_reason", DataType.Int), new Column("start_time", DataType.DateTime), new Column("end_time", DataType.DateTime), new Column("version", DataType.Int), new Column("replay_data", DataType.LongBlob));
		private static Table UsersLogin = new Table("users_login", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("market", DataType.Int), new Column("version", DataType.Int), new Column("ip", DataType.VarChar), new Column("rtt", DataType.Int), new Column("result", DataType.Int), new Column("disconected_count", DataType.Int), new Column("start_time", DataType.DateTime), new Column("end_time", DataType.DateTime));
		private static Table UsersMigrateCode = new Table("users_migrate_code", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("code", DataType.Int), new Column("used_by_user_id", DataType.Int), new Column("created_time", DataType.DateTime));
		private static Table Users_purchase = new Table("users_purchase", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("pack_id", DataType.Int), new Column("sku", DataType.VarChar), new Column("price", DataType.Int), new Column("coin", DataType.Int), new Column("token", DataType.VarChar), new Column("is_valid", DataType.Int), new Column("occurs_time", DataType.DateTime), new Column("instant_level", DataType.Int), new Column("instant_coin", DataType.Int));
		private static Table UsersPush = new Table("users_push", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("push_id", DataType.VarChar));
		private static Table UsersResource = new Table("users_resource", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("coin", DataType.Int), new Column("xp", DataType.Int), new Column("level", DataType.Int));
		private static Table UsersScore = new Table("users_score", Collates.UTF8, Engines.InnoDB, new Column("id", DataType.Int, Flags.PrimaryKey | Flags.AutoIncrement), new Column("user_id", DataType.Int), new Column("coin", DataType.Int), new Column("occurs_time", DataType.DateTime));

		public static void UpdateStructure(Database Database)
		{
			Table[] tables = ReflectionExtensions.GetFields<Table>(typeof(DatabaseGenerator), ReflectionExtensions.PrivateStaticFlags);

			StringBuilder updateQuery = new StringBuilder();
			StringBuilder deprecatedQuery = new StringBuilder();

			for (int i = 0; i < tables.Length; ++i)
				TSQLGenerator.MySQL.GenerateCreateTable(Database, tables[i], SyncTypes.Keep, updateQuery, deprecatedQuery);

			if (updateQuery.Length != 0)
				Database.Execute(updateQuery.ToString());
		}
	}
}