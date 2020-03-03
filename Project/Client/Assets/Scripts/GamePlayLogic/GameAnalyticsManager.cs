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
    public enum Currency
    {
        USD,
        IRR
    }

    private interface GaEventbase
    {
        string Message { get; set; }
    }

    private struct CostumeEvent : GaEventbase
    {
        public string Message { get; set; }

        public CostumeEvent(string messgae) : this()
        {
            this.Message = messgae;
        }
    }

    private struct CostumeDimension : GaEventbase
    {
        public string Message { get; set; }

        public CostumeDimension(string messgae) : this()
        {
            this.Message = messgae;
        }
    }

    private struct ErrorEvent : GaEventbase
    {
        public string Message { get; set; }
        public GAErrorSeverity errorSeverity { get; set; }

        public ErrorEvent(string messgae, GAErrorSeverity errorSeverity) : this()
        {
            this.Message = messgae;
            this.errorSeverity = errorSeverity;
        }
    }

    private struct ResourceEvent : GaEventbase
    {
        public string Message { get; set; }
        public GAResourceFlowType flowType { get; set; }
        public string currency { get; set; }
        public string place { get; set; }
        public string caretType { get; set; }

        public ResourceEvent(string messgae, GAResourceFlowType flowType, string currency, string plcae, string caretType) : this()
        {
            this.Message = messgae;
            this.flowType = flowType;
            this.currency = currency;
            this.place = plcae;
            this.caretType = caretType;

        }
    }

    private struct BussinesEvent : GaEventbase
    {
        public string Message { get; set; }
        public int Amount { get; set; }
        public string ItemType { get; set; }
        public string ItemID { get; set; }
        public string CaretType { get; set; }

        public BussinesEvent(string messgae, int amount, string itemType, string itemID, string caretType) : this()
        {
            this.Message = messgae;
            Amount = amount;
            ItemType = itemType;
            ItemID = itemID;
            CaretType = caretType;
        }
    }

    private static object callLock = new object();
    private List<GaEventbase> events = new List<GaEventbase>();
    private float sendEventsTime;
    private float period = 1F;

    public void Initilize(string Dimension)
    {
        lock (callLock)
        {
            IntilizeCustomDimension(Dimension);
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

    public void SendCustomDimension(string Dimension)
    {
        if (GameAnalytics.SettingsGA.CustomDimensions01.Contains(Dimension))
        {
            GameAnalytics.SetCustomDimension01(Dimension);
        }
        else if (GameAnalytics.SettingsGA.CustomDimensions02.Contains(Dimension))
        {
            GameAnalytics.SetCustomDimension02(Dimension);
        }
        else if (GameAnalytics.SettingsGA.CustomDimensions03.Contains(Dimension))
        {
            GameAnalytics.SetCustomDimension03(Dimension);
        }

    }

    public void IntilizeCustomDimension(string Dimension)
    {
        if (GameAnalytics.SettingsGA.CustomDimensions01.Count < 20)
        {
            if (!GameAnalytics.SettingsGA.CustomDimensions01.Contains(Dimension))
            {
                GameAnalytics.SettingsGA.CustomDimensions01.Add(Dimension);
            }
            return;
        }

        if (GameAnalytics.SettingsGA.CustomDimensions02.Count < 20)
        {
            if (!GameAnalytics.SettingsGA.CustomDimensions02.Contains(Dimension))
            {
                GameAnalytics.SettingsGA.CustomDimensions02.Add(Dimension);
            }
            return;
        }

        if (GameAnalytics.SettingsGA.CustomDimensions03.Count < 20)
        {
            if (!GameAnalytics.SettingsGA.CustomDimensions03.Contains(Dimension))
            {
                GameAnalytics.SettingsGA.CustomDimensions03.Add(Dimension);
            }
        }
    }

    public void SendEvent(string EventName)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new CostumeEvent(EventName));
        }
    }

    public void SendCoinSinkEvent(float Amount, string Place, string Carttype)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new ResourceEvent(Amount.ToString(), GAResourceFlowType.Sink, "Coin", Place, Carttype));
        }
    }

    public void SendCoinSourceEvent(float Amount, string Place, string Carttype)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new ResourceEvent(Amount.ToString(), GAResourceFlowType.Source, "Coin", Place, Carttype));
        }
    }

    public void SendBussinesEvent(string messgae, int amount, string itemType, string itemID, string caretType)
    {
        lock (callLock)
        {
            if (!GameAnalytics._hasInitializeBeenCalled)
                return;

            events.Add(new BussinesEvent(messgae, amount, itemType, itemID, caretType));
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
                GameAnalytics.NewDesignEvent(co.Message);
            }
            else if (type == typeof(ResourceEvent))
            {
                ResourceEvent re = (ResourceEvent)ev;
                GameAnalytics.NewResourceEvent(re.flowType, re.currency, float.Parse(re.Message), re.place, re.caretType);
            }
            else if (type == typeof(ErrorEvent))
            {
                ErrorEvent ee = (ErrorEvent)ev;
                GameAnalytics.NewErrorEvent(ee.errorSeverity, ee.Message);

            }
            else if (type == typeof(BussinesEvent))
            {
                BussinesEvent be = (BussinesEvent)ev;
                GameAnalytics.NewBusinessEvent(be.Message, be.Amount, be.ItemType, be.ItemID, be.CaretType);
            }
            else if (type == typeof(CostumeDimension))
            {
                CostumeDimension cd = (CostumeDimension)ev;
                GameAnalytics.SetCustomDimension01(cd.Message);
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
