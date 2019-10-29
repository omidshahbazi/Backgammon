using BeardedManStudios.Forge.Networking;
using Networking.Common;
using System.Collections.Generic;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;
using Networking.Server.Data;
using GameFramework.CommonAPIManaged.PurchaseValidation;

namespace Networking.Server
{
	class Lobby : LogicObjects
	{
		private struct WaitingInfo
		{
			public Player Player;
			public uint TableBet;
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

				if (room.PLayerCount == 0)
					RemoveRoom(room);
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
				else if (command == Commands.Lobby.GET_STRINGS)
				{
					HandleGetStrings(Buffer, player);
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
				else if (command == Commands.Lobby.GET_GAMES_LOG)
				{
					HandleGetGamesLogData(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_GAME_REPLAY_DATA)
				{
					HandleGetGameReplayData(Buffer, player);
				}
				else if (command == Commands.Lobby.ADD_FRIENDSHIP_REQUEST)
				{
					HandleAddFriendshipRequest(Buffer, player);
				}
				else if (command == Commands.Lobby.REMOVE_FRIENDSHIP)
				{
					HandleRemoveFriendship(Buffer, player);
				}
				else if (command == Commands.Lobby.ACCEPT_FRIENDSHIP)
				{
					HandleAcceptFriendship(Buffer, player);
				}
				else if (command == Commands.Lobby.GET_FRIENDSHIPS)
				{
					HandleGetFriendship(Buffer, player);
				}
				else if (command == Commands.Lobby.Get_DAILY_REWARD)
				{
					HandleGetDailyReward(Buffer, player);
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

		public void RemoveRoom(Room Room)
		{
			rooms.Remove(Room);

			Log("Room " + Room + " removed");
		}

		public ISerializeObject GetStatistics()
		{
			ISerializeObject statObj = Creator.Create<ISerializeObject>();

			statObj.Set("RoomCount", rooms.Count);
			statObj.Set("CCU", playersMap.Count);
			statObj.Set("WaitingCount", waitings.Count);

			return statObj;
		}

		private void HandleVersionCheck(BufferStream Buffer, NetworkingPlayer Player)
		{
			Markets market = (Markets)Buffer.ReadInt32();
			int clientVersion = Buffer.ReadInt32();

			VersionCheckResults result = VersionCheckResults.OK;

			ISerializeObject versionObj = GameData.VersionObject;
			if (versionObj == null)
			{
				LogError("Version couldn't find in data");
				return;
			}

			if (versionObj.Get<bool>("IsUnderMaintenance"))
				result = VersionCheckResults.UnderMaintenance;
			else
			{
				versionObj = versionObj.Get<ISerializeObject>(market.ToString());
				if (versionObj == null)
				{
					LogError("Version::" + market + " couldn't find in data");
					return;
				}

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

			if (result == VersionCheckResults.NewerVersionAvailable || result == VersionCheckResults.UpdateNeeded)
				smallSendBuffer.WriteString(versionObj.Get<string>("Link"));

			Send(Player, smallSendBuffer);
		}

		private void HandleAuthenticate(BufferStream Buffer, NetworkingPlayer Player)
		{
			string deviceID = Buffer.ReadString();
			Markets market = (Markets)Buffer.ReadInt32();
			int version = Buffer.ReadInt32();

			ISerializeObject resultObj = DatabaseLayer.Authenticate(deviceID, market, version, Player.Ip, Player.RoundTripLatency);
			AuthenticateResults result = resultObj.Get<AuthenticateResults>("result");

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			smallSendBuffer.WriteInt32((int)result);

			int id = resultObj.Get<int>("id");
			smallSendBuffer.WriteInt32(id);

			if (result == AuthenticateResults.Passed)
				playersMap[Player] = new Player(Player, id, resultObj.Get<int>("split_test_group_id"), version);

			Send(Player, smallSendBuffer);
		}

		private void HandleSetUserInfo(BufferStream Buffer, Player Player)
		{
			string username = Buffer.ReadString();
			int avatar = Buffer.ReadInt32();

			DatabaseLayer.SetUserInfo(Player.ID, username, avatar);
		}

		private void HandleGetUserInfo(BufferStream Buffer, Player Player)
		{
			int userID = Buffer.ReadInt32();

			ISerializeObject resultObj = DatabaseLayer.GetAdvancedUserInfo(userID);

			largeSendBuffer.Reset();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_USER_INFO);
			largeSendBuffer.WriteInt32(userID);
			largeSendBuffer.WriteString(resultObj == null ? "" : resultObj.Content);

			Send(Player, largeSendBuffer);
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
			string code = Buffer.ReadString();

			MigrateResults result = DatabaseLayer.ApplyMigrateCode(Player.ID, code);

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_MIGRATE_CODE);
			smallSendBuffer.WriteInt32((int)result);

			Send(Player, smallSendBuffer);
		}

		private void HandleSetPushID(BufferStream Buffer, Player Player)
		{
			string pushID = Buffer.ReadString();

			DatabaseLayer.SetPushID(Player.ID, pushID);
		}

		private void HandleGetInitialData(BufferStream Buffer, Player Player)
		{
			uint hash = Buffer.ReadUInt32();

			if (GameData.GetSplitTestGroupInitialDataHash(Player.SplitTestGroupID) == hash)
			{
				smallSendBuffer.Reset();
				smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
				smallSendBuffer.WriteInt32((int)DataHashStatus.OK);

				Send(Player, smallSendBuffer);
			}

			Send(Player, GameData.GetSplitTestGroupInitialDataBuffer(Player.SplitTestGroupID));
		}

		private void HandleGetStrings(BufferStream Buffer, Player Player)
		{
			uint hash = Buffer.ReadUInt32();

			if (GameData.GetSplitTestGroupStringsHash(Player.SplitTestGroupID) == hash)
			{
				smallSendBuffer.Reset();
				smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_STRINGS);
				smallSendBuffer.WriteInt32((int)DataHashStatus.OK);

				Send(Player, smallSendBuffer);
			}

			Send(Player, GameData.GetSplitTestGroupStringsBuffer(Player.SplitTestGroupID));
		}

		private void HandleJoinToRoom(BufferStream Buffer, Player Player)
		{
			for (int i = 0; i < waitings.Count; ++i)
				if (waitings[i].Player == Player)
					return;

			uint bet = Buffer.ReadUInt32();

			if (!DatabaseLayer.HasEnoughResource(Player.ID, new CostInfo(bet)))
				return;

			bool withBot = Buffer.ReadBool();

			if (withBot)
			{
				CreateOneByBotRoom(Player, bet);

				return;
			}

			for (int i = 0; i < waitings.Count; ++i)
			{
				WaitingInfo info = waitings[i];

				if (info.Player == Player || info.Player.Version != Player.Version)
					continue;

				if (info.TableBet != bet)
					continue;

				CreateOneByOneRoom(info.Player, Player, bet);

				waitings.RemoveAt(i);

				return;
			}

			waitings.Add(new WaitingInfo { Player = Player, TableBet = bet });
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
					validator = new CafeBazaarPurchaseValidator(Constants.PACKAGE_NAME, "", "", "");
				}
				else if (market == Markets.Myket)
				{
					validator = new MyketPurchaseValidator(Constants.PACKAGE_NAME, "8c491ebd-9a2a-4140-a203-b49d66f2abf6");
				}

				if (validator != null)
					validator.Validate(price, sku, token, (Result, Error) =>
					{
						isValid = Result;
					});
			}

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.PURCHASE_FINISHED);
			smallSendBuffer.WriteBool(isValid);

