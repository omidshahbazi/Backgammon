using BeardedManStudios.Forge.Networking;
using Networking.Common;
using System.Collections.Generic;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;

namespace Networking.Server
{
	class Lobby : LogicObjects
	{
		private struct WaitingInfo
		{
			public Player Player;
			public uint TableEnterance;
		}

		private class WaitingInfoList : List<WaitingInfo>
		{ }

		private BufferStream sendBuffer = null;
		private RoomList rooms = null;
		private NetworPlayerMap playersMap = null;
		private WaitingInfoList waitings = null;

		public Lobby(Application Application) :
			base(Application)
		{
			sendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);
			rooms = new RoomList();
			playersMap = new NetworPlayerMap();
			waitings = new WaitingInfoList();
		}

		public void HandlePlayerDisconnection(NetworkingPlayer Player)
		{
			Player player = FindPlayer(Player);
			if (player == null)
				return;

			Room room = FindRoom(Player);
			if (room != null)
			{
				room.HandlePlayerDisconnection(player);
				rooms.Remove(room);

				Log("Room " + room + " removed");
			}

			for (int i = 0; i < waitings.Count; ++i)
			{
				if (waitings[i].Player != player)
					continue;

				waitings.RemoveAt(i);

				break;
			}

			playersMap.Remove(player.NetworkingPlayer);

			DatabaseLayer.LogDisconnection(player.ID);
		}

