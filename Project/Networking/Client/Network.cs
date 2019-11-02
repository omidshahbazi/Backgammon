using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;

namespace Networking.Client
{
	public delegate void VersionCheckRespondEventHandler(VersionCheckResults Result);
	public delegate void AuthenticationRespondEventHandler(AuthenticateResults Result, int ID);
	public delegate void RestoreSessionRespondEventHandler(SessionRestoreResults Result);
	public delegate void UserInfoReadyEventHandler(int UserID, string Info);
	public delegate void MigrateCodeReadyEventHandler(string Code);
	public delegate void ApplyMigrateCodeRespondEventHandler(MigrateResults Result);
	public delegate void InitialDataReadyEventHandler(DataHashStatus Status, uint Hash, string Data);
	public delegate void StringsReadyEventHandler(DataHashStatus Status, uint Hash, string Data);
	public delegate void JoinedToRoomEventHandler(int GameID, string OtherPlayerInfo);
	public delegate void LeaderboardDataReadyEventHandler(LeaderboardTypes Type, long StartTime, string Data);
	public delegate void PurchaseFinishedEventHandler(bool IsValid);
	public delegate void GamesLogDataReadyEventHandler(string Data);
	public delegate void GameReplayDataReadyEventHandler(bool IsAvailable, string OtherPlayerInfo, byte[] ReplayData);
	public delegate void FriendshipDataReadyEventHandler(string Data);
	public delegate void DailyRewardReadyEventHandler(bool IsClaimed, int Dice1, int Dice2, RewardInfo Reward, long NextClaimTime);

	public delegate void GameDataReadyEventHandler(PlayerColors Color);
	public delegate void FramesDataReadyEventHandler(bool IsFullStep, byte[] Data);
	public delegate void TurnStartedEventHandler(PlayerColors Color, double StartTime, double EndTime);
	public delegate void BoardToBoardMovedEventHandler(int Hash, Identifier FromIdentifier, Identifier ToIdentifier);
	public delegate void BarToBoardMovedEventHandler(int Hash, PlayerColors Color, Identifier ToIdentifier);
	public delegate void BoardToBarMovedEventHandler(int Hash, PlayerColors Color, Identifier FromIdentifier);
	public delegate void BearedOffEventHandler(int Hash, Identifier FromIdentifier);
	public delegate void TurnFinishedEventHandler(int Hash, PlayerColors Color);
	public delegate void GameFinishedEventHandler(PlayerColors WinnerColor, GameFinishReasons Reason, RewardInfo Reward);
	public delegate void ChatReceivedEventHandler(int TextIndex);

	public class Network : Connection
	{
		private const int BUFFER_SIZE = 64;

		private BufferStream sendBuffer = null;
		private int userID = 0;

		public event VersionCheckRespondEventHandler OnVersionCheckRespond;
		public event AuthenticationRespondEventHandler OnAuthenticationRespond;
		public event RestoreSessionRespondEventHandler OnRestoreSessionRespond;
		public event UserInfoReadyEventHandler OnUserInfoReady;
		public event MigrateCodeReadyEventHandler OnMigrateCodeReady;
		public event ApplyMigrateCodeRespondEventHandler OnApplyMigrateCodeRespond;
		public event InitialDataReadyEventHandler OnInitialDataReady;
		public event StringsReadyEventHandler OnStringsReady;
		public event JoinedToRoomEventHandler OnJoinedToRoom;
		public event LeaderboardDataReadyEventHandler OnLeaderboardDataReady;
		public event PurchaseFinishedEventHandler OnPurchaseFinished;
		public event GamesLogDataReadyEventHandler OnGamesLogDataReady;
		public event GameReplayDataReadyEventHandler OnGameReplayDataReady;
		public event FriendshipDataReadyEventHandler OnFriendshipDataReady;
		public event DailyRewardReadyEventHandler OnDailyRewardReady;

		public event GameDataReadyEventHandler OnGameDataReady;
		public event FramesDataReadyEventHandler OnFramesDataReady;
		public event TurnStartedEventHandler OnTurnStarted;
		public event BoardToBoardMovedEventHandler OnBoardToBoardMoved;
		public event BoardToBarMovedEventHandler OnBoardToBarMoved;
		public event BarToBoardMovedEventHandler OnBarToBoardMoved;
		public event BearedOffEventHandler OnBearedOff;
		public event TurnFinishedEventHandler OnTurnFinished;
		public event GameFinishedEventHandler OnGameFinished;
		public event ChatReceivedEventHandler OnChatReceived;

