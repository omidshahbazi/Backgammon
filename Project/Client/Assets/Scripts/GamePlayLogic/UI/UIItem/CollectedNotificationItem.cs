using Assets.Scripts.ClientUtilities.Extensions;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{

    public class CollectedNotificationItem : MonoBehaviour
    {
        public Vector2 AnchoredPosition
        {
            set
            {
                rect.anchoredPosition = value;
            }

            get
            {
                return rect.anchoredPosition;
            }
        }

        private RectTransform rect;
        private Image itemIconImage;
        private RTLTextMeshPro itemCount;

        private void Awake()
        {
            itemIconImage = transform.FindDeep("ItemIcon").GetComponent<Image>();
            itemCount = transform.FindDeep("ItemCount").GetComponent<RTLTextMeshPro>();
            rect = transform.GetComponent<RectTransform>();
        }

        public void SetData(Sprite Icon, string TextValue)
        {
            itemIconImage.sprite = Icon;
            itemCount.text = TextValue;
            SetImageState(true);

        }

        public void SetSize(Vector2 MinAnchor, Vector2 MaxAnchor)
        {
            rect.anchorMin= MinAnchor;
            rect.anchorMax = MaxAnchor;
        }


        public void SetImageState(bool IsEnable)
        {
            itemIconImage.enabled = IsEnable;
        }
    }
}