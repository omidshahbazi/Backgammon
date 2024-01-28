using System.IO;
using GameFramework.ASCIISerializer;
using GameFramework.Common.Utilities;

namespace Networking.Server.Data
{
	static class Configs
	{
		public struct Network
		{
			public string BindAddress;
			public ushort Port;
			public int SendBufferSize;
			public int MaxConnectionCount;
			public bool DebugInfo;
		}

		public struct Database
		{
			public string Address;
			public string Username;
			public string Password;
			public string Name;
		}

		private static readonly Random random = new Random();

		public static Network NetworkConfig;
		public static Database DatabaseConfig;

		public static Random Random
		{
			get { return random; }
		}

		public static string ExecutingPath
		{
			get { return ConsoleHelper.ExecutingPath; }
		}

		public static string ExecutableFileName
		{
			get { return ConsoleHelper.ExecutableFileName; }
		}

		static Configs()
		{
			string filePath = ExecutingPath + ExecutableFileName + ".json";

			ISerializeObject obj = null;

			if (File.Exists(filePath))
				obj = Creator.Create<ISerializeObject>(File.ReadAllText(filePath));
			else
			{
				obj = CreateTemplate();

				File.WriteAllText(filePath, obj.Content);
			}

			ISerializeObject networkObj = obj.Get<ISerializeObject>("Network");
			if (networkObj == null)
				return;

			NetworkConfig = new Network();
			NetworkConfig.BindAddress =			networkObj.Get<string>("BindAddress");
			NetworkConfig.Port =				networkObj.Get<ushort>("Port");
			NetworkConfig.SendBufferSize =		networkObj.Get<int>("SendBufferSize");
			NetworkConfig.MaxConnectionCount =	networkObj.Get<int>("MaxConnectionCount");
			NetworkConfig.DebugInfo =			 networkObj.Get<bool>("DebugInfo");

			ISerializeObject databaseObj = obj.Get<ISerializeObject>("Database");
			if (databaseObj == null)
				return;

			DatabaseConfig = new Database();
			DatabaseConfig.Address = databaseObj.Get<string>("Address");
			DatabaseConfig.Username = databaseObj.Get<string>("Username");
			DatabaseConfig.Password = databaseObj.Get<string>("Password");
			DatabaseConfig.Name = databaseObj.Get<string>("Name");
		}

		private static ISerializeObject CreateTemplate()
		{
			ISerializeObject obj = Creator.Create<ISerializeObject>();

			ISerializeObject networkObj = obj.AddObject("Network");
			{
				networkObj.Set("BindAddress", "127.0.01");
				networkObj.Set("Port", 80);
				networkObj.Set("SendBufferSize", 2048);
				networkObj.Set("MaxConnectionCount", 1024);
				networkObj.Set("DebugInfo", true);
			}

			ISerializeObject databaseObj = obj.AddObject("Database");
			{
				networkObj.Set("Address", "127.0.01");
				networkObj.Set("Username", "");
				networkObj.Set("Password", "");
				networkObj.Set("Name", "");
			}

			return obj;
		}
	}
}