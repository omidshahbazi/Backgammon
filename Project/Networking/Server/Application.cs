using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using Networking.Common;
using System;
using GameFramework.BinarySerializer;
using GameFramework.Common.Compression;

namespace Networking.Server
{
	class Application
	{
#if USING_TCP
		private TCPServer socket = null;
#else
		private UDPServer socket = null;
#endif

		private Lobby lobby = null;

		public Application()
		{
#if USING_TCP
			socket = new TCPServer(Configs.NetworkConfig.MaxConnectionCount);
#else
			socket = new UDPServer(Configs.NetworkConfig.MaxConnectionCount);
#endif

			socket.serverAccepted += OnServerAccepted;
			socket.playerConnected += OnPlayerConnected;
			socket.playerDisconnected += OnPlayerDisconnected;
			socket.playerAccepted += OnPlayerAccepted;
			socket.binaryMessageReceived += OnBinaryMessageReceived;

			lobby = new Lobby(this);

			Log("Application created.");
		}

		public void Bind()
		{
			socket.Connect(Configs.NetworkConfig.BindAddress, (ushort)Configs.NetworkConfig.Port);

			socket.StartAcceptingConnections();

#if USING_TCP
			Log("Listening for clients on TCP port [" + Configs.NetworkConfig.Port + "].");
#else
			Log("Listening for clients on UDP port [" + Configs.NetworkConfig.Port + "].");
#endif
		}

		public void Unbind()
		{
			socket.Disconnect(false);
		}

		public void Send(NetworkingPlayer Player, BufferStream Buffer)
		{
			byte[] buffer = new byte[Buffer.Size];
			Array.Copy(Buffer.Buffer, 0, buffer, 0, buffer.Length);

			//buffer = Compressor.Compress(buffer);

#if USING_TCP
			socket.Send(Player.TcpClientHandle, new Binary(socket.Time.Timestep, true, buffer, Receivers.All, Constants.BINARY_FRAME_GROUP_ID, true));
#else
			socket.Send(Player, new Binary(socket.Time.Timestep, false, buffer, Receivers.All, Constants.BINARY_FRAME_GROUP_ID, false), true);
#endif
		}

		private void OnServerAccepted(NetWorker Sender)
		{
			Log("Server accepted.");
		}

		private void OnPlayerConnected(NetworkingPlayer Player, NetWorker Sender)
		{
			Log("Player [" + Player.IPEndPointHandle + "] connected.");
		}

		private void OnPlayerDisconnected(NetworkingPlayer Player, NetWorker Sender)
		{
			lobby.HandlePlayerDisconnection(Player);

			Log("Player [" + Player.IPEndPointHandle + "] disconnected.");
		}

		private static void OnPlayerAccepted(NetworkingPlayer Player, NetWorker Sender)
		{
			Log("Player [" + Player.IPEndPointHandle + "] accepted.");
		}

		private void OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			if (Frame.GroupId != Constants.BINARY_FRAME_GROUP_ID)
				return;

			//byte[] data = Compressor.Decompress(Frame.StreamData.byteArr, Frame.StreamData.Size);
			//BufferStream buffer = new BufferStream(data);

			BufferStream buffer = new BufferStream(Frame.StreamData.byteArr, Frame.StreamData.Size);

			if (Configs.NetworkConfig.DebugInfo)
				buffer.Print();

			byte category = buffer.ReadByte();

			if (category == Commands.Category.LOBBY)
			{
				lobby.HandleLobbyRequest(buffer, Player);
			}
			else if (category == Commands.Category.ROOM)
			{
				lobby.HandleRoomRequest(buffer, Player);
			}
		}

		private static void Log(string Content)
		{
			System.Console.WriteLine(Content);
		}
	}
}
