#define SINGLE_THREADED_BUFFER_PROCESSING
using System.Collections.Generic;
using GameFramework.BinarySerializer;
using Networking.Common;

namespace Networking.Client
{
	public delegate void BufferReceivedEventHandler(BufferStream Buffer);

	public class Connection
	{
		public const string HOST = "193.176.243.149";
		//public const string HOST = "127.0.0.1";

		public const ushort PORT = 80;

		private const byte ON_CONNECTION_CATEGORY = byte.MaxValue;
		private const byte ON_CONNECTED_COMMAND = byte.MaxValue;
		private const byte ON_CONNECTION_FAILED_COMMAND = byte.MaxValue - 1;
		private const byte ON_CONNECTION_LOST_COMMAND = byte.MaxValue - 2;
		private const byte ON_CONNECTION_RESTORED_COMMAND = byte.MaxValue - 3;

		private Common.Client client = null;
		private bool isConnectionLost = false;

#if SINGLE_THREADED_BUFFER_PROCESSING
		private object lockObject = null;
		private List<BufferStream> incommingBuffers = null;
#endif

		public event ConnectionEventHandler OnConnected;
		public event ConnectionEventHandler OnConnectionFailed;
		public event ConnectionEventHandler OnConnectionLost;
		public event ConnectionEventHandler OnConnectionRestored;
		public event BufferReceivedEventHandler OnBufferReceived;

		public bool IsConnected
		{
			get { return client.IsConnected; }
		}

		public float PacketLossSimulation
		{
			get { return client.PacketLossSimulation; }
			set { client.PacketLossSimulation = value; }
		}

		public int LatencySimulation
		{
			get { return client.LatencySimulation; }
			set { client.LatencySimulation = value; }
		}

		public float ProximityDistance
		{
			get { return client.ProximityDistance; }
			set { client.ProximityDistance = value; }
		}

		public Connection()
		{
			client = new Common.Client();
			client.OnConnected += Client_OnConnected;
			client.OnConnectionFailed += Client_OnConnectionFailed;
			client.OnConnectionLost += Client_OnConnectionLost;
			client.OnConnectionRestored += Client_OnConnectionRestored;
			client.OnMessageReceived += Client_OnMessageReceived;

#if SINGLE_THREADED_BUFFER_PROCESSING
			lockObject = new object();
			incommingBuffers = new List<BufferStream>();
#endif
		}

		public void Connect()
		{
			client.Connect(HOST, PORT);
		}

		public void Disconnect()
		{
			client.Disconnect();
		}

		public void Service()
		{
			client.Service();

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				for (int i = 0; i < incommingBuffers.Count; ++i)
				{
					try
					{
						HandleIncommingBuffer(incommingBuffers[i]);
					}
					catch
					{
					}
				}

				incommingBuffers.Clear();
			}
#endif
		}

		protected void Send(BufferStream Buffer)
		{
			client.Send(Buffer);
		}

		private void HandleIncommingBuffer(BufferStream Buffer)
		{
			byte category = Buffer.ReadByte();

			if (category == ON_CONNECTION_CATEGORY)
			{
				byte command = Buffer.ReadByte();

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
				else if (command == ON_CONNECTION_FAILED_COMMAND)
				{
					if (OnConnectionFailed != null)
						OnConnectionFailed();
				}
				else if (command == ON_CONNECTION_RESTORED_COMMAND)
				{
					if (OnConnectionRestored != null)
						OnConnectionRestored();
				}
			}
			else
			{
				Buffer.Reset();

				if (OnBufferReceived != null)
					OnBufferReceived(Buffer);
			}
		}

		private void Client_OnConnected()
		{
			isConnectionLost = false;

			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTED_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incommingBuffers.Add(buffer);
			}
#else
			HandleIncommingBuffer(buffer);
#endif
		}

		private void Client_OnConnectionFailed()
		{
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_FAILED_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incommingBuffers.Add(buffer);
			}
#else
			HandleIncommingBuffer(buffer);
#endif
		}

		private void Client_OnConnectionLost()
		{
			if (isConnectionLost)
				return;

			isConnectionLost = true;

			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_LOST_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incommingBuffers.Add(buffer);
			}
#else
			HandleIncommingBuffer(buffer);
#endif
		}

		private void Client_OnConnectionRestored()
		{
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_RESTORED_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incommingBuffers.Add(buffer);
			}
#else
			HandleIncommingBuffer(buffer);
#endif
		}

		private void Client_OnMessageReceived(BufferStream Buffer)
		{
#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incommingBuffers.Add(Buffer);
			}
#else
			HandleIncommingBuffer(Buffer);
#endif
		}
	}
}
