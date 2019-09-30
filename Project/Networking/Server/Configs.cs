using System.IO;
using System.Reflection;
using Zorvan.Framework.ASCIISerializer;
using Zorvan.Framework.Common.Utilities;

namespace Networking.Server
{
	static class Configs
	{
		public struct Network
		{
			public string BindAddress;
			public int Port;
			public int SendBufferSize;
			public int MaxConnectionCount;
		}

		public struct Database
		{
			public string Address;
			public string Username;
			public string Password;
			public string Name;
		}

		public static Network NetworkConfig;
		public static Database DatabaseConfig;

		public static readonly Random Random = new Random();

		public static string ExecutingPath
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"; }
		}

		public static string ExecutableFileName
		{
			get { return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location); }
		}

		static Configs()
		{
			ISerializeObject obj = Creator.Create<ISerializeObject>(File.ReadAllText(ExecutingPath + ExecutableFileName + ".json"));
			if (obj == null)
				return;

			ISerializeObject networkObj = obj.Get<ISerializeObject>("Network");
			if (networkObj == null)
				return;

			NetworkConfig = new Network();
			NetworkConfig.BindAddress = networkObj.Get<string>("BindAddress");
			NetworkConfig.Port = networkObj.Get<int>("Port");
			NetworkConfig.SendBufferSize = networkObj.Get<int>("SendBufferSize");
			NetworkConfig.MaxConnectionCount = networkObj.Get<int>("MaxConnectionCount");

			ISerializeObject databaseObj = obj.Get<ISerializeObject>("Database");
			if (databaseObj == null)
				return;

			DatabaseConfig = new Database();
			DatabaseConfig.Address = databaseObj.Get<string>("Address");
			DatabaseConfig.Username = databaseObj.Get<string>("Username");
			DatabaseConfig.Password = databaseObj.Get<string>("Password");
			DatabaseConfig.Name = databaseObj.Get<string>("Name");
		}
	}
}