using ClientUtilities.Singleton;
using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameAnalyticsManager : MonoBehaviorSingleton<GameAnalyticsManager>
{
    //public enum EventType
    //{
    //    CostumeEvent,
    //    ErrorEvent,
    //    ResourceEvent
    //}


    private interface GaEventbase
    {
        string messgae { get; set; }
    }

    private struct CostumeEvent : GaEventbase
    {
        public string messgae { get; set; }

        public CostumeEvent(string messgae) : this()
        {
            this.messgae = messgae;
        }
    }

    private struct ErrorEvent : GaEventbase
    {
        public string messgae { get; set; }
        public GAErrorSeverity errorSeverity { get; set; }

        public ErrorEvent(string messgae, GAErrorSeverity errorSeverity) : this()
        {
            this.messgae = messgae;
            this.errorSeverity = errorSeverity;
        }
    }

    private struct ResourceEvent : GaEventbase
    {
        public string messgae { get; set; }
        public GAResourceFlowType flowType { get; set; }
        public string currency { get; set; }
        public string place { get; set; }
        public string caretType { get; set; }

        public ResourceEvent(string messgae, GAResourceFlowType flowType, string currency, string plcae, string caretType) : this()
        {
            this.messgae = messgae;
            this.flowType = flowType;
            this.currency = currency;
            this.place = plcae;
            this.caretType = caretType;
        }
    }

    private static object callLock = new object();
    private List<GaEventbase> events = new List<GaEventbase>();
    private float sendEventsTime;
    private float period = 1F;

    public void Initilize()
    {
        lock (callLock)
        {
            GameAnalytics.Initialize();
        }
    }

    public void SetUserID(int ID)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;
            GameAnalytics.StartSession();
            GameAnalytics.SetCustomId(ID.ToString());
        }

    }

    public void SendEvent(string EventName)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new CostumeEvent(EventName));

            // GameAnalytics.NewDesignEvent(EventName);
        }
    }

    public void SendCoinSinkEvent(float Amount, string Place, string Carttype)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new ResourceEvent(Amount.ToString(), GAResourceFlowType.Sink, "Coin", Place, Carttype));
            //GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", Amount, Place, Carttype);
        }
    }

    public void SendCoinSourceEvent(float Amount, string Place, string Carttype)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new ResourceEvent(Amount.ToString(), GAResourceFlowType.Source, "Coin", Place, Carttype));

            //GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Coin", Amount, Place, Carttype);
        }
    }

    public void SendUIOpened(string Name)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;
            SendEvent("UIOpend " + Name);
        }
    }

    public void SendUIClosed(string Name)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;
            SendEvent("UIClosed " + Name);
        }
    }

    public void SendButtonClicked(string Name)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;
            SendEvent("ButtonCliked" + Name);
        }
    }

    public void SendErrorEvent(GAErrorSeverity Severity, string Message)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new ErrorEvent(Message, Severity));

            //GameAnalytics.NewErrorEvent(Severity, Message);
        }
    }

    private void Update()
    {
        if (Time.time < sendEventsTime)
            return;

        sendEventsTime = Time.time + period;
        for (int i = 0; i < events.Count; ++i)
        {
            GaEventbase ev = events[i];
            Type type = ev.GetType();
          
                if (type == typeof(CostumeEvent))
                {
                    CostumeEvent co = (CostumeEvent)ev;
                    GameAnalytics.NewDesignEvent(co.messgae);
                }
                else if (type == typeof(ResourceEvent))
                {
                    ResourceEvent re = (ResourceEvent)ev;
                    GameAnalytics.NewResourceEvent(re.flowType, re.currency, float.Parse(re.messgae), re.place, re.caretType);
                }
                else if (type == typeof(ErrorEvent))
                {
                    ErrorEvent ee = (ErrorEvent)ev;
                    GameAnalytics.NewErrorEvent(ee.errorSeverity, ee.messgae);

                }
            lock (callLock)
            {
                events.RemoveAt(i--);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!GameAnalytics._hasInitializeBeenCalled)
            return;
        GameAnalytics.EndSession();
    }

}
