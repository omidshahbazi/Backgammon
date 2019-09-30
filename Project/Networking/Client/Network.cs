using Networking.Common;
using Zorvan.Framework.BinarySerializer;

namespace Networking.Client
{
	public delegate void AuthenticationRespondEventHandler(AuthenticateResult Result, int ID, string Username);
	public delegate void JoinedToRoomEventHandler();
	public delegate void InitialDataReadyEventHandler(string Data);
	public delegate void CheckerMovedEventHandler();
	public delegate void ResignedEventHandler();

	public class Network : Connection
	{
		private const int BUFFER_SIZE = 32;

		private BufferStream buffer = null;

		public event AuthenticationRespondEventHandler OnAuthenticationRespond;
		public event JoinedToRoomEventHandler OnJoinedToRoom;
		public event InitialDataReadyEventHandler OnInitialDataReady;
		public event CheckerMovedEventHandler OnCheckerMoved;
		public event ResignedEventHandler OnResigned;

		public Network()
		{
			buffer = new BufferStream(new byte[BUFFER_SIZE]);

			OnBufferReceived += Connection_OnBufferReceived;
		}

		public void Authenticate(string Username, string Password)
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.AUTHENTICATE);
			buffer.WriteString(Username);
			buffer.WriteString(Password);

			Send(buffer);
		}

		public void JoinToRoom(int TableEnterance)
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.JOIN_TO_ROOM);
			buffer.WriteInt32(TableEnterance);

			Send(buffer);
		}

		public void CancelJoinToRoom()
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.CANCEL_JOIN_TO_ROOM);

			Send(buffer);
		}

		public void GetInitialData()
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.LOBBY, Commands.Lobby.GET_INITIAL_DATA);

			Send(buffer);
		}

		public void MoveChecker()
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.ROOM, Commands.Room.MOVE_CHECKER);

			Send(buffer);
		}

		public void Resign()
		{
			buffer.Reset();

			buffer.WriteBytes(Commands.Category.ROOM, Commands.Room.RESIGN);

			Send(buffer);
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
					if (OnJoinedToRoom != null)
						OnJoinedToRoom();
				}
			}
			else if (category == Commands.Category.ROOM)
			{
				if (command == Commands.Room.MOVE_CHECKER)
				{
					if (OnCheckerMoved != null)
						OnCheckerMoved();
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