			Send(Player, smallSendBuffer);

			DatabaseLayer.AddPurchase(Player.ID, id, sku, price, coin, token, isValid);
		}

		private void HandleGetGamesLogData(BufferStream Buffer, Player Player)
		{
			const int COUNT = 20;

			ISerializeArray arr = DatabaseLayer.GetGamesLogData(Player.ID, Player.Version, COUNT);

			largeSendBuffer.Reset();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAMES_LOG);
			largeSendBuffer.WriteString(arr == null ? "[]" : arr.Content);

			Send(Player, largeSendBuffer);
		}

		private void HandleGetGameReplayData(BufferStream Buffer, Player Player)
		{
			int gameID = Buffer.ReadInt32();

			byte[] replayData = DatabaseLayer.GetGameReplayData(gameID, Player.Version);
			bool isAvailable = replayData != null;

			largeSendBuffer.Reset();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAME_REPLAY_DATA);
			largeSendBuffer.WriteBool(isAvailable);

			if (isAvailable)
			{
				ISerializeObject gameDataObj = DatabaseLayer.GetGameData(gameID);

				int opponentID = gameDataObj.Get<int>("opponent_user_id");

				if (opponentID == -1)
					largeSendBuffer.WriteString(gameDataObj.Get<string>("bot_user_info"));
				else
					largeSendBuffer.WriteString(DatabaseLayer.GetBasicUserInfo(opponentID).Content);

				largeSendBuffer.WriteInt32(replayData.Length);
				largeSendBuffer.WriteBytes(replayData);
			}

			Send(Player, largeSendBuffer);
		}

		private void HandleAddFriendshipRequest(BufferStream Buffer, Player Player)
		{
			int otherUserID = Buffer.ReadInt32();

			DatabaseLayer.AddFriendshipRequest(Player.ID, otherUserID);
		}

		private void HandleRemoveFriendship(BufferStream Buffer, Player Player)
		{
			int otherUserID = Buffer.ReadInt32();

			DatabaseLayer.RemoveFrinedship(Player.ID, otherUserID);
		}

		private void HandleAcceptFriendship(BufferStream Buffer, Player Player)
		{
			int otherUserID = Buffer.ReadInt32();

			DatabaseLayer.AcceptFriendship(Player.ID, otherUserID);
		}

		private void HandleGetFriendship(BufferStream Buffer, Player Player)
		{
			ISerializeArray arr = DatabaseLayer.GetFriendships(Player.ID);

			if (arr != null)
			{
				for (uint i = 0; i < arr.Count; ++i)
				{
					ISerializeObject obj = arr.Get<ISerializeObject>(i);

					bool isOnline = (FindPlayer(obj.Get<int>("friend_user_id")) != null);

					obj.Set("is_online", isOnline);
				}
			}

			largeSendBuffer.Reset();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAMES_LOG);
			largeSendBuffer.WriteString(arr == null ? "[]" : arr.Content);

			Send(Player, largeSendBuffer);
		}

		private void HandleGetDailyReward(BufferStream Buffer, Player Player)
		{
			ISerializeObject result = DatabaseLayer.CanClaimDailyReward(Player.ID);
			if (result == null)
				return;

			bool canClaim = result.Get<bool>("can_claim");

			smallSendBuffer.Reset();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.Get_DAILY_REWARD);
			smallSendBuffer.WriteBool(canClaim);

			if (canClaim)
			{
				RewardInfo reward = GeneralData.GetInitialResource(Player.SplitTestGroupID);
				if (reward == null)
					return;

				int dice1 = Configs.Random.Next(1, 6);
				int dice2 = Configs.Random.Next(1, 6);
				float ratio = (dice1 + dice2) / 12.0F;

				reward.SetCoin((uint)(reward.Coin * ratio));
				DatabaseLayer.AddReward(Player.ID, reward, Places.DailyReward);

				smallSendBuffer.WriteInt32(dice1);
				smallSendBuffer.WriteInt32(dice2);

				smallSendBuffer.WriteString(reward.Serialize().Content);
			}
			else
				smallSendBuffer.WriteInt64(result.Get<long>("next_claim_time"));

			Send(Player, smallSendBuffer);
		}

		private void CreateOneByOneRoom(Player Player1, Player Player2, uint TableEnteracnce)
		{
			CostInfo cost = new CostInfo(TableEnteracnce);
			DatabaseLayer.GetCost(Player1.ID, cost, Places.JoinToRoom);
			DatabaseLayer.GetCost(Player2.ID, cost, Places.JoinToRoom);

			OneByOneRoom room = new OneByOneRoom(Application, TableEnteracnce, TableData.GetTurnTime(Player1.SplitTestGroupID, TableEnteracnce));

			room.AddPlayer(Player1);
			room.AddPlayer(Player2);

			rooms.Add(room);

			room.Initialize();

			SendJoinedToRoom(Player1, DatabaseLayer.GetBasicUserInfo(Player2.ID).Content, room.GameID);
			SendJoinedToRoom(Player2, DatabaseLayer.GetBasicUserInfo(Player1.ID).Content, room.GameID);
		}

		private void CreateOneByBotRoom(Player Player, uint TableEnteracnce)
		{
			DatabaseLayer.GetCost(Player.ID, new CostInfo(TableEnteracnce), Places.JoinToRoom);

			OneByBotRoom room = new OneByBotRoom(Application, TableEnteracnce, TableData.GetTurnTime(Player.SplitTestGroupID, TableEnteracnce));

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

		private Player FindPlayer(int UserID)
		{
			var it = playersMap.GetEnumerator();

			while (it.MoveNext())
			{
				if (it.Current.Value.ID == UserID)
					return it.Current.Value;
			}

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