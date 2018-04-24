// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using ExitGames.Client.Photon;
using GameServer.Common;
using System.Collections.Generic;

namespace GameServer.Client
{
	public class MessageReceivedEventArgs
	{
		public Dictionary<byte, object> Parameters
		{
			get;
			private set;
		}

		public MessageReceivedEventArgs(Dictionary<byte, object> Parameters)
		{
			this.Parameters = Parameters;
		}
	}

	public class ResponseReceivedEventArgs : MessageReceivedEventArgs
	{
		public int Number
		{
			get;
			private set;
		}

		public ResponseReceivedEventArgs(int Number, Dictionary<byte, object> Parameters) :
			base(Parameters)
		{
			this.Number = Number;
		}
	}

	public delegate void ConnectionEventHandler();
	public delegate void MessageReceivedEventHandler(MessageReceivedEventArgs e);
	public delegate void ResponseReceivedEventHandler(ResponseReceivedEventArgs e);

	public class Connection : IPhotonPeerListener
	{
		private PhotonPeer peer = null;
		private int lastMessageNumber = 0;

		public event ConnectionEventHandler Connected = null;
		public event ConnectionEventHandler Disconnected = null;
		public event MessageReceivedEventHandler MessageReceived = null;
		public event ResponseReceivedEventHandler ResponseReceived = null;

		public string Address
		{
			get;
			set;
		}

		public bool IsConnected
		{
			get;
			private set;
		}

		public void Update()
		{
			if (peer == null)
				return;

			peer.Service();
		}

		public void Connect()
		{
			peer = new PhotonPeer(this, ConnectionProtocol.Udp);
			//peer.IsSimulationEnabled = false;
			peer.Connect(Address, string.Empty);
		}

		public void Disconnect()
		{
			peer.Disconnect();
			peer = null;

			OnDisconnected();
		}

		public void DebugReturn(DebugLevel Level, string Message)
		{
		}

		public void OnEvent(EventData EventData)
		{
		}

		public void OnMessage(object Message)
		{
		}

		public void OnOperationResponse(OperationResponse OperationResponse)
		{
			if (OperationResponse.Parameters.ContainsKey((byte)ParameterTypes.MessageNumber))
			{
				if (ResponseReceived != null)
					ResponseReceived(new ResponseReceivedEventArgs(ParameterHelper.GetParameter<int>(OperationResponse.Parameters, ParameterTypes.MessageNumber), OperationResponse.Parameters));
			}
			else
			{
				if (MessageReceived != null)
					MessageReceived(new MessageReceivedEventArgs(OperationResponse.Parameters));
			}
		}

		public void OnStatusChanged(StatusCode StatusCode)
		{
			switch (StatusCode)
			{
				case StatusCode.Connect:
					OnConnected();
					break;
				case StatusCode.Disconnect:
					OnDisconnected();
					break;
			}
		}

		public int SendMessage(params object[] Parameters)
		{
			peer.OpCustom((byte)OperationTypes.InGame, ParameterHelper.Combine(ParameterHelper.MakeMap(ParameterTypes.MessageNumber, ++lastMessageNumber), ParameterHelper.MakeMap(Parameters)), true);

			return lastMessageNumber;
		}

		private void OnConnected()
		{
			IsConnected = true;

			if (Connected != null)
				Connected();
		}

		private void OnDisconnected()
		{
			IsConnected = false;

			if (Disconnected != null)
				Disconnected();
		}
	}
}