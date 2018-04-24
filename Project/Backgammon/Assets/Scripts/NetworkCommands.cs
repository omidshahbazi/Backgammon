using GameServer.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Response
{
	private Action<Dictionary<byte, object>> callback = null;

	public void Then(Action<Dictionary<byte, object>> Callback)
	{
		callback = Callback;
	}

	public void __OnResponse(Dictionary<byte, object> Response)
	{
		if (callback != null)
			callback(Response);
	}
}

public static class NetworkCommands
{
	public static Response Authenticate()
	{
		return SendMessage(ParameterTypes.MessageType, MessageTypes.Authenticate);
	}

	public static Response GetUserInfo()
	{
		return SendMessage(ParameterTypes.MessageType, MessageTypes.GetUserInfo);
	}

	public static Response GetAMatch()
	{
		return SendMessage(ParameterTypes.MessageType, MessageTypes.GetAMatch);
	}

	public static Response GetDice()
	{
		return SendMessage(ParameterTypes.MessageType, MessageTypes.GetDice);
	}

	private static Response SendMessage(params object[] Parameters)
	{
		List<object> parameters = new List<object>();
		parameters.Add(ParameterTypes.UDID);
		parameters.Add(SystemInfo.deviceUniqueIdentifier);

		parameters.AddRange(Parameters);

		Response response = new Response();

		NetworkManager.Instance.SendMessage(response.__OnResponse, parameters.ToArray());

		return response;
	}
}