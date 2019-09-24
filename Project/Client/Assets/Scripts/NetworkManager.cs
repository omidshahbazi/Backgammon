using GameServer.Client;
using GameServer.Common;
using System;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviorBase
{
	public static NetworkManager Instance
	{
		get;
		private set;
	}

	private Connection connection = null;
	private Dictionary<int, Action<Dictionary<byte, object>>> callbacks = new Dictionary<int, Action<Dictionary<byte, object>>>();
	private Dictionary<MessageTypes, Action<Dictionary<byte, object>>> messageCallbacks = new Dictionary<MessageTypes, Action<Dictionary<byte, object>>>();

	public bool IsConnected
	{
		get { return (connection == null ? false : connection.IsConnected); }
	}

	protected override void Awake()
	{
		Instance = this;

		base.Awake();
	}

	protected override void Update()
	{
		base.Update();

		if (connection == null)
			return;

		connection.Update();
	}

	public void Connect()
	{
		if (connection == null)
			connection = new Connection();
		else
			connection.Disconnect();

		connection.Address = "192.168.1.10:2288";
		connection.Connect();
		connection.Connected += Connected;
		connection.Disconnected += Disconnected;
		connection.MessageReceived += MessageReceived;
		connection.ResponseReceived += ResponseReceived;
	}

	private void Connected()
	{
	}

	private void Disconnected()
	{
	}

	private void ResponseReceived(ResponseReceivedEventArgs e)
	{
		if (!callbacks.ContainsKey(e.Number))
			return;

		callbacks[e.Number](e.Parameters);

		callbacks.Remove(e.Number);
	}

	private void MessageReceived(MessageReceivedEventArgs e)
	{
		MessageTypes messageType = ParameterHelper.GetParameter<MessageTypes>(e.Parameters, ParameterTypes.MessageType);

		if (messageCallbacks.ContainsKey(messageType))
			messageCallbacks[messageType](e.Parameters);
	}

	public void RegisterMessageTypeCallback(MessageTypes Type, Action<Dictionary<byte, object>> Callback)
	{
		messageCallbacks[Type] = Callback;
	}

	public void SendMessage(Action<Dictionary<byte, object>> OnResponse, params object[] Parameters)
	{
		int number = connection.SendMessage(Parameters);

		if (OnResponse != null)
			callbacks[number] = OnResponse;
	}
}