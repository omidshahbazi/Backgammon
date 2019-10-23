using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using Networking.Common;
using GameFramework.BinarySerializer;
using GameFramework.Common.Compression;
using System;

namespace Networking.Client
{
	public delegate void ConnectionEventHandler();
	public delegate void MessageReceivedEventHandler(BufferStream Buffer);

	public class Client
	{
		//public const string SERVER_IP = "193.176.243.149";
		public const string SERVER_IP = "127.0.0.1";

		public const int PORT_NUMBER = 433;

#if USING_TCP
		private TCPClient socket = null;
#else
		private UDPClient socket = null;
#endif

		private bool isReconnecting = false;

		public event ConnectionEventHandler OnConnected;
		public event ConnectionEventHandler OnConnectionLost;
		public event ConnectionEventHandler OnConnectionRestored;
		public event MessageReceivedEventHandler OnMessageReceived;

		public bool IsConnected
		{
			get;
			private set;
		}

		public void Connect()
		{
#if USING_TCP
			socket = new TCPClient();
#else
			socket = new UDPClient();
#endif

			socket.Connect(SERVER_IP, PORT_NUMBER);

			socket.serverAccepted += OnConnetedEvent;
			socket.disconnected += OnDisconnectedEvent;
			socket.binaryMessageReceived += OnBinaryMessageReceived;
		}

		public void Disconnect()
		{
			if (socket == null)
				return;

			socket.serverAccepted -= OnConnetedEvent;
			socket.disconnected -= OnDisconnectedEvent;
			socket.binaryMessageReceived -= OnBinaryMessageReceived;

			socket.Disconnect(false);
		}

		public void Send(BufferStream Buffer)
		{
			BufferStream buffer = NetworkingCommon.PrepareForSend(Buffer);
			if (buffer == null)
				return;

#if USING_TCP
			socket.Send(new Binary(socket.Time.Timestep, true, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, true));
#else
			socket.Send(new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, false), false);
#endif
		}

		private void OnConnetedEvent(NetWorker Sender)
		{
			IsConnected = true;

			if (isReconnecting)
			{
				if (OnConnectionRestored != null)
					OnConnectionRestored();

				isReconnecting = false;
			}
			else if (OnConnected != null)
				OnConnected();
		}

		private void OnDisconnectedEvent(NetWorker Sender)
		{
			IsConnected = false;

			if (OnConnectionLost != null)
				OnConnectionLost();

			Disconnect();

			isReconnecting = true;
			Connect();
		}

		protected virtual void OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			BufferStream buffer = NetworkingCommon.PrepareForReceive(new BufferStream(Frame.StreamData.byteArr, (uint)Frame.StreamData.Size));
			if (buffer == null)
				return;

			if (OnMessageReceived != null)
				OnMessageReceived(buffer);
		}
	}
}