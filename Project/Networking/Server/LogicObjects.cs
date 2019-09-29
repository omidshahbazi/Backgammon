using BeardedManStudios.Forge.Networking;
using Zorvan.Framework.BinarySerializer;

namespace Networking.Server
{
	abstract class LogicObjects
	{
		protected Application Application
		{
			get;
			private set;
		}

		public LogicObjects(Application Application)
		{
			this.Application = Application;
		}

		protected void Send(NetworkingPlayer Player, BufferStream Buffer)
		{
			Application.Send(Player, Buffer);
		}

		protected void Send(Player Player, BufferStream Buffer)
		{
			Send(Player.NetworkingPlayer, Buffer);
		}

		protected static void Log(string Content)
		{
			System.Console.WriteLine(Content);
		}
	}
}