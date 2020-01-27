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
		private struct RoomWaitingInfo
		{
			public Player Player;
			public int TableID;
		}

		private class RoomWaitingInfoList : List<RoomWaitingInfo>
		{ }

		private struct FriendlyWaitingInfo
		{
			public Player Player;
			public Player FriendPlayer;
		}

		private class FriendlyWaitingInfoList : List<FriendlyWaitingInfo>
		{ }

		private BufferStream smallSendBuffer = null;
		private BufferStream largeSendBuffer = null;
		private RoomList rooms = null;
		private NetworPlayerMap playersMap = null;
		private RoomWaitingInfoList roomWaitings = null;
		private FriendlyWaitingInfoList friendlyWaitings = null;

		public Lobby(Application Application) :
			base(Application)
		{
			smallSendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize]);
			largeSendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize * 1000]);

			rooms = new RoomList();
			playersMap = new NetworPlayerMap();
			roomWaitings = new RoomWaitingInfoList();
			friendlyWaitings = new FriendlyWaitingInfoList();

			// Do not uncomment these lines
			//CafeBazaarPurchaseValidator.OpenGetCodeURL("http://royalgammon.com", "BBNoKz4YtVpL9hOYYwpDIawnzUDK5qS4geocgLR6");
			//string tok = CafeBazaarPurchaseValidator.GetRefreshToken("f83acV1wtmCepdFhY0rcQpjKXdzCx0", "BBNoKz4YtVpL9hOYYwpDIawnzUDK5qS4geocgLR6", "Uy7W5PL2K5QHuEYpSyHQBzcf5rHcpcdrBiWBtsDColf762BVH3iOT3dZ6jFT", "http://royalgammon.com");
		}

		public void HandlePlayerDisconnection(NetworkingPlayer Player)
		{
			Player player = FindPlayer(Player);
			if (player == null)
				return;

			player.IsConnected = false;

			ScheduleWokerFor(GeneralData.GetWaitForRestoreSession(player.SplitTestGroupID), () =>
			{
				if (player.IsConnected)
					return;

				Room room = FindRoom(player);
				if (room != null)
				{
					room.HandlePlayerDisconnection(player);

					if (room.PLayerCount == 0)
						RemoveRoom(room);
				}

				CancelRoomWaiting(player);
				CancelFriendlyWaiting(player);

				playersMap.Remove(player.NetworkingPlayer);
			});

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
			else if (command == Commands.Lobby.RESTORE_SESSION)
			{
				HandleRestoreSession(Buffer, Player);
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
				else if (command == Commands.Lobby.SWITCH_DICE)
				{
					HandleSwitchDice(Buffer, player);
				}
				else if (command == Commands.Lobby.PLAY_WITH_FRIEND)
				{
					HandlePlayWithFriend(Buffer, player);
				}
				else if (command == Commands.Lobby.RESPONSE_FRIEND_PLAY)
				{
					HandleResponseFriendPlay(Buffer, player);
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

		public void GetStatistics(ISerializeObject Object)
		{
			Object.Set("CCU", playersMap.Count);
			Object.Set("WaitingCount", roomWaitings.Count);

			ISerializeArray roomsArr = Object.AddArray("Rooms");
			for (int i = 0; i < rooms.Count; ++i)
			{
				Room room = rooms[i];

				room.GetStatistics(roomsArr.AddObject());
			}
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

			smallSendBuffer.ResetWrite();
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

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			smallSendBuffer.WriteInt32((int)result);

			int id = resultObj.Get<int>("id");
			smallSendBuffer.WriteInt32(id);

			if (result == AuthenticateResults.Passed)
				playersMap[Player] = new Player(Player, id, resultObj.Get<int>("split_test_group_id"), version);

			Send(Player, smallSendBuffer);
		}

		private void HandleRestoreSession(BufferStream Buffer, NetworkingPlayer Player)
		{
			int userID = Buffer.ReadInt32();

			Player player = FindPlayer(userID);

			if (player == null)
				return;

			NetworkingPlayer netPlayer = FindNetworkingPlayer(userID);
			playersMap.Remove(netPlayer);

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.RESTORE_SESSION);

			bool isExists = (player != null);

			if (isExists)
			{
				player.NetworkingPlayer = Player;
				player.IsConnected = true;

				playersMap[Player] = player;
			}

			smallSendBuffer.WriteInt32((int)(isExists ? SessionRestoreResults.Done : SessionRestoreResults.Failed));

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

			largeSendBuffer.ResetWrite();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_USER_INFO);
			largeSendBuffer.WriteInt32(userID);
			largeSendBuffer.WriteString(resultObj == null ? "" : resultObj.Content);

			Send(Player, largeSendBuffer);
		}

		private void HandleGetMigrateCode(BufferStream Buffer, Player Player)
		{
			ISerializeObject resultObj = DatabaseLayer.GetMigrateCode(Player.ID);

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_MIGRATE_CODE);
			smallSendBuffer.WriteString(resultObj.Get<string>("code"));

			Send(Player, smallSendBuffer);
		}

		private void HandleApplyMigrateCode(BufferStream Buffer, Player Player)
		{
			string code = Buffer.ReadString();

			MigrateResults result = DatabaseLayer.ApplyMigrateCode(Player.ID, code);

			smallSendBuffer.ResetWrite();
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
				smallSendBuffer.ResetWrite();
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
				smallSendBuffer.ResetWrite();
				smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_STRINGS);
				smallSendBuffer.WriteInt32((int)DataHashStatus.OK);

				Send(Player, smallSendBuffer);
			}

			Send(Player, GameData.GetSplitTestGroupStringsBuffer(Player.SplitTestGroupID));
		}

		private void HandleJoinToRoom(BufferStream Buffer, Player Player)
		{
			if (IsInRoomWaiting(Player))
				return;

			if (IsInFriendlyWaiting(Player))
				return;

			if (FindRoom(Player) != null)
				return;

			int tableID = Buffer.ReadInt32();

			uint bet = TableData.GetBet(Player.SplitTestGroupID, tableID);
			if (!DatabaseLayer.HasEnoughResource(Player.ID, new CostInfo(bet)))
				return;

			ISerializeObject userObj = DatabaseLayer.GetBasicUserInfo(Player.ID);
			if (userObj.Get<uint>("level") < TableData.GetUnlockLevel(Player.SplitTestGroupID, tableID))
				return;

			bool withBot = Buffer.ReadBool();

			if (withBot)
			{
				CreateOneByBotRoom(Player, tableID);

				return;
			}

			for (int i = 0; i < roomWaitings.Count; ++i)
			{
				RoomWaitingInfo info = roomWaitings[i];

				if (info.Player == Player || info.Player.Version != Player.Version)
					continue;

				if (info.TableID != tableID)
					continue;

				CreateOneByOneRoom(info.Player, Player, tableID);

				roomWaitings.RemoveAt(i);

				return;
			}

			roomWaitings.Add(new RoomWaitingInfo { Player = Player, TableID = tableID });
		}

		private void HandleCancelJoinToRoom(BufferStream Buffer, Player Player)
		{
			CancelRoomWaiting(Player);
			CancelFriendlyWaiting(Player);
		}

		private void HandleGetLeaderboardData(BufferStream Buffer, Player Player)
		{
			LeaderboardTypes type = (LeaderboardTypes)Buffer.ReadInt32();

			int count = (int)GeneralData.GetLeaderboardMaxCount(Player.SplitTestGroupID);

			int myCoin;
			ISerializeArray arr = DatabaseLayer.GetLeaderboard(Player.ID, type, count, out myCoin);

			if (arr != null)
			{
				ISerializeObject prevUserObj = arr.Get<ISerializeObject>(arr.Count - 1);

				for (uint i = arr.Count; i < count; ++i)
				{
					ISerializeObject obj = prevUserObj.Clone();
					prevUserObj = obj;

					ISerializeObject userInfoObj = obj.Get<ISerializeObject>("user_info");
					uint upperCoinRange = userInfoObj.Get<uint>("coin");

					BotPlayerInfoMaker.Make(userInfoObj, upperCoinRange - 5, upperCoinRange, 1, LevelData.GetLevelCount(Player.SplitTestGroupID));

					obj.Set("coin", prevUserObj.Get<int>("coin") - 10);

					arr.Add(obj);
				}
			}

			largeSendBuffer.ResetWrite();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_LEADERBOARD);
			largeSendBuffer.WriteInt32((int)type);
			largeSendBuffer.WriteInt64(DatabaseLayer.GetLeaderboardStartTime(type));
			largeSendBuffer.WriteString(arr == null ? "[]" : arr.Content);
			largeSendBuffer.WriteInt32(myCoin);

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
			string sku = "";
			uint price = 0;
			RewardInfo pack = null;

			ISerializeObject packObj = ShopData.GetPack(Player.SplitTestGroupID, market, packID);
			if (packObj != null)
			{
				sku = packObj.Get<string>("SKU");
				price = packObj.Get<uint>("Price");

				pack = new RewardInfo();
				pack.Deserialize(packObj.Get<ISerializeObject>("Pack"));

				IPurchaseValidator validator = null;

				if (market == Markets.Windows)
				{
					//fill validator
				}
				else if (market == Markets.Cafebazaar)
				{
					validator = new CafeBazaarPurchaseValidator(Constants.PACKAGE_NAME, "BBNoKz4YtVpL9hOYYwpDIawnzUDK5qS4geocgLR6", "Uy7W5PL2K5QHuEYpSyHQBzcf5rHcpcdrBiWBtsDColf762BVH3iOT3dZ6jFT", "QMNIbgikXFyz5kZccUxIRKzsPXr7mj");
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

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.PURCHASE_FINISHED);
			smallSendBuffer.WriteBool(isValid);

			Send(Player, smallSendBuffer);

			DatabaseLayer.AddPurchase(Player.ID, (int)market, packID, sku, price, pack, token, isValid);
		}

		private void HandleGetGamesLogData(BufferStream Buffer, Player Player)
		{
			const int COUNT = 20;

			ISerializeArray arr = DatabaseLayer.GetGamesLogData(Player.ID, Player.Version, COUNT);

			for (uint i = 0; i < arr.Count; ++i)
			{
				ISerializeObject obj = arr.Get<ISerializeObject>(i);

				string botUserInfo = obj.Get<string>("bot_user_info");
				obj.Remove("bot_user_info");

				if (string.IsNullOrEmpty(botUserInfo))
					continue;

				ISerializeObject botUserInfoObj = Creator.Create<ISerializeObject>(botUserInfo);

				obj.Set("bot_user_info", botUserInfoObj);
			}

			largeSendBuffer.ResetWrite();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAMES_LOG);
			largeSendBuffer.WriteString(arr == null ? "[]" : arr.Content);

			Send(Player, largeSendBuffer);
		}

		private void HandleGetGameReplayData(BufferStream Buffer, Player Player)
		{
			int gameID = Buffer.ReadInt32();

			byte[] replayData = DatabaseLayer.GetGameReplayData(gameID, Player.Version);
			bool isAvailable = replayData != null;

			largeSendBuffer.ResetWrite();
			largeSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAME_REPLAY_DATA);
			largeSendBuffer.WriteBool(isAvailable);

			if (isAvailable)
			{
				ISerializeObject gameDataObj = DatabaseLayer.GetGameData(Player.ID, gameID);

				int opponentID = gameDataObj.Get<int>("opponent_user_id");

				if (opponentID == -1)
					largeSendBuffer.WriteString(gameDataObj.Get<string>("bot_user_info"));
				else
					largeSendBuffer.WriteString(DatabaseLayer.GetBasicUserInfo(opponentID).Content);

				largeSendBuffer.WriteUInt32((uint)replayData.Length);
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

			largeSendBuffer.ResetWrite();
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

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.Get_DAILY_REWARD);
			smallSendBuffer.WriteBool(canClaim);

			if (canClaim)
			{
				RewardInfo reward = DailyRewardData.GetTotalReward(Player.SplitTestGroupID);
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

		private void HandleSwitchDice(BufferStream Buffer, Player Player)
		{
			int diceID = Buffer.ReadInt32();

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.SWITCH_DICE);

			smallSendBuffer.WriteBool(DatabaseLayer.SwitchDice(Player.ID, diceID));

			Send(Player, smallSendBuffer);
		}

		private void HandlePlayWithFriend(BufferStream Buffer, Player Player)
		{
			int friendUserID = Buffer.ReadInt32();

			if (IsInRoomWaiting(Player))
				return;

			if (IsInFriendlyWaiting(Player))
				return;

			if (FindRoom(Player) != null)
				return;

			Player friendPlayer = FindPlayer(friendUserID);

			if (IsInRoomWaiting(friendPlayer))
				return;

			if (IsInFriendlyWaiting(friendPlayer))
				return;

			if (FindRoom(friendPlayer) != null)
				return;

			if (Player.Version != friendPlayer.Version)
				return;

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.PLAY_WITH_FRIEND);

			smallSendBuffer.WriteInt32(Player.ID);

			Send(friendPlayer, smallSendBuffer);

			friendlyWaitings.Add(new FriendlyWaitingInfo { Player = Player, FriendPlayer = friendPlayer });
		}

		private void HandleResponseFriendPlay(BufferStream Buffer, Player Player)
		{
			bool accepted = Buffer.ReadBool();

			if (friendlyWaitings.Count == 0)
				return;

			FriendlyWaitingInfo info = new FriendlyWaitingInfo();
			for (int i = 0; i < friendlyWaitings.Count; ++i)
			{
				FriendlyWaitingInfo winfo = friendlyWaitings[i];

				if (winfo.FriendPlayer != Player)
					continue;

				friendlyWaitings.RemoveAt(i);

				info = winfo;

				break;
			}

			if (info.Player == null)
				return;

			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.RESPONSE_FRIEND_PLAY);

			if (accepted)
			{
				CreateFriendlyRoom(info.Player, info.FriendPlayer);

				smallSendBuffer.WriteBool(true);
			}
			else
			{
				smallSendBuffer.WriteBool(false);
			}

			Send(info.Player, smallSendBuffer);
		}

		private void CreateOneByOneRoom(Player Player1, Player Player2, int TableID)
		{
			DatabaseLayer.GetCost(Player1.ID, new CostInfo(TableData.GetBet(Player1.SplitTestGroupID, TableID)), Places.JoinToRoom);
			DatabaseLayer.GetCost(Player2.ID, new CostInfo(TableData.GetBet(Player2.SplitTestGroupID, TableID)), Places.JoinToRoom);

			OneByOneRoom room = new OneByOneRoom(Application, TableID, TableData.GetTurnTime(Player1.SplitTestGroupID, TableID));

			room.AddPlayer(Player1);
			room.AddPlayer(Player2);

			rooms.Add(room);

			room.Initialize();

			SendJoinedToRoom(Player1, DatabaseLayer.GetBasicUserInfo(Player2.ID).Content, room.Seed);
			SendJoinedToRoom(Player2, DatabaseLayer.GetBasicUserInfo(Player1.ID).Content, room.Seed);
		}

		private void CreateOneByBotRoom(Player Player, int TableID)
		{
			DatabaseLayer.GetCost(Player.ID, new CostInfo(TableData.GetBet(Player.SplitTestGroupID, TableID)), Places.JoinToRoom);

			OneByBotRoom room = new OneByBotRoom(Application, TableID, TableData.GetTurnTime(Player.SplitTestGroupID, TableID));

			room.AddPlayer(Player);

			room.Initialize();

			rooms.Add(room);

			SendJoinedToRoom(Player, room.BotPlayerInfo, room.Seed);
		}

		private void CreateFriendlyRoom(Player Player1, Player Player2)
		{
			ISerializeArray tablesArr = TableData.GetTablesArray(Player1.SplitTestGroupID);
			int tableID = tablesArr.Get<ISerializeObject>(0).Get<int>("ID");

			FriendlyRoom room = new FriendlyRoom(Application, tableID, TableData.GetTurnTime(Player1.SplitTestGroupID, tableID));

			room.AddPlayer(Player1);
			room.AddPlayer(Player2);

			rooms.Add(room);

			room.Initialize();

			SendJoinedToRoom(Player1, DatabaseLayer.GetBasicUserInfo(Player2.ID).Content, room.Seed);
			SendJoinedToRoom(Player2, DatabaseLayer.GetBasicUserInfo(Player1.ID).Content, room.Seed);
		}

		private void SendJoinedToRoom(Player To, string OtherPlayerInfo, int GameID)
		{
			smallSendBuffer.ResetWrite();
			smallSendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			smallSendBuffer.WriteInt32(GameID);
			smallSendBuffer.WriteString(OtherPlayerInfo);
			Send(To, smallSendBuffer);
		}

		private void CancelRoomWaiting(Player Player)
		{
			for (int i = 0; i < roomWaitings.Count; ++i)
			{
				if (roomWaitings[i].Player != Player)
					continue;

				roomWaitings.RemoveAt(i);

				break;
			}
		}

		private void CancelFriendlyWaiting(Player Player)
		{
			for (int i = 0; i < friendlyWaitings.Count; ++i)
			{
				if (friendlyWaitings[i].Player != Player && friendlyWaitings[i].FriendPlayer != Player)
					continue;

				friendlyWaitings.RemoveAt(i);

				break;
			}
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

		private NetworkingPlayer FindNetworkingPlayer(int UserID)
		{
			var it = playersMap.GetEnumerator();

			while (it.MoveNext())
			{
				if (it.Current.Value.ID == UserID)
					return it.Current.Key;
			}

			return null;
		}

		private bool IsInRoomWaiting(Player Player)
		{
			for (int i = 0; i < roomWaitings.Count; ++i)
			{
				RoomWaitingInfo waitingInfo = roomWaitings[i];

				if (waitingInfo.Player == Player)
					return true;
			}

			return false;
		}

		private bool IsInFriendlyWaiting(Player Player)
		{
			for (int i = 0; i < friendlyWaitings.Count; ++i)
			{
				FriendlyWaitingInfo waitingInfo = friendlyWaitings[i];

				if (waitingInfo.Player == Player)
					return true;
			}

			return false;
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