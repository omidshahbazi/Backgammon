using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using GameFramework.BinarySerializer;

namespace Networking.Common
{
	public delegate void ConnectionEventHandler();
	public delegate void MessageReceivedEventHandler(BufferStream Buffer);

	public class Client
	{
		private string host;
		private ushort port;

#if USING_TCP
		private TCPClient socket = null;
#else
		private UDPClient socket = null;
#endif

		private bool isFirstConnection = true;
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

		public float PacketLossSimulation
		{
			get { return socket.PacketLossSimulation; }
			set { socket.PacketLossSimulation = value; }
		}

		public int LatencySimulation
		{
			get { return socket.LatencySimulation; }
			set { socket.LatencySimulation = value; }
		}

		public float ProximityDistance
		{
			get { return socket.ProximityDistance; }
			set { socket.ProximityDistance = value; }
		}

		public void Connect(string Host, ushort Port)
		{
			host = Host;
			port = Port;

#if USING_TCP
			socket = new TCPClient();
#else
			socket = new UDPClient();
#endif

			socket.Connect(host, port);

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
			socket.Send(new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, false), true);
#endif
		}

		private void OnConnetedEvent(NetWorker Sender)
		{
			isFirstConnection = false;
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

			if (!isFirstConnection)
				isReconnecting = true;

			Connect(host, port);
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