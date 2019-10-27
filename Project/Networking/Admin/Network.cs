using Networking.Common;
using GameFramework.BinarySerializer;

namespace Networking.Admin
{
	public delegate void DataReceivedEventHandler(string Data);

	public class Network : Client
	{
		private const int BUFFER_SIZE = 64;

		private BufferStream sendBuffer = null;

		public event DataReceivedEventHandler OnStatisticsReady;

		public Network()
		{
			sendBuffer = new BufferStream(new byte[BUFFER_SIZE]);

			OnMessageReceived += Connection_OnMessageReceived;
		}

		public void GetStatistics()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ADMIN, Commands.Admin.GET_STATISTICS);

			Send(sendBuffer);
		}

		private void Connection_OnMessageReceived(BufferStream Buffer)
		{
			byte category = Buffer.ReadByte();
			byte command = Buffer.ReadByte();

			if (category == Commands.Category.ADMIN)
			{
				if (command == Commands.Admin.GET_STATISTICS)
				{
					string data = Buffer.ReadString();

					if (OnStatisticsReady != null)
						OnStatisticsReady(data);
				}
			}
		}
	}
}
