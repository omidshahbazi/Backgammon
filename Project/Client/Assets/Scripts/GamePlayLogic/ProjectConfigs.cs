using Networking.Common;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
	public class ProjectConfigs : MonoBehaviour
	{
		public static ProjectConfigs Instance
		{
			get;
			private set;
		}

		private void Awake()
		{
			Instance = this;
		}

	
		public Markets market;

	
		public string Version;

		
		public int VersionNumber;

        public GameAnalyticsManager.Currency CurrencyType;
       
    }
}