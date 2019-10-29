

namespace Assets.Scripts.PushNotification
{
	public delegate void OnNotificationReceivedHandler(NotificationData data);
	public delegate void OnInitializeCompletedHandler(string userID);

	public enum PushNotificationAPIs
	{
		Pushe = 1
	}

	public interface INotificationServices
	{
		PushNotificationAPIs API
		{
			get;
		}

		void Initialize(string ID = default(string));
		void OnInitializeCompleted(OnInitializeCompletedHandler OnComplete);
		void AddNotificationReceivedHandler(OnNotificationReceivedHandler OnRecieved);
		void RemoveNotificationReceivedHandler(OnNotificationReceivedHandler OnRecieved);
	}
}
