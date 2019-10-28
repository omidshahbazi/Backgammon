using Assets.Scripts.ClientUtilities.ScheduleSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Assets.Scripts.PushNotification
{

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public class Pushe : INotificationServices
    {
        private AndroidJavaObject activityContext = null;
        public bool showGooglePlayDialog = true; //if it is true, user will see a dialog for installing GooglePlayService if it is not installed on her/his device

        public PushNotificationAPIs API
        {
            get
            {
                return PushNotificationAPIs.Pushe;
            }
        }

        public event OnInitializeCompletedHandler InitializeCompleted;

        public void OnInitializeCompleted(OnInitializeCompletedHandler OnComplete)
        {
            Debug.Log("[PusheServices] OnInitializeRegisterCalled  ");

            InitializeCompleted += OnComplete;
        }

        private void Register()
        {
            Debug.Log("[PusheServices] RegisterCall  ");
            if (IsPusheInitialized())
            {
                Debug.Log("[PusheServices] RegisterCall Begin  ");
                SetNotificationOn();
                //Subscribe("PublicChannel");
                if (InitializeCompleted != null)
                {
                    InitializeCompleted(GetPusheId());
                    Debug.Log("[PusheServices] RegisterCalled  ");
                }
            }
            else
            {
                Debug.Log("[PusheServices] Pushe NotInitialized");
                Debug.Log("[PusheServices] Pushe scheduled ");
                ScheduleManager.Instance.AddSchedule(Register, 5);
            }

        }


        public void Initialize(string ID)
        {
            try
            {
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

                //getting context of unity activity
                activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                //calling plugin class by package name
                AndroidJavaClass pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");

                Debug.Log("[PusheServices] Initialize Begin ");

                if (pluginClass != null)
                {
                    Debug.Log("[PusheServices] Activity Context Call ");
                    activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        //calling initialize static method
                        pluginClass.CallStatic("initialize", new object[2] { activityContext, showGooglePlayDialog });
                        Debug.Log("[PusheServices] Initialized  ");

                    }));
                    Register();
                }
                else
                {
                    Debug.Log("[PusheServices] Plugin Class is null");
                    GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "[PusheServices] Plugin Class is null");

                }

            }
            catch (Exception e)
            {
                Debug.Log("[PusheServices] Failed to initialize the project somehow!");
                GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "Push Initilize Failed" + e.ToString());
            }
        }

        public void AddNotificationReceivedHandler(OnNotificationReceivedHandler OnRecieved)
        {
        }

        public void RemoveNotificationReceivedHandler(OnNotificationReceivedHandler OnRecieved)
        {
        }




        /**
         * Call for subscribing to a topic. It has to be called after Pushe.initialize() has completed its work
         * So, call it with a reasonable delay (30 sec to 2 min) after Pushe.initialize()
         **/
        public static void Subscribe(string topic)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            pluginClass.CallStatic("subscribe", context, topic);
        }

        /**
         * Call for unSubscribing from a topic. It has to be called after Pushe.initialize() has completed its work
         * So, call it with a reasonable delay (30 sec to 2 min) after Pushe.initialize()
         **/
        public static void Unsubscribe(string topic)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            pluginClass.CallStatic("unsubscribe", context, topic);
        }

        /**
         * Call this method to enable publishing notification to user, if you already called SetNotificationOff()
         **/
        public static void SetNotificationOn()
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("setNotificationOn", context);
            }
        }

        /**
         * Call this method to disable publishing notification to user.
         * To enable showing notifications again, you need to call SetNotificationOn()
         **/
        public static void SetNotificationOff()
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("setNotificationOff", context);
            }
        }

        /**
         * Call this method to check if pushe is initialized.
         * It is needed before call to un/subscribe, and sendNotif to user methods
         **/
        public static bool IsPusheInitialized()
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            return pluginClass != null && pluginClass.CallStatic<bool>("isPusheInitialized", context);
        }

        /**
         * Call this method to get this device pusheId.
         * It is needed for call to and sendNotif to user methods
         **/
        public static string GetPusheId()
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            return pluginClass != null ? pluginClass.CallStatic<string>("getPusheId", context) : "";
        }

        /**
         * Call this method to send simple notification from client to another client.
         **/
        public static void SendSimpleNotifToUser(string userPusheId, string title, string content)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("sendSimpleNotifToUser", context, userPusheId, title, content);
            }
        }

        /**
         * Call this method to send advanced notification from client to another client.
         * You need to prepare advanced notification as a valid json string.
         **/
        public static void SendAdvancedNotifToUser(string userPusheId, string notificationJson)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("sendAdvancedNotifToUser", context, userPusheId, notificationJson);
            }
        }

        /**
         * Create a custom notification channel. This method works for android 8+
         * On lower android version, call to this method has no effect
         **/
        public static void CreateNotificationChannel(string channelId, string channelName,
            string description, int importance,
            bool enableLight, bool enableVibration,
            bool showBadge, int ledColor, long[] vibrationPattern)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("createNotificationChannel", context, channelId, channelName, description, importance, enableLight, enableVibration, showBadge, ledColor, vibrationPattern);
            }
        }

        /**
         * Remove a custom notification channel. This methos works for android 8+
         * On lower android version, call to this method has no effect
         **/
        public static void RemoveNotificationChannel(string channelId)
        {
            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var pluginClass = new AndroidJavaClass("co.ronash.pushe.Pushe");
            if (pluginClass != null)
            {
                pluginClass.CallStatic("removeNotificationChannel", context, channelId);
            }
        }


    }
}