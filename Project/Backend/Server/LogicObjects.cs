using BeardedManStudios.Forge.Networking;
using GameFramework.BinarySerializer;
using System;

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

		protected void ScheduleWokerFor(float Delay, Action Worker)
		{
			Application.ScheduleWokerFor(Delay, Worker);
		}

		protected void Log(string Content)
		{
			Application.Logger.Log(Content);
		}

		protected void LogError(string Content)
		{
			Application.Logger.LogError(Content);
		}
	}
}