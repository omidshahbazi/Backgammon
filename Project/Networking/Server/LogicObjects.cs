using BeardedManStudios.Forge.Networking;
using Networking.Common;

namespace Networking.Server
{
	abstract class LogicObjects
	{
		public Application Application
		{
			get;
			private set;
		}

		public LogicObjects(Application Application)
		{
			this.Application = Application;
		}

		public abstract void HandleRequest(BufferStream Buffer, NetworkingPlayer Player);

		public void Send(NetworkingPlayer Player, BufferStream Buffer)
		{
			Application.Send(Player, Buffer);
		}

		protected static void Log(string Content)
		{
			System.Console.WriteLine(Content);
		}
	}
}