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

			RoomBase room = FindRoom(Player);
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

			if (command == Commands.Lobby.AUTHENTICATE)
			{
				Authenticate(Buffer, Player);
			}
			else if (command == Commands.Lobby.GET_INITIAL_DATA)
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				Send(Player, GameData.SplitTestGroupsInitialDataBuffer[player.SplitTestGroupID]);
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
		}

		public void HandleRoomRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			RoomBase room = FindRoom(Player);
			if (room == null)
				return;

			Player player = FindPlayer(Player);
			if (player == null)
				return;

			room.HandleRequest(Buffer, player);
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
				CreateNewBotRoom(Player, tableEnterance);

				return;
			}

			for (int i = 0; i < waitings.Count; ++i)
			{
				WaitingInfo info = waitings[i];

				if (info.Player == Player)
					continue;

				if (info.TableEnterance == tableEnterance)
				{
					CreateNewRoom(info.Player, Player, tableEnterance);

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

		private void CreateNewRoom(Player Player1, Player Player2, uint TableEnteracnce)
		{
			int gameID = DatabaseLayer.CreateGame(Player1.ID, Player2.ID, DatabaseLayer.GameTypes.OneByOne);

			CostInfo cost = new CostInfo(TableEnteracnce);
			DatabaseLayer.GetCost(Player1.ID, cost);
			DatabaseLayer.GetCost(Player2.ID, cost);

			Room room = new Room(Application, gameID, TableEnteracnce);

			room.AddPlayer(Player1);
			room.AddPlayer(Player2);

			rooms.Add(room);

			SendJoinedToRoom(Player1, Player2, gameID);
			SendJoinedToRoom(Player2, Player1, gameID);
		}

		private void CreateNewBotRoom(Player Player, uint TableEnteracnce)
		{
			int gameID = DatabaseLayer.CreateGame(Player.ID, Constants.NULL_PLAYER_ID, DatabaseLayer.GameTypes.OneByBot);

			DatabaseLayer.GetCost(Player.ID, new CostInfo(TableEnteracnce));

			BotRoom room = new BotRoom(Application, gameID, TableEnteracnce);

			room.AddPlayer(Player);

			rooms.Add(room);

			SendJoinedToRoom(Player, Constants.NULL_PLAYER_ID, gameID);
		}

		private void SendJoinedToRoom(Player To, Player Other, int GameID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteInt32(GameID);
			sendBuffer.WriteInt32(Other.ID);
			Send(To, sendBuffer);
		}

		private void SendJoinedToRoom(Player To, int OtherID, int GameID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteInt32(GameID);
			sendBuffer.WriteInt32(OtherID);
			Send(To, sendBuffer);
		}

		private Player FindPlayer(NetworkingPlayer Player)
		{
			if (playersMap.ContainsKey(Player))
				return playersMap[Player];

			return null;
		}

		private RoomBase FindRoom(NetworkingPlayer Player)
		{
			for (int i = 0; i < rooms.Count; ++i)
			{
				RoomBase room = rooms[i];

				if (room.ContainsPlayer(Player))
					return room;
			}

			return null;
		}
	}
}