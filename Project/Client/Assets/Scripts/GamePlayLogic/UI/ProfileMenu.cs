using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;

using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using RTLTMPro;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using TMPro;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class ProfileMenu : UIBase
    {
        private UserInfo userInfo;
        private Action OnClose = null;
        private UIButton backButton;
        private UIButton editButton;
        private UIButton applyButton;
        private RTLTextMeshPro Uname;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro gtext;
        private RTLTextMeshPro wTtext;
        private RTLTextMeshPro Wtext;
        private RTLTextMeshPro ltext;
        private RTLTextMeshPro wbtext;
        private RTLTextMeshPro lbtext;
        private GameObject setProfileDataPanel;
        private TMP_InputField inputFiled;
        private RTLTextMeshPro placeHolderText;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            base.SetUIRefrences();

            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            editButton = transform.FindDeep("EditButton").GetComponent<UIButton>();
            Uname = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();

            gtext = transform.FindDeep("GCountText").GetComponent<RTLTextMeshPro>();
            wTtext = transform.FindDeep("WCountText").GetComponent<RTLTextMeshPro>();
            Wtext = transform.FindDeep("WGCountText").GetComponent<RTLTextMeshPro>();
            ltext = transform.FindDeep("LCountText").GetComponent<RTLTextMeshPro>();
            wbtext = transform.FindDeep("WBGCountText").GetComponent<RTLTextMeshPro>();
            lbtext = transform.FindDeep("LBCountText").GetComponent<RTLTextMeshPro>();
            setProfileDataPanel = transform.FindDeep("SetProfilePanel").gameObject;
            inputFiled = transform.FindDeep("InputField - RTLTMP", true).GetComponent<TMP_InputField>();
            placeHolderText = inputFiled.placeholder.GetComponent<RTLTextMeshPro>();
            applyButton = transform.FindDeep("ApplyButton",true).GetComponent<UIButton>();
            backButton.onClick.AddListener(HideUI);
            editButton.onClick.AddListener(ShowProfileData);
            applyButton.onClick.AddListener(SubmitData);
            inputFiled.onEndEdit.AddListener(OnEndEdit);

        }

        private void SubmitData()
        {
            applyButton.enabled = false;
            if (placeHolderText.text != UserInfoManager.Instance.User.UserName)
            {
                Uname.text = placeHolderText.text;
                RequestManager.Instance.Network.SetUserInfo(placeHolderText.text, 1);
                UserInfoManager.Instance.UpdateUserInfo();

            }

            setProfileDataPanel.gameObject.SetActive(false);
        }

        private void OnEndEdit(string arg0)
        {
            inputFiled.text = placeHolderText.text = arg0;
        }

        private void ShowProfileData()
        {
            applyButton.enabled = true;
            inputFiled.characterLimit = 20;
            inputFiled.text = placeHolderText.text = Uname.text = userInfo.UserName;
            setProfileDataPanel.gameObject.SetActive(true);
        }

        public override void ShowUI(params object[] Args)
        {

            if (Args != null && Args.Length != 0)
            {
                userInfo = (UserInfo)Args[0];
                if (Args.Length > 1)
                    OnClose = (Action)Args[1];
            }

            base.ShowUI(Args);

            editButton.gameObject.SetActive(userInfo.ID == UserInfoManager.Instance.User.ID);
            Uname.text = userInfo.UserName;
            uLevel.text = "سطح" + userInfo.Level;
            gtext.text = userInfo.GameCount.ToString();
            wTtext.text = userInfo.WinCount.ToString();
            Wtext.text = userInfo.WinGammonCount.ToString();
            ltext.text = userInfo.LoseGammonCount.ToString();
            wbtext.text = userInfo.WinBackGammonCount.ToString();
            lbtext.text = userInfo.LoseBackGammonCount.ToString();

        }



        public override void HideUI()
        {
            base.HideUI();
            OnClose?.Invoke();
        }

    }
}