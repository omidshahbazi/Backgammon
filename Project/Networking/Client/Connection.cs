using BeardedManStudios.Forge.Networking.Frame;
using Networking.Common;
using System.Collections.Generic;

namespace Networking.Client
{
	public delegate void BufferReceivedEventHandler(BufferStream Buffer);

	public class Connection
	{
		private const byte ON_CONNECTION_CATEGORY = byte.MaxValue;
		private const byte ON_CONNECTED_COMMAND = byte.MaxValue;
		private const byte ON_CONNECTION_LOST_COMMAND = byte.MaxValue - 1;
		private const byte ON_CONNECTION_RESTORED_COMMAND = byte.MaxValue - 2;

		private Client client = null;
		private List<BufferStream> incommingBuffers = null;

		public event ConnectionEventHandler OnConnected;
		public event ConnectionEventHandler OnConnectionLost;
		public event ConnectionEventHandler OnConnectionRestored;
		public event BufferReceivedEventHandler OnBufferReceived;

		public bool IsConnected
		{
			get { return client.IsConnected; }
		}

		public Connection()
		{
			client = new Client();
			client.OnConnected += Client_OnConnected;
			client.OnConnectionLost += Client_OnConnectionLost;
			client.OnConnectionRestored += Client_OnConnectionRestored;
			client.OnMessageReceived += Client_OnMessageReceived;

			incommingBuffers = new List<BufferStream>();
		}

		public void Connect()
		{
			client.Connect();
		}

		public void Disconnect()
		{
			client.Disconnect();
		}

		public void Service()
		{
			for(int i = 0; i < incommingBuffers.Count; ++i)
			{
				BufferStream buffer = incommingBuffers[i];
				incommingBuffers.RemoveAt(0);

				byte category = buffer.ReadByte();

				if (category == ON_CONNECTION_CATEGORY)
				{
					byte command = buffer.ReadByte();

					if (command == ON_CONNECTED_COMMAND)
					{
						if (OnConnected != null)
							OnConnected();
					}
					else if (command == ON_CONNECTION_LOST_COMMAND)
					{
						if (OnConnectionLost != null)
							OnConnectionLost();
					}
					else if (command == ON_CONNECTION_RESTORED_COMMAND)
					{
						if (OnConnectionRestored != null)
							OnConnectionRestored();
					}
				}
				else
				{
					if (OnBufferReceived != null)
						OnBufferReceived(buffer);
				}
			}

			incommingBuffers.Clear();
		}

		protected void Send(BufferStream Buffer)
		{
			client.Send(Buffer);
		}

		private void Client_OnConnected()
		{
			incommingBuffers.Add(new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTED_COMMAND }));
		}

		private void Client_OnConnectionLost()
		{
			incommingBuffers.Add(new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_LOST_COMMAND }));
		}

		private void Client_OnConnectionRestored()
		{
			incommingBuffers.Add(new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_RESTORED_COMMAND }));
		}

		private void Client_OnMessageReceived(Binary Frame)
		{
			incommingBuffers.Add(new BufferStream(Frame.StreamData.byteArr));
		}
	}
}
