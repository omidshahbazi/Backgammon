using System;
using BeardedManStudios.Forge.Logging;
using GameFramework.Common.FileLayer;

namespace Networking.Server
{
	class Logger : IBMSLogger
	{
		private LogManager logManager = null;

		public Logger()
		{
			logManager = new LogManager("Logs", "General.log");
		}

		public void Log(string Content)
		{
			Console.WriteLine(Content);

			logManager.Log(Content);
		}

		public void LogFormat(string Content, params object[] Args)
		{
			Log(string.Format(Content, Args));
		}

		public void LogWarning(string Content)
		{
			Console.WriteLine("[Warning] " + Content);

			logManager.LogWarning(Content);
		}

		public void LogWarningFormat(string Content, params object[] Args)
		{
			LogWarning(string.Format(Content, Args));
		}

		public void LogException(string Content)
		{
			Console.WriteLine("[Exception] " + Content);

			logManager.LogError(Content);
		}

		public void LogExceptionFormat(string Content, params object[] Args)
		{
			LogException(string.Format(Content, Args));
		}
	}
}
