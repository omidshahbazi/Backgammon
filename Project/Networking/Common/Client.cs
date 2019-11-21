using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using GameFramework.BinarySerializer;
using GameFramework.Common.Timing;

namespace Networking.Common
{
	public delegate void ConnectionEventHandler();
	public delegate void MessageReceivedEventHandler(BufferStream Buffer);

	public class Client
	{
		private const float PING_PERIOD = 5;
		private const float DEFAULT_RECONNECTION_TIME = 10;

		private enum States
		{
			Disconnected = 0,
			Connected = 1,
			Connecting = 2,
			Reconnecting = 3
		}

		private string host;
		private ushort port;

#if USING_TCP
		private TCPClient socket = null;
#else
		private UDPClient socket = null;
#endif

		private States state = States.Disconnected;
		private double nextReconnectingTime = 0;
		private double nextPingTime = 0;

		public event ConnectionEventHandler OnConnected;
		public event ConnectionEventHandler OnConnectionFailed;
		public event ConnectionEventHandler OnConnectionLost;
		public event ConnectionEventHandler OnConnectionRestored;
		public event MessageReceivedEventHandler OnMessageReceived;

		public bool IsConnected
		{
			get { return state == States.Connected; }
		}

		public float ReconnectionTime
		{
			get;
			set;
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

		public Client()
		{
			ReconnectionTime = DEFAULT_RECONNECTION_TIME;
		}

		public void Connect(string Host, ushort Port)
		{
			host = Host;
			port = Port;

			if (state != States.Reconnecting)
				state = States.Connecting;

#if USING_TCP
			socket = new TCPClient();
#else
			socket = new UDPClient();
#endif

			AddListeners();

			socket.Connect(host, port);
		}

		public void Disconnect()
		{
			if (socket == null)
				return;

			socket.Disconnect(false);

			RemoveListeners();
		}

		public void Service()
		{
			if (state == States.Reconnecting && nextReconnectingTime <= Time.CurrentEpochTime)
			{
				Connect(host, port);

				nextReconnectingTime = Time.CurrentEpochTime + ReconnectionTime;
			}

			if (state == States.Connected)
			{
				if (nextPingTime > Time.CurrentEpochTime)
					return;

				nextPingTime = Time.CurrentEpochTime + PING_PERIOD;

				try
				{
					socket.Ping();
				}
				catch
				{
					OnDisconnectedEvent(null);
				}
			}
		}

		public void Send(BufferStream Buffer)
		{
			if (!IsConnected)
				return;

			BufferStream buffer = NetworkingCommon.PrepareForSend(Buffer);
			if (buffer == null)
				return;

			try
			{
#if USING_TCP
				socket.Send(new Binary(socket.Time.Timestep, true, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, true));
#else
				socket.Send(new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, false), true);
#endif
			}
			catch
			{
				//OnDisconnectedEvent(null);
			}
		}

		private void OnConnetedEvent(NetWorker Sender)
		{
			bool isReconnecting = (state == States.Reconnecting);

			state = States.Connected;

			if (isReconnecting)
			{
				if (OnConnectionRestored != null)
					OnConnectionRestored();
			}
			else if (OnConnected != null)
				OnConnected();
		}

		private void OnConnectAttemptFailed(NetWorker Sender)
		{
			if (state == States.Connecting)
				if (OnConnectionFailed != null)
					OnConnectionFailed();

			RemoveListeners();

			state = States.Reconnecting;
		}

		private void OnDisconnectedEvent(NetWorker Sender)
		{
			if (OnConnectionLost != null)
				OnConnectionLost();

			RemoveListeners();

			state = States.Reconnecting;
		}

		protected virtual void OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			BufferStream buffer = NetworkingCommon.PrepareForReceive(new BufferStream(Frame.StreamData.byteArr, (uint)Frame.StreamData.Size));
			if (buffer == null)
				return;

			if (OnMessageReceived != null)
				OnMessageReceived(buffer);
		}

		private void AddListeners()
		{
			socket.serverAccepted += OnConnetedEvent;
			socket.disconnected += OnDisconnectedEvent;
			socket.connectAttemptFailed += OnConnectAttemptFailed;
			socket.binaryMessageReceived += OnBinaryMessageReceived;
		}

		public void RemoveListeners()
		{
			socket.serverAccepted -= OnConnetedEvent;
			socket.disconnected -= OnDisconnectedEvent;
			socket.connectAttemptFailed -= OnConnectAttemptFailed;
			socket.binaryMessageReceived -= OnBinaryMessageReceived;
		}
	}
}