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

		[SerializeField]
		public Markets market;

		[SerializeField]
		public string Version;

		[SerializeField]
		public int VersionNumber;
	}
}