		public void HandleLobbyRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Lobby.VERSION_CHECK)
			{
				VersionCheck(Buffer, Player);
			}
			else if (command == Commands.Lobby.AUTHENTICATE)
			{
				Authenticate(Buffer, Player);
			}
			else if (command == Commands.Lobby.GET_INITIAL_DATA)
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				Send(Player, GameData.GetSplitTestGroupsInitialDataBuffer(player.SplitTestGroupID));
			}
			else if (command == Commands.Lobby.JOIN_TO_ROOM)
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				JoinToRoom(Buffer, player);
			}
			else if (command == Commands.Lobby.CANCEL_JOIN_TO_ROOM)
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				CancelJoinToRoom(Buffer, player);
			}
			else if (command == Commands.Lobby.GET_LEADERBOARD)
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				SendLeaderboardData(Buffer, player);
			}
		}

		public void HandleRoomRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			Room room = FindRoom(Player);
			if (room == null)
				return;

			Player player = FindPlayer(Player);
			if (player == null)
				return;

			room.HandleRequest(Buffer, player);
		}

		private void VersionCheck(BufferStream Buffer, NetworkingPlayer Player)
		{
			int clientVersion = Buffer.ReadInt32();

			VersionCheckResults result = VersionCheckResults.OK;

			ISerializeObject versionObj = GameData.VersionObject;
			if (versionObj.Get<bool>("IsUnderMaintenance"))
				result = VersionCheckResults.UnderMaintenance;
			else
			{
				if (clientVersion < versionObj.Get<int>("MinimumVersion") || versionObj.Get<int>("MaximumVersion") < clientVersion)
					result = VersionCheckResults.UpdateNeeded;
				else
				{
					result = VersionCheckResults.OK;

					if (versionObj.Get<bool>("CheckVersion"))
					{
						if (clientVersion == versionObj.Get<int>("MaximumVersion"))
							result = VersionCheckResults.OK;
						else
							result = VersionCheckResults.NewerVersionAvailable;
					}
				}
			}

			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.VERSION_CHECK);
			sendBuffer.WriteInt32((int)result);

			Send(Player, sendBuffer);
		}

		private void Authenticate(BufferStream Buffer, NetworkingPlayer Player)
		{
			string username = Buffer.ReadString();
			string password = Buffer.ReadString();

			ISerializeObject resultObj = DatabaseLayer.Authenticate(username, password, Player.Ip, Player.RoundTripLatency);
			AuthenticateResult result = resultObj.Get<AuthenticateResult>("result");

			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			sendBuffer.WriteInt32((int)result);

			if (result == AuthenticateResult.Passed)
			{
				int id = resultObj.Get<int>("id");

				sendBuffer.WriteInt32(id);
				sendBuffer.WriteString(username);

				playersMap[Player] = new Player(Player, id, resultObj.Get<int>("split_test_group_id"));
			}

			Send(Player, sendBuffer);
		}

		private void JoinToRoom(BufferStream Buffer, Player Player)
		{
			for (int i = 0; i < waitings.Count; ++i)
				if (waitings[i].Player == Player)
					return;

			uint tableEnterance = Buffer.ReadUInt32();
			bool withBot = Buffer.ReadBool();

			if (withBot)
			{
				CreateOneByBotRoom(Player, tableEnterance);

				return;
			}

			for (int i = 0; i < waitings.Count; ++i)
			{
				WaitingInfo info = waitings[i];

				if (info.Player == Player)
					continue;

				if (info.TableEnterance == tableEnterance)
				{
					CreateOneByOneRoom(info.Player, Player, tableEnterance);

					waitings.RemoveAt(i);

					return;
				}
			}

			waitings.Add(new WaitingInfo { Player = Player, TableEnterance = tableEnterance });
		}

		private void CancelJoinToRoom(BufferStream Buffer, Player Player)
		{
			for (int i = 0; i < waitings.Count; ++i)
			{
				if (waitings[i].Player != Player)
					continue;

				waitings.RemoveAt(i);

				break;
			}
		}

		private void SendLeaderboardData(BufferStream Buffer, Player Player)
		{
			LeaderboardTypes type = (LeaderboardTypes)Buffer.ReadInt32();

			ISerializeArray arr = DatabaseLayer.GetLeaderboard(type, 50);

			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_LEADERBOARD);
			sendBuffer.WriteInt32((int)type);
			sendBuffer.WriteInt64(DatabaseLayer.GetLeaderboardStartTime(type));
			sendBuffer.WriteString(arr == null ? "[]" : arr.Content);

			Send(Player, sendBuffer);
		}

		private void CreateOneByOneRoom(Player Player1, Player Player2, uint TableEnteracnce)
		{
			CostInfo cost = new CostInfo(TableEnteracnce);
			DatabaseLayer.GetCost(Player1.ID, cost);
			DatabaseLayer.GetCost(Player2.ID, cost);

			OneByOneRoom room = new OneByOneRoom(Application, TableEnteracnce);

			room.AddPlayer(Player1);
			room.AddPlayer(Player2);

			rooms.Add(room);

			room.Initialize();

			SendJoinedToRoom(Player1, DatabaseLayer.GetUserInfo(Player2.ID).Content, room.GameID);
			SendJoinedToRoom(Player2, DatabaseLayer.GetUserInfo(Player1.ID).Content, room.GameID);
		}

		private void CreateOneByBotRoom(Player Player, uint TableEnteracnce)
		{
			DatabaseLayer.GetCost(Player.ID, new CostInfo(TableEnteracnce));

			OneByBotRoom room = new OneByBotRoom(Application, TableEnteracnce);

			room.AddPlayer(Player);

			room.Initialize();

			rooms.Add(room);

			SendJoinedToRoom(Player, room.BotPlayerInfo, room.GameID);
		}

		private void SendJoinedToRoom(Player To, string OtherPlayerInfo, int GameID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteInt32(GameID);
			sendBuffer.WriteString(OtherPlayerInfo);
			Send(To, sendBuffer);
		}

		private Player FindPlayer(NetworkingPlayer Player)
		{
			if (playersMap.ContainsKey(Player))
				return playersMap[Player];

			return null;
		}

		private Room FindRoom(NetworkingPlayer Player)
		{
			for (int i = 0; i < rooms.Count; ++i)
			{
				Room room = rooms[i];

				if (room.ContainsPlayer(Player))
					return room;
			}

			return null;
		}
	}
}