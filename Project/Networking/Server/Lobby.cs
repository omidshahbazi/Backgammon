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

		private BufferStream smallSendBuffer = null;
		private BufferStream largeSendBuffer = null;
		private RoomList rooms = null;
		private NetworPlayerMap playersMap = null;
		private WaitingInfoList waitings = null;

		public Lobby(Application Application) :
			base(Application)
		{
			smallSendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);
			largeSendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize * 100]);

			rooms = new RoomList();
			playersMap = new NetworPlayerMap();
			waitings = new WaitingInfoList();
		}

		public void HandlePlayerDisconnection(NetworkingPlayer Player)
		{
			Player player = FindPlayer(Player);
			if (player == null)
				return;

			Room room = FindRoom(player);
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
				HandleVersionCheck(Buffer, Player);
			}
			else if (command == Commands.Lobby.AUTHENTICATE)
			{
				HandleAuthenticate(Buffer, Player);
			}
			else
			{
				Player player = FindPlayer(Player);
				if (player == null)
					return;

				if (command == Commands.Lobby.SET_USER_INFO)
				{
					HandleSetUserInfo(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_USER_INFO)
				{
					HandleGetUserInfo(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_MIGRATE_CODE)
				{
					HandleGetMigrateCode(Buffer, player);
				}
				else if (command == Commands.Lobby.APPLY_MIGRATE_CODE)
				{
					HandleApplyMigrateCode(Buffer, player);
				}
				else if (command == Commands.Lobby.SET_PUSH_ID)
				{
					HandleSetPushID(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_INITIAL_DATA)
				{
					HandleGetInitialData(Buffer, player);
				}
				else if (command == Commands.Lobby.JOIN_TO_ROOM)
				{
					HandleJoinToRoom(Buffer, player);
				}
				else if (command == Commands.Lobby.CANCEL_JOIN_TO_ROOM)
				{
					HandleCancelJoinToRoom(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_LEADERBOARD)
				{
					HandleGetLeaderboardData(Buffer, player);
				}
				else if (command == Commands.Lobby.PURCHASE_FINISHED)
				{
					HandlePurchaseFinished(Buffer, player);
				}
			}
		}

		public void HandleRoomRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			Player player = FindPlayer(Player);
			if (player == null)
				return;

			Room room = FindRoom(player);
			if (room == null)
				return;

			room.HandleRequest(Buffer, player);
		}

		private void HandleVersionCheck(BufferStream Buffer, NetworkingPlayer Player)
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

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.VERSION_CHECK);
			smallSendBuffer.WriteInt32((int)result);

			Send(Player, smallSendBuffer);
		}

		private void HandleAuthenticate(BufferStream Buffer, NetworkingPlayer Player)
		{
			string deviceID = Buffer.ReadString();
			Markets market = (Markets)Buffer.ReadInt32();

			ISerializeObject resultObj = DatabaseLayer.Authenticate(deviceID, market, Player.Ip, Player.RoundTripLatency);
			AuthenticateResult result = resultObj.Get<AuthenticateResult>("result");

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			smallSendBuffer.WriteInt32((int)result);

			int id = resultObj.Get<int>("id");
			smallSendBuffer.WriteInt32(id);

			smallSendBuffer.WriteString(resultObj.Get<string>("username"));

			if (result == AuthenticateResult.Passed)
				playersMap[Player] = new Player(Player, id, resultObj.Get<int>("split_test_group_id"));

			Send(Player, smallSendBuffer);
		}

		private void HandleSetUserInfo(BufferStream Buffer, Player Player)
		{
			string username = Buffer.ReadString();

			DatabaseLayer.SetUserInfo(Player.ID, username);
		}

		private void HandleGetUserInfo(BufferStream Buffer, Player Player)
		{
			int userID = Buffer.ReadInt32();

			ISerializeObject resultObj = DatabaseLayer.GetUserInfo(userID);

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_USER_INFO);
			smallSendBuffer.WriteInt32(userID);
			smallSendBuffer.WriteString(resultObj == null ? "" : resultObj.Content);

			Send(Player, smallSendBuffer);
		}

		private void HandleGetMigrateCode(BufferStream Buffer, Player Player)
		{
			ISerializeObject resultObj = DatabaseLayer.GetMigrateCode(Player.ID);

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_MIGRATE_CODE);
			smallSendBuffer.WriteString(resultObj.Get<string>("code"));

			Send(Player, smallSendBuffer);
		}

		private void HandleApplyMigrateCode(BufferStream Buffer, Player Player)
		{
		}

		private void HandleSetPushID(BufferStream Buffer, Player Player)
		{
			string pushID = Buffer.ReadString();

			DatabaseLayer.SetPushID(Player.ID, pushID);
		}

		private void HandleGetInitialData(BufferStream Buffer, Player Player)
		{
			Send(Player, GameData.GetSplitTestGroupsInitialDataBuffer(Player.SplitTestGroupID));
		}

		private void HandleJoinToRoom(BufferStream Buffer, Player Player)
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

		private void HandleCancelJoinToRoom(BufferStream Buffer, Player Player)
		{
			for (int i = 0; i < waitings.Count; ++i)
			{
				if (waitings[i].Player != Player)
					continue;

				waitings.RemoveAt(i);

				break;
			}
		}

		private void HandleGetLeaderboardData(BufferStream Buffer, Player Player)
		{
			LeaderboardTypes type = (LeaderboardTypes)Buffer.ReadInt32();

			const int COUNT = 50;

			ISerializeArray arr = DatabaseLayer.GetLeaderboard(type, COUNT);

			if (arr != null)
			{
				ISerializeObject prevUserObj = arr.Get<ISerializeObject>(0);
				uint upperCoinRange = prevUserObj.Get<uint>("coin");

				for (uint i = arr.Count; i < COUNT; ++i)
				{
					ISerializeObject obj = prevUserObj.Clone();
					prevUserObj = obj;

					BotPlayerInfoMaker.Make(obj, upperCoinRange - 5, upperCoinRange, 1, LevelData.GetLevelCount(Player.SplitTestGroupID));

					arr.Add(obj);
				}
			}

			largeSendBuffer.Reset();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_LEADERBOARD);
			largeSendBuffer.WriteInt32((int)type);
			largeSendBuffer.WriteInt64(DatabaseLayer.GetLeaderboardStartTime(type));
			largeSendBuffer.WriteString(arr == null ? "[]" : arr.Content);

			Send(Player, largeSendBuffer);
		}

		private void HandlePurchaseFinished(BufferStream Buffer, Player Player)
		{
			Markets market = (Markets)Buffer.ReadInt32();
			int packID = Buffer.ReadInt32();
			string token = Buffer.ReadString();

			if (DatabaseLayer.GetPurchase(Player.ID, token) != null)
				return;

			bool isValid = false;
			int id = -1;
			string sku = "";
			uint price = 0;
			uint coin = 0;

			ISerializeObject packObj = ShopData.GetPack(Player.SplitTestGroupID, market, packID);
			if (packObj != null)
			{
				id = packObj.Get<int>("ID");
				sku = packObj.Get<string>("SKU");
				price = packObj.Get<uint>("Price");
				coin = packObj.Get<uint>("Coin");

				IPurchaseValidator validator = null;

				if (market == Markets.Windows)
				{
					//fill validator
				}
				else if (market == Markets.Cafebazaar)
				{
					//fill validator
				}

				isValid = validator.Validate(sku, token);
			}

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.PURCHASE_FINISHED);
			smallSendBuffer.WriteBool(isValid);

			Send(Player, smallSendBuffer);

			DatabaseLayer.AddPurchase(Player.ID, id, sku, price, coin, token, isValid);
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
			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			smallSendBuffer.WriteInt32(GameID);
			smallSendBuffer.WriteString(OtherPlayerInfo);
			Send(To, smallSendBuffer);
		}

		private Player FindPlayer(NetworkingPlayer Player)
		{
			if (playersMap.ContainsKey(Player))
				return playersMap[Player];

			return null;
		}

		private Room FindRoom(Player Player)
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