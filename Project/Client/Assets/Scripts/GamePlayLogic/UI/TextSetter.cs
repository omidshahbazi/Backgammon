using RTLTMPro;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UI
{
    [RequireComponent(typeof(RTLTextMeshPro))]
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
            if (!Application.isPlaying || !GameManager.Instance.IsGameDataReady)
                return;
            text.text = GameDataManager.GetString(Key);
        }
    }
}