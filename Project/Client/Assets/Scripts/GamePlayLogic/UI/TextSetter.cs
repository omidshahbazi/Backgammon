using RTLTMPro;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UI
{
	public class TextSetter : MonoBehaviour
	{
		public string Key;

		private RTLTextMeshPro text;

		private void Awake()
		{
			text = GetComponent<RTLTextMeshPro>();
		}

		private void OnEnable()
		{
			text.text = GameDataManager.GetString(Key);
		}
	}
}