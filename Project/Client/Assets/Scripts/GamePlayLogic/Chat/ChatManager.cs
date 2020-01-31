using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Networking.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void SimpleChatRecived(int PackID,int Index);

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

public class ChatPack
{
    public int ID
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public SimpleChat[] Chat
    {
        get;
        private set;
    }

    public bool IsSold
    {
        get;
        set;
    }

    public CostInfo Cost
    {
        get;
        private set;
    }

    public RewardInfo Reward
    {
        get;
        private set;
    }


    public ChatPack(int ID, string Name, SimpleChat[] SimpleChatList, RewardInfo Reward, CostInfo Cost, bool IsSold)
    {
        this.ID = ID;
        this.Name = Name;
        this.Chat = SimpleChatList;
        this.IsSold = IsSold;
        this.Reward = Reward;
        this.Cost = Cost;
    }
}


public class ChatManager : MonoBehaviorSingleton<ChatManager>
{
    public event SimpleChatRecived OnSimpleChatRecived = null;
    private Action OnChatStateUpdate = null;

    public enum ChatType
    {
        SimpleChat = 0,
    }

    private void Network_OnChatReceived(int PackID ,int ChatID)
    {
        OnSimpleChatRecived?.Invoke(PackID,ChatID);
    }

    public ChatPack[] SimpleChatList
    {
        get;
        private set;
    }

    public void DeserializeSimpleChat(ISerializeArray Array)
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.Network.OnChatReceived += Network_OnChatReceived;

        GameAnalyticsManager.Instance.SendEvent("Simple Chat Deserialize Begin");
        SimpleChatList = new ChatPack[Array.Count];
        int id = int.MinValue;
        string name = string.Empty;
        for (uint i = 0; i < SimpleChatList.Length; ++i)
        {
            SimpleChat[] chat = null;
            CostInfo cost = new CostInfo();
            RewardInfo reward = new RewardInfo();
            ISerializeObject obj = Array.Get<ISerializeObject>(i);
            if (obj.IsContains("ID"))
                id = obj.Get<int>("ID");
            if (obj.IsContains("Name"))
                name = obj.Get<string>("Name");
            if (obj.IsContains("Cost"))
                cost.Deserialize(obj.Get<ISerializeObject>("Cost"));
            if (obj.IsContains("Reward"))
                reward.Deserialize(obj.Get<ISerializeObject>("Reward"));
            if (obj.IsContains("Chat"))
            {
                ISerializeArray chatArray = obj.Get<ISerializeArray>("Chat");
                chat = new SimpleChat[chatArray.Count];
                for (uint j = 0; j < chat.Length; ++j)
                {
                    chat[j] = new SimpleChat(chatArray.Get<string>(j));
                }
            }

            bool isExist = false;
            for (int k = 0; k < UserInfoManager.Instance.User.ChatPack.Length; ++k)
            {
                if (id != UserInfoManager.Instance.User.ChatPack[k])
                    continue;

                isExist = true;
                break;

            }
            SimpleChatList[i] = new ChatPack(id, name, chat, reward, cost, isExist);
        }
        GameAnalyticsManager.Instance.SendEvent("Simple Chat Deserialize End");

    }

    public void SendSimpleChat( int PackID,int Index)
    {
        RequestManager.Instance.Network.SendChat(PackID,Index);
        GameAnalyticsManager.Instance.SendEvent("Simple Chat " + Index);
    }

    public void BuyChat(int ID,Action OnComplete)
    {
        RequestManager.Instance.Network.BuyChatPack(ID);
        OnChatStateUpdate = null;
        OnChatStateUpdate = OnComplete;
    }

    public void UpdateChatStates()
    {
        for (uint i = 0; i < SimpleChatList.Length; ++i)
        {
            for (int k = 0; k < UserInfoManager.Instance.User.ChatPack.Length; ++k)
            {
                if (SimpleChatList[i].ID != UserInfoManager.Instance.User.ChatPack[k])
                    continue;

                SimpleChatList[i].IsSold = true;
                break;

            }
        }

        OnChatStateUpdate?.Invoke();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
