using ClientUtilities.Singleton;
using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameAnalyticsManager : MonoBehaviorSingleton<GameAnalyticsManager>
{

    public void Initilize()
    {
        GameAnalytics.Initialize();
    }

    public void SetUserID(int ID)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.StartSession();
        GameAnalytics.SetCustomId(ID.ToString());
    }

    public void SendEvent(string EventName)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.NewDesignEvent(EventName);
    }

    public void SendCoinSinkEvent(float Amount,string Place ,string Carttype)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", Amount, Place, Carttype);
    }

    public void SendCoinSourceEvent(float Amount, string Place ,string Carttype)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Coin", Amount, Place, Carttype);
    }

    public void SendUIOpened(string Name)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        SendEvent("UIOpend " + Name);
    }

    public void SendUIClosed(string Name)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        SendEvent("UIClosed " + Name);
    }

    public void SendButtonClicked(string Name)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        SendEvent("ButtonCliked" + Name);
    }

    public void SendErrorEvent(GAErrorSeverity Severity, string Message)
    {
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.NewErrorEvent(Severity, Message);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.EndSession();
    }

}
