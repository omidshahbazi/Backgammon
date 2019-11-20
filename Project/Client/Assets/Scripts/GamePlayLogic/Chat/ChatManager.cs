using Assets.Scripts.GamePlayLogic.RequestManagers;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void SimpleChatRecived(int Index);

interface BaseChat
{
  
}

public class SimpleChat : BaseChat
{
    public string Content
    {
        get;
        private set;
    }

    public SimpleChat(string content)
    {
        Content = content;
    }
}


public class ChatManager : MonoBehaviorSingleton<ChatManager>
{
    public event SimpleChatRecived OnSimpleChatRecived = null;
    public enum ChatType
    {
        SimpleChat =0,
    }


    private void Awake()
    {
 
    }

    private void Network_OnChatReceived(int TextIndex)
    {
        OnSimpleChatRecived?.Invoke(TextIndex);
    }

    public SimpleChat [] SimpleChatList
    {
        get;
        private set;
    }

    public void DeserializeSimpleChat(ISerializeArray Array)
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.Network.OnChatReceived += Network_OnChatReceived;

        GameAnalyticsManager.Instance.SendEvent("Simple Chat Deserialize Begin");
        SimpleChatList = new SimpleChat[Array.Count];

        for(uint i = 0; i<SimpleChatList.Length;++i)
        {
            string obj = Array.Get<string>(i);
            SimpleChatList[i] = new SimpleChat(obj);
        }
        GameAnalyticsManager.Instance.SendEvent("Simple Chat Deserialize End");

    }

    public void SendSimpleChat(int Index)
    {
        RequestManager.Instance.Network.SendChat(Index);
        GameAnalyticsManager.Instance.SendEvent("Simple Chat " + Index);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
