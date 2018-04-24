// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using System.Data;

namespace GameServer.Server
{
	static class Database
	{
		private static DatabaseConnection connection = null;

		static Database()
		{
			connection = new DatabaseConnection("127.0.0.1", "backgammon", "root", "!QAZ2wsx");
		}

		public static void Execute(string Query, params object[] Parameters)
		{
			connection.Execute(Query, Parameters);
		}

		public static DataTable ExecuteWithReturn(string Query, params object[] Parameters)
		{
			return connection.ExecuteWithReturn(Query, Parameters);
		}

		public static int GetLastInsertID()
		{
			return connection.GetLastInsertID();
		}
	}
}