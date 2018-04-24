// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace GameServer.Server
{
	class DatabaseConnection
	{
		private MySqlConnection connection = null;

		public DatabaseConnection(string Address, string DatabaseName, string Username, string Password)
		{
			connection = new MySqlConnection("server=" + Address + ";uid=" + Username + ";pwd=" + Password + ";database=" + DatabaseName);
		}

		public void Execute(string Query, params object[] Parameters)
		{
			OpenConnection();

			CreateCommand(Query, Parameters).ExecuteNonQuery();
		}

		public DataTable ExecuteWithReturn(string Query, params object[] Parameters)
		{
			OpenConnection();

			MySqlDataAdapter adapter = new MySqlDataAdapter(CreateCommand(Query, Parameters));
			DataTable table = new DataTable();
			adapter.Fill(table);
			return table;
		}

		public int GetLastInsertID()
		{
			DataTable table = ExecuteWithReturn("SELECT LAST_INSERT_ID() ID");
			return Convert.ToInt32(table.Rows[0]["ID"]);
		}

		private MySqlCommand CreateCommand(string Query, object[] Parameters)
		{
			MySqlCommand command = new MySqlCommand(Query, connection);

			if (Parameters != null)
			{
				if (Parameters.Length % 2 != 0)
					throw new System.Exception("Parameters count must be even");

				for (int i = 0; i < Parameters.Length; i += 2)
					command.Parameters.AddWithValue(Parameters[i].ToString(), Parameters[i + 1]);
			}

			return command;
		}

		private void OpenConnection()
		{
			if (connection.State != ConnectionState.Open)
				connection.Open();
		}
	}
}