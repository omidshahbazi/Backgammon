#define SINGLE_THREADED_BUFFER_PROCESSING
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using GameFramework.BinarySerializer;
using Networking.Common;
using Networking.Server.Data;
using System.Collections.Generic;

namespace Networking.Server
{
	public delegate void ConnectionEventHandler(NetworkingPlayer Player);
	public delegate void MessageReceivedEventHandler(NetworkingPlayer Player, BufferStream Buffer);

	public class ServerSocket
	{
		private class Packet
		{
			public NetworkingPlayer Sender;
			public BufferStream Buffer;
		}

		private const byte ON_CONNECTION_CATEGORY = byte.MaxValue;
		private const byte ON_PLAYER_CONNECTED_COMMAND = byte.MaxValue;
		private const byte ON_PLAYER_DISCONNECTED_COMMAND = byte.MaxValue - 1;

		private string host;
		private ushort port;

#if USING_TCP
		private TCPServer socket = null;
#else
		private UDPServer socket = null;
#endif

#if SINGLE_THREADED_BUFFER_PROCESSING
		private object lockObject = null;
		private List<Packet> incomingPackets = null;
#endif

		public event ConnectionEventHandler OnPlayerConnected;
		public event ConnectionEventHandler OnPlayerDisconnected;
		public event MessageReceivedEventHandler OnMessageReceived;

		public ulong BandwidthIn
		{
			get { return socket.BandwidthIn; }
		}

		public ulong BandwidthOut
		{
			get { return socket.BandwidthOut; }
		}

		public ServerSocket()
		{
#if USING_TCP
			socket = new TCPServer(Configs.NetworkConfig.MaxConnectionCount);
#else
			socket = new UDPServer(Configs.NetworkConfig.MaxConnectionCount);
#endif

#if SINGLE_THREADED_BUFFER_PROCESSING
			lockObject = new object();
			incomingPackets = new List<Packet>();
#endif
		}

		public void Bind(string Host, ushort Port)
		{
			if (socket == null)
				return;

			host = Host;
			port = Port;

			socket.playerConnected += Socket_OnPlayerConnected;
			socket.playerDisconnected += Socket_OnPlayerDisconnected;
			socket.binaryMessageReceived += Socket_OnBinaryMessageReceived;

			socket.Connect(host, port);

			socket.StartAcceptingConnections();
		}

		public void Unbind()
		{
			if (socket == null)
				return;

			socket.playerConnected -= Socket_OnPlayerConnected;
			socket.playerDisconnected -= Socket_OnPlayerDisconnected;
			socket.binaryMessageReceived -= Socket_OnBinaryMessageReceived;

			socket.Disconnect(false);
		}

		public void Service()
		{
#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				for (int i = 0; i < incomingPackets.Count; ++i)
					HandleincomingPackets(incomingPackets[i]);

				incomingPackets.Clear();
			}
#endif
		}

		public void Send(NetworkingPlayer Player, BufferStream Buffer)
		{
			if (socket == null)
				return;

			BufferStream buffer = NetworkingCommon.PrepareForSend(Buffer);
			if (buffer == null)
				return;

#if USING_TCP
			socket.Send(Player.TcpClientHandle, new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, true));
#else
			socket.Send(Player, new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, false), true);
#endif
		}

		private void HandleincomingPackets(Packet Packet)
		{
			byte category = Packet.Buffer.ReadByte();

			if (category == ON_CONNECTION_CATEGORY)
			{
				byte command = Packet.Buffer.ReadByte();

				if (command == ON_PLAYER_CONNECTED_COMMAND)
				{
					if (OnPlayerConnected != null)
						OnPlayerConnected(Packet.Sender);
				}
				else if (command == ON_PLAYER_DISCONNECTED_COMMAND)
				{
					if (OnPlayerDisconnected != null)
						OnPlayerDisconnected(Packet.Sender);
				}
			}
			else
			{
				Packet.Buffer.ResetRead();

				if (OnMessageReceived != null)
					OnMessageReceived(Packet.Sender, Packet.Buffer);
			}
		}

		private void Socket_OnPlayerConnected(NetworkingPlayer Player, NetWorker Sender)
		{
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_PLAYER_CONNECTED_COMMAND });

			Packet packet = new Packet() { Sender = Player, Buffer = buffer };

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incomingPackets.Add(packet);
			}
#else
			HandleincomingPackets(packet);
#endif
		}

		private void Socket_OnPlayerDisconnected(NetworkingPlayer Player, NetWorker Sender)
		{
			BufferStream buffer = new BufferStream(new byte[] { ON_CONNECTION_CATEGORY, ON_PLAYER_DISCONNECTED_COMMAND });

			Packet packet = new Packet() { Sender = Player, Buffer = buffer };

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incomingPackets.Add(packet);
			}
#else
			HandleincomingPackets(packet);
#endif
		}

		protected void Socket_OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			if (Frame.GroupId != Constants.BINARY_FRAME_GROUP_ID)
				return;

			BufferStream buffer = NetworkingCommon.PrepareForReceive(new BufferStream(Frame.StreamData.byteArr, (uint)Frame.StreamData.Size));
			if (buffer == null)
				return;

			Packet packet = new Packet() { Sender = Player, Buffer = buffer };

#if SINGLE_THREADED_BUFFER_PROCESSING
			lock (lockObject)
			{
				incomingPackets.Add(packet);
			}
#else
			HandleincomingPackets(Buffer);
#endif
		}
	}
}