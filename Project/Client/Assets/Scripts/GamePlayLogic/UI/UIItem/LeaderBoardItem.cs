using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.LeaderBoard;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Shop;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.IAP;
using ClientUtilities.ResourceManager;
using ClientUtilities.UI;
using OnePF;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{


    public class LeaderBoardItem : MonoBehaviour
    {
        private GameObject mainPanel;
        private static bool ActiveBackGround = false;
        private static int rank = 1;
        private int minSizeR = int.MaxValue;
        private int minSizeU = int.MaxValue;
        private int minSizeC = int.MaxValue;
        public User info;
        private UIButton button;
        private Image backGroundImage;
        private Image avatarImage;
        private RTLTextMeshPro rankText;
        private RTLTextMeshPro userNameText;
        private RTLTextMeshPro CoinText;

        // Start is called before the first frame update
        private void Awake()
        {
            button = GetComponent<UIButton>();
            backGroundImage = button.image.GetComponent<Image>();
            rankText = transform.FindDeep("RankText").GetComponent<RTLTextMeshPro>();
            userNameText = transform.FindDeep("UserNameText").GetComponent<RTLTextMeshPro>();
            CoinText = transform.FindDeep("CoinText").GetComponent<RTLTextMeshPro>();
            avatarImage = transform.FindDeep("Avatar").GetComponent<Image>();
            button.onClick.AddListener(ShowProfileMenu);
        }

        private void OnDisable()
        {

        }

        public void ResetInitialValues()
        {
            minSizeR = int.MaxValue;
            minSizeU = int.MaxValue;
            minSizeC = int.MaxValue;
            rankText.enableAutoSizing = true;
            CoinText.enableAutoSizing = true;
            userNameText.enableAutoSizing = true;
            ActiveBackGround = false;
            rank = 1;
        }

        public void SetData(User User, GameObject UIPanel)
        {
            info = User;
            ActiveBackGround = !ActiveBackGround;
            if (ActiveBackGround)
                backGroundImage.color = new Color(backGroundImage.color.r, backGroundImage.color.g, backGroundImage.color.b, 255);
            else
                backGroundImage.color = new Color(backGroundImage.color.r, backGroundImage.color.g, backGroundImage.color.b, 0);
            rankText.text = rank.ToString();
            userNameText.text = info.UserInfo.UserName;
            avatarImage.sprite = GameResourceManager.Instance.LoadAvatarSprite(info.UserInfo.AvatarID.ToString());
            CoinText.text = info.Coin.ToString();
            rank++;

            mainPanel = UIPanel;
            if (rankText.fontSize < minSizeR)
                minSizeR = (int)rankText.fontSize;
            if (CoinText.fontSize < minSizeC)
                minSizeC = (int)CoinText.fontSize;
            if (userNameText.fontSize < minSizeU)
                minSizeU = (int)userNameText.fontSize;
        }


        public void SetTextSize()
        {
            rankText.enableAutoSizing = false;
            rankText.fontSize = minSizeR;
            CoinText.enableAutoSizing = false;
            CoinText.fontSize = minSizeC;
            userNameText.enableAutoSizing = false;
            userNameText.fontSize = minSizeU;
        }

        private void ShowProfileMenu()
        {
            UserInfoManager.Instance.GetUserInfo(info.UserInfo.ID, (Info) =>
            {
                object userInfo = (UserInfo)Info;
                if (userInfo == null)
                    return;

                object Close = (Action)(() => { mainPanel.gameObject.SetActive(true); });
                mainPanel.gameObject.SetActive(false);
                UIManager.Instance.ShowUI("ProfileMenu", userInfo, Close);
            });


        }
    }
}