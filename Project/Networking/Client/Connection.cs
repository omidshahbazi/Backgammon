#define SINGLE_THREADED_BUFFER_PROCESSING
using System.Collections.Generic;
using GameFramework.BinarySerializer;
using Networking.Common;

namespace Networking.Client
{
	public delegate void BufferReceivedEventHandler(BufferStream Buffer);

	public class Connection
	{
		private class Packet
		{
			public BufferStream buffer;
			public System.DateTime time;
		}

		//public const string HOST = "193.176.243.149";
		public const string HOST = "127.0.0.1";

		public const ushort PORT = 433;

		private const byte ON_CONNECTION_CATEGORY = byte.MaxValue;
		private const byte ON_CONNECTED_COMMAND = byte.MaxValue;
		private const byte ON_CONNECTION_LOST_COMMAND = byte.MaxValue - 1;
		private const byte ON_CONNECTION_RESTORED_COMMAND = byte.MaxValue - 2;

		private Networking.Common.Client client = null;

#if SINGLE_THREADED_BUFFER_PROCESSING
		private object lockObject = null;
		private List<Packet> incommingBuffers = null;
#endif

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
			client = new Networking.Common.Client();
			client.OnConnected += Client_OnConnected;
			client.OnConnectionLost += Client_OnConnectionLost;
			client.OnConnectionRestored += Client_OnConnectionRestored;
			client.OnMessageReceived += Client_OnMessageReceived;

#if SINGLE_THREADED_BUFFER_PROCESSING
			lockObject = new object();
			incommingBuffers = new List<Packet>();
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
#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				for (int i = 0; i < incommingBuffers.Count; ++i)
					HandleIncommingBuffer(incommingBuffers[i].buffer);

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
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTED_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				//incommingBuffers.Add(buffer);
				incommingBuffers.Add(new Packet() { buffer = buffer, time = System.DateTime.Now });
			}
#else
			HandleIncommingBuffer(buffer);
#endif
		}

		private void Client_OnConnectionLost()
		{
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_CONNECTION_LOST_COMMAND });

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				//incommingBuffers.Add(buffer);
				incommingBuffers.Add(new Packet() { buffer = buffer, time = System.DateTime.Now });
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
				//incommingBuffers.Add(buffer);
				incommingBuffers.Add(new Packet() { buffer = buffer, time = System.DateTime.Now });
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
				//incommingBuffers.Add(Buffer);
				incommingBuffers.Add(new Packet() { buffer = Buffer, time = System.DateTime.Now });
			}
#else
			HandleIncommingBuffer(Buffer);
#endif
		}
	}
}
