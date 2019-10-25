using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using ClientUtilities.UI;
using RTLTMPro;
using System;
using Assets.Scripts.GamePlayLogic.UserData;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class MatchMakingMenu : UIBase
    {
        private UIButton backButton;
        private RTLTextMeshPro Uname;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro OName;
        private RTLTextMeshPro OLevel;
        private RTLTextMeshPro Enterance;
        private GameObject OPanel;
        private Action OnClose = null;
        private ushort entranceValue;
        private ScheduleObj handler = null;
        private bool IsMatchFound = false;
        private bool isQuitting;


        protected override void Awake()
        {
            base.Awake();

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (RequestManager.Instance != null)
                RequestManager.Instance.OnMatchFound += Instance_OnMatchFound;
        }



        protected override void OnDisable()
        {
            base.OnDisable();
            if (RequestManager.Instance != null)
                RequestManager.Instance.OnMatchFound -= Instance_OnMatchFound;

        }



        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();
            RegisterUI("MatchMakingMenu", this);
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            Uname = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();
            OName = transform.FindDeep("OName").GetComponent<RTLTextMeshPro>();
            OLevel = transform.FindDeep("OLevel").GetComponent<RTLTextMeshPro>();
            Enterance = transform.FindDeep("Entrance").GetComponent<RTLTextMeshPro>();
            OPanel = transform.FindDeep("OponentPanel").gameObject;
            backButton.onClick.AddListener(HideUI);

        }

        public override void ShowUI(params object[] Args)
        {
            base.ShowUI(Args);

            if (Args != null && Args.Length != 0)
            {
                entranceValue = (ushort)Args[0];
                OnClose = (Action)Args[1];
            }

            
            backButton.enabled = true;
            IsMatchFound = false;
            isQuitting = false;
            Uname.text = UserInfoManager.Instance.User.UserName;
            uLevel.text = "سطح" + UserInfoManager.Instance.User.Level.ToString();
            Enterance.text = "x" + entranceValue;
            RequestForMatch(false);
            OPanel.gameObject.SetActive(false);

            handler = ScheduleManager.Instance.AddSchedule(() => RequestForMatch(true), GameManager.Instance.WaitForMatch);
        }

        public override void HideUI()
        {

            isQuitting = true;
            if (handler != null)
            {
                handler.CancelSchedule();
                handler = null;
            }
            RequestManager.Instance.Network.CancelJoinToRoom();
            base.HideUI();
            OnClose?.Invoke();
        }


        private void RequestForMatch(bool WithBOT = false)
        {
            if (isQuitting || IsMatchFound)
                return;

            if (WithBOT)
            {
                backButton.enabled = false;
                RequestManager.Instance.Network.CancelJoinToRoom();
            }
            RequestManager.Instance.Network.JoinToRoom(entranceValue, WithBOT);
        }



        private void Instance_OnMatchFound()
        {
            backButton.enabled = false;
            OName.text = UserInfoManager.Instance.Opponnent.UserName;
            OLevel.text = "سطح" + UserInfoManager.Instance.Opponnent.Level.ToString();
            OPanel.gameObject.SetActive(true);
        }

    }
}