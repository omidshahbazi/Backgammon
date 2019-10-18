using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;

namespace Networking.Client
{
	public delegate void VersionCheckRespondEventHandler(VersionCheckResults Result);
	public delegate void AuthenticationRespondEventHandler(AuthenticateResults Result, int ID, string Username);
	public delegate void UserInfoReadyEventHandler(int UserID, string Data);
	public delegate void MigrateCodeReadyEventHandler(string Code);
	public delegate void ApplyMigrateCodeRespondEventHandler(MigrateResults Result);
	public delegate void JoinedToRoomEventHandler(int GameID, string OtherPlayerInfo);
	public delegate void LeaderboardDataReadyEventHandler(LeaderboardTypes Type, long StartTime, string Data);
	public delegate void InitialDataReadyEventHandler(string Data);
	public delegate void GameDataReadyEventHandler(PlayerColors Color);
	public delegate void TurnStartedEventHandler(PlayerColors Color, double StartTime, double EndTime);
	public delegate void BoardToBoardMovedEventHandler(int Hash, Identifier FromIdentifier, Identifier ToIdentifier);
	public delegate void BarToBoardMovedEventHandler(int Hash, PlayerColors Color, Identifier ToIdentifier);
	public delegate void BoardToBarMovedEventHandler(int Hash, PlayerColors Color, Identifier FromIdentifier);
	public delegate void BearedOffEventHandler(int Hash, Identifier FromIdentifier);
	public delegate void TurnFinishedEventHandler(int Hash, PlayerColors Color);
	public delegate void GameFinishedEventHandler(PlayerColors WinnerColor, GameFinishReasons Reason, RewardInfo Reward);
	public delegate void ChatReceivedEventHandler(int TextIndex);
	public delegate void PurchaseFinishedEventHandler(bool IsValid);

	public class Network : Connection
	{
		private const int BUFFER_SIZE = 32;

		private BufferStream sendBuffer = null;

		public event VersionCheckRespondEventHandler OnVersionCheckRespond;
		public event AuthenticationRespondEventHandler OnAuthenticationRespond;
		public event UserInfoReadyEventHandler OnUserInfoReady;
		public event MigrateCodeReadyEventHandler OnMigrateCodeReady;
		public event ApplyMigrateCodeRespondEventHandler OnApplyMigrateCodeRespond;
		public event JoinedToRoomEventHandler OnJoinedToRoom;
		public event LeaderboardDataReadyEventHandler OnLeaderboardDataReady;
		public event InitialDataReadyEventHandler OnInitialDataReady;
		public event GameDataReadyEventHandler OnGameDataReady;
		public event TurnStartedEventHandler OnTurnStarted;
		public event BoardToBoardMovedEventHandler OnBoardToBoardMoved;
		public event BoardToBarMovedEventHandler OnBoardToBarMoved;
		public event BarToBoardMovedEventHandler OnBarToBoardMoved;
		public event BearedOffEventHandler OnBearedOff;
		public event TurnFinishedEventHandler OnTurnFinished;
		public event GameFinishedEventHandler OnGameFinished;
		public event ChatReceivedEventHandler OnChatReceived;
		public event PurchaseFinishedEventHandler OnPurchaseFinished;

		public Network()
		{
			sendBuffer = new BufferStream(new byte[BUFFER_SIZE]);

			OnBufferReceived += Connection_OnBufferReceived;
		}

		public void VersionCheck(int Version)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.VERSION_CHECK);
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

		public void SetUserInfo(string Username)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.SET_USER_INFO);
			sendBuffer.WriteString(Username);

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

		public void JoinToRoom(uint TableEnterance, bool WithBot)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteUInt32(TableEnterance);
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

		public void GetInitialData()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);

			Send(sendBuffer);
		}

		public void GetGameData()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

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

		private void Connection_OnBufferReceived(BufferStream Buffer)
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
					int id = Buffer.ReadInt32();
					string username = Buffer.ReadString();

					if (OnAuthenticationRespond != null)
						OnAuthenticationRespond(result, id, username);
				}
				else if (command == Commands.Lobby.GET_USER_INFO)
				{
					int userID = Buffer.ReadInt32();
					string data = Buffer.ReadString();

					if (OnUserInfoReady != null)
						OnUserInfoReady(userID, data);
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
					string data = Buffer.ReadString();

					if (OnInitialDataReady != null)
						OnInitialDataReady(data);
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
			}
			else if (category == Commands.Category.ROOM)
			{
				if (command == Commands.Room.GET_GAME_DATA)
				{
					PlayerColors color = (PlayerColors)Buffer.ReadInt32();

					if (OnGameDataReady != null)
						OnGameDataReady(color);
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
						OnBoardToBoardMoved(hash, fromIdentifier, fromIdentifier);
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
