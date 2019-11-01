using GameFramework.ASCIISerializer;
using GameFramework.Common.Timing;
using System;

namespace Networking.Admin
{
	class Application
	{
		private const float REFRESH_PERIOD = 4;

		private Network network = null;
		private bool isReady = false;

		private double nextRefreshTime = 0;

		public Application()
		{
			network = new Network();

			network.OnConnected += Network_OnConnected;
			network.OnConnectionLost += Network_OnConnectionLost;
			network.OnConnectionRestored += Network_OnConnectionRestored;
			network.OnStatisticsReady += Network_OnStatusDataReady;

			//network.Connect("193.176.243.149", 433);
			network.Connect("127.0.0.1", 433);
		}

		public void Update()
		{
			if (!isReady)
				return;

			if (nextRefreshTime > Time.CurrentEpochTime)
				return;

			nextRefreshTime = Time.CurrentEpochTime + REFRESH_PERIOD;

			network.GetStatistics();
		}

		private void Network_OnConnected()
		{
			isReady = true;
		}

		private void Network_OnConnectionLost()
		{
			isReady = false;
		}

		private void Network_OnConnectionRestored()
		{
			isReady = true;
		}

		private void Network_OnStatusDataReady(string Data)
		{
			ISerializeObject obj = Creator.Create<ISerializeObject>(Data);

			Console.Clear();
			Console.WriteLine(Time.CurrentUTCDateTime);

			PrintStatistics(obj);
		}

		private static void PrintStatistics(ISerializeObject Object)
		{
			var it = Object.GetEnumerator();
			while (it.MoveNext())
			{
				string key = it.Current.Key;
				object value = it.Current.Value;

				if (value is ISerializeObject)
				{
					Console.WriteLine();
					Console.Write(key);

					Console.WriteLine();
					PrintStatistics((ISerializeObject)value);

					continue;
				}

				Console.Write(key);
				Console.Write(": ");
				Console.Write(value);
				Console.Write(" ");
			}
		}
	}
}
