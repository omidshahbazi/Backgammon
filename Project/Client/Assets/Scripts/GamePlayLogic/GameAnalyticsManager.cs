using ClientUtilities.Singleton;
using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameAnalyticsManager : MonoBehaviorSingleton<GameAnalyticsManager>
{
    public void Awake()
    {
        GameAnalytics.Initialize();
        GameAnalytics.StartSession();
    }

    public void SetUserID(int ID)
    {
        GameAnalytics.SetCustomId(ID.ToString());
    }

    public void SendEvent(string EventName)
    {
        GameAnalytics.NewDesignEvent(EventName);
    }

    public void SendCoinSinkEvent(float Amount, string ItemType)
    {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", Amount, ItemType, string.Empty);
    }

    public void SendCoinSourceEvent(float Amount, string ItemType)
    {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Coin", Amount, ItemType, string.Empty);
    }

    public void SendUIOpened(string Name)
    {
        SendEvent("UIOpend ==" + Name);
    }

    public void SendUIClosed(string Name)
    {
        SendEvent("UIClose ==" + Name);
    }

    public void SendButtonClicked(string Name)
    {
        SendEvent("ButtonCliked" + Name);
    }

    public void SendErrorEvent(GAErrorSeverity Severity, string Message)
    {
        GameAnalytics.NewErrorEvent(Severity, Message);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameAnalytics.EndSession();
    }

   

}
