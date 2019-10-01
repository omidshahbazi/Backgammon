using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using Zorvan.Framework.BinarySerializer;

namespace Networking.Client
{
	public delegate void AuthenticationRespondEventHandler(AuthenticateResult Result, int ID, string Username);
	public delegate void JoinedToRoomEventHandler(int GameID, int OtherPlayerID);
	public delegate void InitialDataReadyEventHandler(string Data);
	public delegate void BoardToBoardMovedEventHandler(int Hash, Identifier FromIdentifier, Identifier ToIdentifier);
	public delegate void BarToBoardMovedEventHandler(int Hash, PlayerColors Color, Identifier ToIdentifier);
	public delegate void BearedOffEventHandler(int Hash, Identifier FromIdentifier);
	public delegate void TurnFinishedEventHandler(int Hash, PlayerColors Color);
	public delegate void ResignedEventHandler();

	public class Network : Connection
	{
		private const int BUFFER_SIZE = 32;

		private BufferStream sendBuffer = null;

		public event AuthenticationRespondEventHandler OnAuthenticationRespond;
		public event JoinedToRoomEventHandler OnJoinedToRoom;
		public event InitialDataReadyEventHandler OnInitialDataReady;
		public event BoardToBoardMovedEventHandler OnBoardToBoardMoved;
		public event BarToBoardMovedEventHandler OnBarToBoardMoved;
		public event BearedOffEventHandler OnBearedOff;
		public event TurnFinishedEventHandler OnTurnFinished;
		public event ResignedEventHandler OnResigned;

		public Network()
		{
			sendBuffer = new BufferStream(new byte[BUFFER_SIZE]);

			OnBufferReceived += Connection_OnBufferReceived;
		}

		public void Authenticate(string Username, string Password)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			sendBuffer.WriteString(Username);
			sendBuffer.WriteString(Password);

			Send(sendBuffer);
		}

		public void JoinToRoom(int TableEnterance, bool WithBot)
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			sendBuffer.WriteInt32(TableEnterance);
			sendBuffer.WriteBool(WithBot);

			Send(sendBuffer);
		}

		public void CancelJoinToRoom()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.CANCEL_JOIN_TO_ROOM);

			Send(sendBuffer);
		}

		public void GetInitialData()
		{
			sendBuffer.Reset();
			sendBuffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);

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

		private void Connection_OnBufferReceived(BufferStream Buffer)
		{
			byte category = Buffer.ReadByte();
			byte command = Buffer.ReadByte();

			if (category == Commands.Category.LOBBY)
			{
				if (command == Commands.Lobby.AUTHENTICATE)
				{
					AuthenticateResult result = (AuthenticateResult)Buffer.ReadInt32();
					int id = -1;
					string username = "";

					if (result == AuthenticateResult.Passed)
					{
						id = Buffer.ReadInt32();
						username = Buffer.ReadString();
					}

					if (OnAuthenticationRespond != null)
						OnAuthenticationRespond(result, id, username);
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
					int otherPlayerID = Buffer.ReadInt32();

					if (OnJoinedToRoom != null)
						OnJoinedToRoom(gameID, otherPlayerID);
				}
			}
			else if (category == Commands.Category.ROOM)
			{
				if (command == Commands.Room.BOARD_TO_BOARD_MOVE)
				{
					int hash = Buffer.ReadInt32();
					Identifier fromIdentifier = new Identifier(Buffer.ReadInt32());
					Identifier toIdentifier = new Identifier(Buffer.ReadInt32());

					if (OnBoardToBoardMoved != null)
						OnBoardToBoardMoved(hash, fromIdentifier, fromIdentifier);
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
				else if (command == Commands.Room.RESIGN)
				{
					if (OnResigned != null)
						OnResigned();
				}
			}
		}
	}
}
