using Assets.Scripts.GamePlayLogic.RequestManagers;
using ClientUtilities.Singleton;
using System;
using UnityEngine;

namespace Assets.Scripts.PushNotification
{
	public class PushNotificationManager : MonoBehaviorSingleton<PushNotificationManager>
	{
		public INotificationServices Service
		{
			get;
			private set;
		}

		public bool IsServiceInitialized
		{
			get;
			private set;
        }
      
		public const string PusheAPPID = "PUSHE_26038114400";


#if !UNITY_EDITOR
		private void Awake()
		{
		
#if UNITY_ANDROID
			Debug.Log("[PushNotificationManager] Pushe Initialized.");
			Service = new Pushe();
			Service.OnInitializeCompleted(OnCompleted);
			Service.Initialize();

#endif
        }

        private void OnCompleted(string userID)
		{
			Debug.Log("[PushNotificationManager] Services OnCompleted TRUE UserID:"+ userID);
			IsServiceInitialized = true;

            RequestManager.Instance.Network.SetPushID(userID);
		}
#endif
    }
}
