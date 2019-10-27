using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using Networking.Common;
using System;
using GameFramework.BinarySerializer;
using GameFramework.Common.Timing;
using System.Collections.Generic;
using Networking.Server.Data;
using BeardedManStudios.Forge.Logging;
using GameFramework.Common.FileLayer;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	class Application
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

		public Logger Logger
		{
			get;
			private set;
		}

		public Lobby Lobby
		{
			get;
			private set;
		}

		public Admin Admin
		{
			get;
			private set;
		}

		public Application()
		{
			FileSystem.DataPath = GameData.ResourcesPath;

			Logger = new Logger();
			BMSLog.Instance.RegisterLoggerService(Logger);
			Logger.Log("Log listener added");

			GameData.Initialize();
			Logger.Log("GameData created.");

			DatabaseLayer.Initialize();
			Logger.Log("DatabaseLayer created.");

#if USING_TCP
			socket = new TCPServer(Configs.NetworkConfig.MaxConnectionCount);
#else
			socket = new UDPServer(Configs.NetworkConfig.MaxConnectionCount);
#endif

			socket.playerConnected += OnPlayerConnected;
			socket.playerDisconnected += OnPlayerDisconnected;
			socket.binaryMessageReceived += OnBinaryMessageReceived;
			Logger.Log("Socket created.");

			Admin = new Admin(this);
			Logger.Log("Admin created.");

			Lobby = new Lobby(this);
			Logger.Log("Lobby created.");

			schedules = new ScheduleList();

			Logger.Log("Application created.");
		}

		public void Bind()
		{
			socket.Connect(Configs.NetworkConfig.BindAddress, (ushort)Configs.NetworkConfig.Port);

			socket.StartAcceptingConnections();

#if USING_TCP
		Logger.	Log("Listening for clients on TCP port [" + Configs.NetworkConfig.Port + "].");
#else
			Logger.Log("Listening for clients on UDP port [" + Configs.NetworkConfig.Port + "].");
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
			if (buffer == null)
				return;

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

		public ISerializeObject GetStatistics()
		{
			ISerializeObject statObj = Creator.Create<ISerializeObject>();

			statObj.Set("SchedulerCount", schedules.Count);

			statObj.Set("HasError", Logger.HasErrorLog);
			statObj.Set("HasError", Logger.HasErrorLog);

			ISerializeObject socketStatObj = statObj.AddObject("Socket");
			{
				socketStatObj.Set("InBandwidth", socket.BandwidthIn);
				socketStatObj.Set("OutBandwidth", socket.BandwidthOut);
			}

			statObj.Set("Lobby", Lobby.GetStatistics());

			return statObj;
		}

		private void OnPlayerConnected(NetworkingPlayer Player, NetWorker Sender)
		{
			Logger.Log("Player [" + Player.IPEndPointHandle + "] connected.");
		}

		private void OnPlayerDisconnected(NetworkingPlayer Player, NetWorker Sender)
		{
			Lobby.HandlePlayerDisconnection(Player);

			Logger.Log("Player [" + Player.IPEndPointHandle + "] disconnected.");
		}

		private void OnBinaryMessageReceived(NetworkingPlayer Player, Binary Frame, NetWorker Sender)
		{
			if (Frame.GroupId != Constants.BINARY_FRAME_GROUP_ID)
				return;

			BufferStream buffer = NetworkingCommon.PrepareForReceive(new BufferStream(Frame.StreamData.byteArr, (uint)Frame.StreamData.Size));
			if (buffer == null)
				return;

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
			else if (category == Commands.Category.ADMIN)
			{
				Admin.HandleRequest(buffer, Player);
			}
		}
	}
}
