using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using Networking.Common;
using System;
using GameFramework.BinarySerializer;
using GameFramework.Common.Compression;
using GameFramework.Common.Timing;
using System.Collections.Generic;
using Networking.Server.Data;
using BeardedManStudios.Forge.Logging;

namespace Networking.Server
{
	class Application : IBMSLogger
	{
		private struct ScheduleInfo
		{
			public double DoTime;
			public Action Worker;
		}

		private class ScheduleList : List<ScheduleInfo>
		{ }

#if USING_TCP
		private TCPServer socket = null;
#else
		private UDPServer socket = null;
#endif
		private ScheduleList schedules = null;


		public Lobby Lobby
		{
			get;
			private set;
		}

		public Application()
		{
			BMSLog.Instance.RegisterLoggerService(this);
			Log("Log listener added");

			GameData.Initialize();
			Log("GameData created.");

			DatabaseLayer.Initialize();
			Log("DatabaseLayer created.");

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
			Log("Socket created.");

			Lobby = new Lobby(this);
			Log("Lobby created.");

			schedules = new ScheduleList();

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

		public void Update()
		{
			double now = Time.CurrentEpochTime;

			for (int i = 0; i < schedules.Count; ++i)
			{
				ScheduleInfo info = schedules[i];

				if (info.DoTime > now)
					continue;

				info.Worker();

				schedules.RemoveAt(i--);
			}
		}

		public void Send(NetworkingPlayer Player, BufferStream Buffer)
		{
			BufferStream buffer = NetworkingCommon.PrepareForSend(Buffer);

#if USING_TCP
			socket.Send(Player.TcpClientHandle, new Binary(socket.Time.Timestep, false,  buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, true));
#else
			socket.Send(Player, new Binary(socket.Time.Timestep, false, buffer.Buffer, Receivers.Target, Constants.BINARY_FRAME_GROUP_ID, false), true);
#endif
		}

		public void ScheduleWokerFor(float Delay, Action Worker)
		{
			schedules.Add(new ScheduleInfo() { DoTime = Time.CurrentEpochTime + Delay, Worker = Worker });
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
			Lobby.HandlePlayerDisconnection(Player);

			Log("Player [" + Player.IPEndPointHandle + "] disconnected.");
		}

		private void OnPlayerAccepted(NetworkingPlayer Player, NetWorker Sender)
		{
			Log("Player [" + Player.IPEndPointHandle + "] accepted.");
		}

		private void OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			if (Frame.GroupId != Constants.BINARY_FRAME_GROUP_ID)
				return;

			BufferStream buffer = NetworkingCommon.PrepareForReceive(new BufferStream(Frame.StreamData.byteArr, (uint)Frame.StreamData.Size));

			if (Configs.NetworkConfig.DebugInfo)
				buffer.Print();

			byte category = buffer.ReadByte();

			if (category == Commands.Category.LOBBY)
			{
				Lobby.HandleLobbyRequest(buffer, Player);
			}
			else if (category == Commands.Category.ROOM)
			{
				Lobby.HandleRoomRequest(buffer, Player);
			}
		}

		public void Log(string Content)
		{
			Console.WriteLine(Content);
		}

		public void LogFormat(string Content, params object[] Args)
		{
			Console.WriteLine(string.Format(Content, Args));
		}

		public void LogWarning(string Content)
		{
			Console.WriteLine("[Warning] " + Content);
		}

		public void LogWarningFormat(string Content, params object[] Args)
		{
			Console.WriteLine("[Warning] " + string.Format(Content, Args));
		}

		public void LogException(string Content)
		{
			Console.WriteLine("[Exception] " + Content);
		}

		public void LogExceptionFormat(string Content, params object[] Args)
		{
			Console.WriteLine("[Exception] " + string.Format(Content, Args));
		}
	}
}