		public Network()
		{
			sendBuffer = new BufferStream(new byte[BUFFER_SIZE]);

			OnBufferReceived += Network_OnBufferReceived;
		}

		public void VersionCheck(Markets Market, int Version)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.VERSION_CHECK);
			sendBuffer.WriteInt32((int)Market);
			sendBuffer.WriteInt32(Version);

			Send(sendBuffer);
		}

		public void Authenticate(string DeviceID, Markets Market, int Version)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			sendBuffer.WriteString(DeviceID);
			sendBuffer.WriteInt32((int)Market);
			sendBuffer.WriteInt32(Version);

			Send(sendBuffer);
		}

		public void RestoreSession()
		{
			if (userID == 0)
				return;

			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.RESTORE_SESSION);
			sendBuffer.WriteInt32(userID);

			Send(sendBuffer);
		}

		public void SetUserInfo(string Username, int Avatar)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.SET_USER_INFO);
			sendBuffer.WriteString(Username);
			sendBuffer.WriteInt32(Avatar);

			Send(sendBuffer);
		}

		public void GetUserInfo(int UserID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_USER_INFO);
			sendBuffer.WriteInt32(UserID);

			Send(sendBuffer);
		}

		public void GetMigrateCode()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_MIGRATE_CODE);

			Send(sendBuffer);
		}

		public void GetMigrateCode(string Code)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.APPLY_MIGRATE_CODE);
			sendBuffer.WriteString(Code);

			Send(sendBuffer);
		}

		public void SetPushID(string PushID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.SET_PUSH_ID);
			sendBuffer.WriteString(PushID);

			Send(sendBuffer);
		}

		public void GetInitialData(uint Hash)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);
			sendBuffer.WriteUInt32(Hash);

			Send(sendBuffer);
		}

		public void GetStrings(uint Hash)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_STRINGS);
			sendBuffer.WriteUInt32(Hash);

			Send(sendBuffer);
		}

		public void JoinToRoom(uint Bet, bool WithBot)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteUInt32(Bet);
			sendBuffer.WriteBool(WithBot);

			Send(sendBuffer);
		}

		public void CancelJoinToRoom()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.CANCEL_JOIN_TO_ROOM);

			Send(sendBuffer);
		}

		public void GetLeaderboard(LeaderboardTypes Type)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_LEADERBOARD);
			sendBuffer.WriteInt32((int)Type);

			Send(sendBuffer);
		}

		public void PurchaseFinished(Markets Market, int PackID, string Token)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.PURCHASE_FINISHED);
			sendBuffer.WriteInt32((int)Market);
			sendBuffer.WriteInt32(PackID);
			sendBuffer.WriteString(Token);

			Send(sendBuffer);
		}

		public void GetGamesLog()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Lobby.GET_GAMES_LOG);

			Send(sendBuffer);
		}

		public void GetGameReplayData(int GameID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_GAME_REPLAY_DATA);
			sendBuffer.WriteInt32(GameID);

			Send(sendBuffer);
		}

		public void AddFriendshipRequest(int OtherPlayerUserID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.ADD_FRIENDSHIP_REQUEST);
			sendBuffer.WriteInt32(OtherPlayerUserID);

			Send(sendBuffer);
		}

		public void RemoveFriendship(int OtherPlayerUserID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.REMOVE_FRIENDSHIP);
			sendBuffer.WriteInt32(OtherPlayerUserID);

			Send(sendBuffer);
		}

		public void AcceptFriendship(int OtherPlayerUserID)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.ACCEPT_FRIENDSHIP);
			sendBuffer.WriteInt32(OtherPlayerUserID);

			Send(sendBuffer);
		}

		public void GetFriendship()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_FRIENDSHIPS);

			Send(sendBuffer);
		}

		public void GetDailyReward()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.Get_DAILY_REWARD);

			Send(sendBuffer);
		}

		public void GetGameData()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			Send(sendBuffer);
		}

		public void GetFramesData()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_FRAMES_DATA);

			Send(sendBuffer);
		}

		public void BoardToBoardMove(int Hash, Identifier FromIdentifier, Identifier ToIdentifier)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BOARD_TO_BOARD_MOVE);
			sendBuffer.WriteInt32(Hash);
			sendBuffer.WriteInt32(FromIdentifier);
			sendBuffer.WriteInt32(ToIdentifier);

			Send(sendBuffer);
		}

		public void BardToBoardMove(int Hash, PlayerColors Color, Identifier ToIdentifier)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BAR_TO_BOARD_MOVE);
			sendBuffer.WriteInt32(Hash);
			sendBuffer.WriteInt32((int)Color);
			sendBuffer.WriteInt32(ToIdentifier);

			Send(sendBuffer);
		}

		public void BearOff(int Hash, Identifier FromIdentifier)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BEAR_OFF);
			sendBuffer.WriteInt32(Hash);
			sendBuffer.WriteInt32(FromIdentifier);

			Send(sendBuffer);
		}

		public void FinishTurn(int Hash, PlayerColors Color)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			sendBuffer.WriteInt32(Hash);
			sendBuffer.WriteInt32((int)Color);

			Send(sendBuffer);
		}

		public void Resign()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.RESIGN);

			Send(sendBuffer);
		}

		public void SendChat(int TextIndex)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.SEND_CHAT);
			sendBuffer.WriteInt32(TextIndex);

			Send(sendBuffer);
		}

		private void Network_OnBufferReceived(BufferStream Buffer)
		{
			byte category = Buffer.ReadByte();
			byte command = Buffer.ReadByte();

			if (category == Commands.Category.LOBBY)
			{
				if (command == Commands.Lobby.VERSION_CHECK)
				{
					VersionCheckResults result = (VersionCheckResults)Buffer.ReadInt32();

					if (OnVersionCheckRespond != null)
						OnVersionCheckRespond(result);
				}
				else if (command == Commands.Lobby.AUTHENTICATE)
				{
					AuthenticateResults result = (AuthenticateResults)Buffer.ReadInt32();
					userID = Buffer.ReadInt32();

					if (OnAuthenticationRespond != null)
						OnAuthenticationRespond(result, userID);
				}
				else if (command == Commands.Lobby.RESTORE_SESSION)
				{
					SessionRestoreResults result = (SessionRestoreResults)Buffer.ReadInt32();

					if (OnRestoreSessionRespond != null)
						OnRestoreSessionRespond(result);
				}
				else if (command == Commands.Lobby.GET_USER_INFO)
				{
					int userID = Buffer.ReadInt32();
					string info = Buffer.ReadString();

					if (OnUserInfoReady != null)
						OnUserInfoReady(userID, info);
				}
				else if (command == Commands.Lobby.GET_MIGRATE_CODE)
				{
					string code = Buffer.ReadString();

					if (OnMigrateCodeReady != null)
						OnMigrateCodeReady(code);
				}
				else if (command == Commands.Lobby.APPLY_MIGRATE_CODE)
				{
					MigrateResults result = (MigrateResults)Buffer.ReadInt32();

					if (OnApplyMigrateCodeRespond != null)
						OnApplyMigrateCodeRespond(result);
				}
				else if (command == Commands.Lobby.GET_INITIAL_DATA)
				{
					DataHashStatus status = (DataHashStatus)Buffer.ReadInt32();
					uint hash = 0;
					string data = "";

					if (status == DataHashStatus.UpdateAvailable)
					{
						hash = Buffer.ReadUInt32();
						data = Buffer.ReadString();
					}

					if (OnInitialDataReady != null)
						OnInitialDataReady(status, hash, data);
				}
				else if (command == Commands.Lobby.GET_STRINGS)
				{
					DataHashStatus status = (DataHashStatus)Buffer.ReadInt32();
					uint hash = 0;
					string data = "";

					if (status == DataHashStatus.UpdateAvailable)
					{
						hash = Buffer.ReadUInt32();
						data = Buffer.ReadString();
					}

					if (OnStringsReady != null)
						OnStringsReady(status, hash, data);
				}
				else if (command == Commands.Lobby.JOIN_TO_ROOM)
				{
					int gameID = Buffer.ReadInt32();
					string otherPlayerInfo = Buffer.ReadString();

					if (OnJoinedToRoom != null)
						OnJoinedToRoom(gameID, otherPlayerInfo);
				}
				else if (command == Commands.Lobby.GET_LEADERBOARD)
				{
					LeaderboardTypes type = (LeaderboardTypes)Buffer.ReadInt32();
					long startTime = Buffer.ReadInt64();
					string data = Buffer.ReadString();

					if (OnLeaderboardDataReady != null)
						OnLeaderboardDataReady(type, startTime, data);
				}
				else if (command == Commands.Lobby.PURCHASE_FINISHED)
				{
					bool isValid = Buffer.ReadBool();

					if (OnPurchaseFinished != null)
						OnPurchaseFinished(isValid);
				}
				else if (command == Commands.Lobby.GET_GAMES_LOG)
				{
					string data = Buffer.ReadString();

					if (OnGamesLogDataReady != null)
						OnGamesLogDataReady(data);
				}
				else if (command == Commands.Lobby.GET_GAME_REPLAY_DATA)
				{
					bool isAvailable = Buffer.ReadBool();
					string otherPlayerInfo = "";
					byte[] replayData = null;

					if (isAvailable)
					{
						otherPlayerInfo = Buffer.ReadString();

						int replayDataLen = Buffer.ReadInt32();
						replayData = new byte[replayDataLen];
						Buffer.ReadBytes(replayData, 0, replayDataLen);
					}

					if (OnGameReplayDataReady != null)
						OnGameReplayDataReady(isAvailable, otherPlayerInfo, replayData);
				}
				else if (command == Commands.Lobby.GET_FRIENDSHIPS)
				{
					string data = Buffer.ReadString();

					if (OnFriendshipDataReady != null)
						OnFriendshipDataReady(data);
				}
				else if (command == Commands.Lobby.Get_DAILY_REWARD)
				{
					bool isClaimed = Buffer.ReadBool();
					int dice1 = 0;
					int dice2 = 0;
					RewardInfo reward = null;
					long nextClaimTime = 0;

					if (isClaimed)
					{
						dice1 = Buffer.ReadInt32();
						dice2 = Buffer.ReadInt32();

						string rewardData = Buffer.ReadString();
						reward = new RewardInfo();
						reward.Deserialize(Creator.Create<ISerializeObject>(rewardData));
					}
					else
						nextClaimTime = Buffer.ReadInt64();

					if (OnDailyRewardReady != null)
						OnDailyRewardReady(isClaimed, dice1, dice2, reward, nextClaimTime);
				}
			}
			else if (category == Commands.Category.ROOM)
			{
				if (command == Commands.Room.GET_GAME_DATA)
				{
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();

					if (OnGameDataReady != null)
						OnGameDataReady(color);
				}
				else if (command == Commands.Room.GET_FRAMES_DATA)
				{
					bool isFullStep = Buffer.ReadBool();
					int dataLen = Buffer.ReadInt32();
					byte[] data = new byte[dataLen];
					Buffer.ReadBytes(data, 0, dataLen);

					if (OnFramesDataReady != null)
						OnFramesDataReady(isFullStep, data);
				}
				else if (command == Commands.Room.START_TURN)
				{
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();
					double startTime = Buffer.ReadFloat64();
					double endTime = Buffer.ReadFloat64();

					if (OnTurnStarted != null)
						OnTurnStarted(color, startTime, endTime);
				}
				else if (command == Commands.Room.BOARD_TO_BOARD_MOVE)
				{
					int hash = Buffer.ReadInt32();
					Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());
					Identifier toIdentifier = new Identifier(Buffer.ReadInt32());

					if (OnBoardToBoardMoved != null)
						OnBoardToBoardMoved(hash, fromIdentifier, toIdentifier);
				}
				else if (command == Commands.Room.BOARD_TO_BAR_MOVE)
				{
					int hash = Buffer.ReadInt32();
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();
					Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());

					if (OnBoardToBarMoved != null)
						OnBoardToBarMoved(hash, color, fromIdentifier);
				}
				else if (command == Commands.Room.BAR_TO_BOARD_MOVE)
				{
					int hash = Buffer.ReadInt32();
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();
					Identifier toIdentifier = new Identifier(Buffer.ReadInt32());

					if (OnBarToBoardMoved != null)
						OnBarToBoardMoved(hash, color, toIdentifier);
				}
				else if (command == Commands.Room.BEAR_OFF)
				{
					int hash = Buffer.ReadInt32();
					Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());

					if (OnBearedOff != null)
						OnBearedOff(hash, fromIdentifier);
				}
				else if (command == Commands.Room.FINISH_TURN)
				{
					int hash = Buffer.ReadInt32();
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();

					if (OnTurnFinished != null)
						OnTurnFinished(hash, color);
				}
				else if (command == Commands.Room.FINISH_GAME)
				{
					PlayerColors winnerColor = (PlayerColors)Buffer.ReadInt32();
					GameFinishReasons reason = (GameFinishReasons)Buffer.ReadInt32();
					string rewardData = Buffer.ReadString();

					RewardInfo reward = new RewardInfo();
					reward.Deserialize(Creator.Create<ISerializeObject>(rewardData));

					if (OnGameFinished != null)
						OnGameFinished(winnerColor, reason, reward);
				}
				else if (command == Commands.Room.SEND_CHAT)
				{
					int textIndex = Buffer.ReadInt32();

					if (OnChatReceived != null)
						OnChatReceived(textIndex);
				}
			}
		}
	}
}
