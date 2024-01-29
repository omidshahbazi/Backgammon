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

		private ServerSocket socket = null;

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

			socket = new ServerSocket();
			socket.OnPlayerConnected += OnPlayerConnected;
			socket.OnPlayerDisconnected += OnPlayerDisconnected;
			socket.OnMessageReceived += OnMessageReceived;
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
			socket.Bind(Configs.NetworkConfig.BindAddress, (ushort)Configs.NetworkConfig.Port);

#if USING_TCP
			Logger.Log("Listening for clients on [" + Configs.NetworkConfig.BindAddress + "::" + Configs.NetworkConfig.Port + "] under TCP.");
#else
			Logger.Log("Listening for clients on [" + Configs.NetworkConfig.BindAddress + "::" + Configs.NetworkConfig.Port + "] under UDP.");
#endif
		}

		public void Unbind()
		{
			socket.OnPlayerConnected -= OnPlayerConnected;
			socket.OnPlayerDisconnected -= OnPlayerDisconnected;
			socket.OnMessageReceived -= OnMessageReceived;

			socket.Unbind();
		}

		public void Update()
		{
			socket.Service();

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
			socket.Send(Player, Buffer);
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

			Lobby.GetStatistics(statObj.AddObject("Lobby"));

			return statObj;
		}

		private void OnPlayerConnected(NetworkingPlayer Player)
		{
			if (Player.IsHost)
				return;

			Logger.Log("Player [" + Player.IPEndPointHandle + "] connected.");
		}

		private void OnPlayerDisconnected(NetworkingPlayer Player)
		{
			Lobby.HandlePlayerDisconnection(Player);

			Logger.Log("Player [" + Player.IPEndPointHandle + "] disconnected.");
		}

		private void OnMessageReceived(NetworkingPlayer Player, BufferStream Buffer)
		{
			if (Configs.NetworkConfig.DebugInfo)
				Buffer.Print();

			byte category = Buffer.ReadByte();

			if (category == Commands.Category.LOBBY)
			{
				Lobby.HandleLobbyRequest(Buffer, Player);
			}
			else if (category == Commands.Category.ROOM)
			{
				Lobby.HandleRoomRequest(Buffer, Player);
			}
			else if (category == Commands.Category.ADMIN)
			{
				Admin.HandleRequest(Buffer, Player);
			}
		}
	}
}
