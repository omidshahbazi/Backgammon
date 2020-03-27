using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.ResourceManager;
using ClientUtilities.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{

    public class AvatarItem : MonoBehaviour
    {
        private UIButton button;
        private Image AvatarIcon;
        private GameObject IsSelectedAvatarImage;
        private int MyAvatarIndex = 0;
        private void Awake()
        {
            button = GetComponent<UIButton>();
            AvatarIcon = transform.FindDeep("AvatarIcon").GetComponent<Image>();
            IsSelectedAvatarImage = transform.FindDeep("IsSelectedAvatarImage").gameObject;
        }

        public void SetData(int AvatarIndex, UnityAction OnClick)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            MyAvatarIndex = AvatarIndex;
            AvatarIcon.sprite = GameResourceManager.Instance.LoadAvatarSprite(MyAvatarIndex.ToString());
            IsSelectedAvatarImage.SetActive(MyAvatarIndex == UserInfoManager.Instance.User.AvatarID);
        }

        public void SetSelected(bool Value)
        {
            IsSelectedAvatarImage.SetActive(Value);
        }
    }
}