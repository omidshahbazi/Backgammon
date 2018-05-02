// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using GameServer.Common;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace GameServer.Server
{
	class ClientInstance : ClientPeer
	{
		public int ID
		{
			get;
			private set;
		}

		public string UDID
		{
			get;
			private set;
		}

		public GameInstance Game
		{
			get;
			private set;
		}

		private Dictionary<MessageTypes, Action<int, Dictionary<byte, object>>> requestsHandler = new Dictionary<MessageTypes, Action<int, Dictionary<byte, object>>>();

		public ClientInstance(InitRequest Request) :
			base(Request)
		{
			requestsHandler[MessageTypes.Authenticate] = Authenticate;
			requestsHandler[MessageTypes.GetUserInfo] = GetUserInfo;
			requestsHandler[MessageTypes.GetAMatch] = GetAMatch;
			requestsHandler[MessageTypes.GetDice] = GetDice;
		}

		protected override void OnDisconnect(DisconnectReason ReasonCode, string ReasonDetail)
		{
			Database.Execute("UPDATE users SET is_online=0 WHERE id=@ID", "ID", ID);

			UserManager.Instance.AddUser(this);
		}

		protected override void OnOperationRequest(OperationRequest OperationRequest, SendParameters SendParameters)
		{
			MessageTypes messageType = ParameterHelper.GetParameter<MessageTypes>(OperationRequest.Parameters, ParameterTypes.MessageType);

			if (requestsHandler.ContainsKey(messageType))
			{
				int messageNumber = ParameterHelper.GetParameter<int>(OperationRequest.Parameters, ParameterTypes.MessageNumber);

				requestsHandler[messageType](messageNumber, OperationRequest.Parameters);
			}
		}

		private void Authenticate(int Number, Dictionary<byte, object> Parameters)
		{
			UDID = ParameterHelper.GetParameter<string>(Parameters, ParameterTypes.UDID);

			DataTable userTable = Database.ExecuteWithReturn("SELECT id FROM users WHERE udid=@UDID", "UDID", UDID);

			if (userTable.Rows.Count == 0)
			{
				Database.Execute("INSERT INTO users(udid, is_online, looking_for_match) VALUES(@UDID, 1, 0)", "UDID", UDID);
				ID = Database.GetLastInsertID();
			}
			else
			{
				ID = Convert.ToInt32(userTable.Rows[0]["id"]);
				Database.Execute("UPDATE users SET is_online=1 WHERE id=@ID", "ID", ID);
			}

			SendOperation(MessageTypes.Authenticate, Number);

			UserManager.Instance.AddUser(this);
		}

		private void GetUserInfo(int Number, Dictionary<byte, object> Parameters)
		{
			SendOperation(MessageTypes.GetUserInfo, Number);
		}

		private void GetAMatch(int Number, Dictionary<byte, object> Parameters)
		{
			Database.Execute("UPDATE users SET looking_for_match=1 WHERE id=@ID", "ID", ID);

			DataTable table = Database.ExecuteWithReturn("SELECT id FROM users WHERE is_online=1 AND looking_for_match=1 AND id<>@ID", "ID", ID);

			if (table.Rows.Count == 0)
			{
				GameManager.Instance.AddWaitingGame(new GameInstance(this));

				SendOperation(MessageTypes.GetAMatch, Number);
				return;
			}

			ClientInstance client = UserManager.Instance.GetByID(Convert.ToInt32(table.Rows[0]["id"]));

			if (client == null)
			{
				GameManager.Instance.AddWaitingGame(new GameInstance(this));

				SendOperation(MessageTypes.GetAMatch, Number);
				return;
			}

			GameInstance game = GameManager.Instance.GetWaitingByClient(client);

			if (game == null)
			{
				GameManager.Instance.AddWaitingGame(new GameInstance(this));

				SendOperation(MessageTypes.GetAMatch, Number);
				return;
			}

			game.Join(this);
			Game = game;
		}

		private void GetDice(int Number, Dictionary<byte, object> Parameters)
		{
			if (Game == null)
				return;

			int dice1, dice2;
			Game.GetDice(out dice1, out dice2, true);

			SendOperation(MessageTypes.GetDice, Number, ParameterTypes.Dice1, dice2, ParameterTypes.Dice2, dice2);
		}

		private void SendOperation(MessageTypes Type, int Number, params object[] Parameters)
		{
			SendOperationResponse(new OperationResponse((byte)OperationTypes.InGame, ParameterHelper.Combine(ParameterHelper.MakeMap(ParameterTypes.MessageType, Type, ParameterTypes.MessageNumber, Number), ParameterHelper.MakeMap(Parameters))), new SendParameters());
		}

		public void SendOperation(MessageTypes Type, params object[] Parameters)
		{
			SendOperationResponse(new OperationResponse((byte)OperationTypes.InGame, ParameterHelper.Combine(ParameterHelper.MakeMap(ParameterTypes.MessageType, Type), ParameterHelper.MakeMap(Parameters))), new SendParameters());
		}
	}
}