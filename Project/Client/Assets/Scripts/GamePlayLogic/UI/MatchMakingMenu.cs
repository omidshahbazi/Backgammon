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
        private GameObject UPanel;
        private Action OnClose = null;
        private TablesDataManager.Table SelectedTable;
        private ScheduleObj handler = null;
        private bool IsMatchFound = false;
        private bool isQuitting;

        private _2dxFX_Hologram2 hologram;
        private UITweenMover mainPanelEffect;
        private UITweenMover entraneEffect;

        protected override void Awake()
        {
            base.Awake();

        }




        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            RegisterUI("MatchMakingMenu", this);
            backButton = transform.FindDeep("BackButton", true).GetComponent<UIButton>();
            Uname = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();
            OName = transform.FindDeep("OName").GetComponent<RTLTextMeshPro>();
            OLevel = transform.FindDeep("OLevel").GetComponent<RTLTextMeshPro>();
            Enterance = transform.FindDeep("Entrance").GetComponent<RTLTextMeshPro>();
            OPanel = transform.FindDeep("OponentPanel").gameObject;
            UPanel = transform.FindDeep("YourPanel").gameObject;
            hologram = OPanel.transform.FindDeep("Avatar").GetComponent<_2dxFX_Hologram2>();
            backButton.onClick.AddListener(HideUI);
            mainPanelEffect = transform.FindDeep("MainPanel").GetComponent<UITweenMover>();
            entraneEffect = transform.FindDeep("EnterancePanel").GetComponent<UITweenMover>();
            ResetEffect();
            base.SetUIRefrences();

        }

        public override void ShowUI(params object[] Args)
        {
            if (RequestManager.Instance != null)
                RequestManager.Instance.OnMatchFound += Instance_OnMatchFound;
            ShowEffect();
            base.ShowUI(Args);

            handler = null;
            if (Args != null && Args.Length != 0)
            {
                SelectedTable = (TablesDataManager.Table)Args[0];
                if (Args.Length > 1)
                    OnClose = (Action)Args[1];
            }


            backButton.enabled = true;
            IsMatchFound = false;
            isQuitting = false;
            Uname.text = UserInfoManager.Instance.User.UserName;
            uLevel.text = string.Format(GameDataManager.GetString("Level"), UserInfoManager.Instance.User.Level.ToString());
            Enterance.text = string.Empty;
            RequestForMatch(false);
            ScheduleManager.Instance.AddSchedule(() => backButton.gameObject.SetActive(true), 0.5F);
            handler = ScheduleManager.Instance.AddSchedule(() => RequestForMatch(true), GameManager.Instance.WaitForMatch);
        }


        private void ResetEffect()
        {
            OnClose = null;
            backButton.gameObject.SetActive(false);
            OName.text = OLevel.text = string.Empty;
            hologram.enabled = true;
            entraneEffect.OnAnimateInsideOut();
        }

        private void MatchFoundEffect()
        {
            hologram.enabled = false;
            UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinAudioPath, UIEffect.Instance.CoinSprite, OPanel.transform.position, this.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinAudioPath, UIEffect.Instance.CoinSprite, UPanel.transform.position, this.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            entraneEffect.OnAnimateInsideIn(() =>
            {
                Enterance.text = " X " + SelectedTable.Enterance;
            });

        }

        public override void HideUI()
        {
            if (RequestManager.Instance != null)
                RequestManager.Instance.OnMatchFound -= Instance_OnMatchFound;

            if (!IsMatchFound)
            {
                BackToMainMenu();
            }

            CloseEffect();
        }

        private void BackToMainMenu()
        {
            isQuitting = true;
            if (handler != null)
            {
                handler.CancelSchedule();
                handler = null;
            }
            RequestManager.Instance.Network.CancelJoinToRoom();
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
            RequestManager.Instance.Network.JoinToRoom(SelectedTable.ID, WithBOT);
        }


        private void ShowEffect()
        {

            mainPanelEffect.OnAnimateInsideIn();

        }

        private void CloseEffect()
        {

            backButton.gameObject.SetActive(false);
            mainPanelEffect.OnAnimateInsideOut(() =>
            {

                ResetEffect();
                base.HideUI();
            });

        }



        private void Instance_OnMatchFound()
        {
            if (handler != null)
            {
                handler.CancelSchedule();
                handler = null;
            }
            IsMatchFound = true;
            backButton.enabled = false;
            MatchFoundEffect();
            OName.text = UserInfoManager.Instance.Opponnent.UserName;
            OLevel.text = string.Format(GameDataManager.GetString("Level"), UserInfoManager.Instance.Opponnent.Level.ToString());
            OPanel.gameObject.SetActive(true);
            GameAnalyticsManager.Instance.SendCoinSinkEvent(SelectedTable.Enterance, "Join To Room", "coin Pack :" + SelectedTable.Enterance);
            TableManager.Instance.SetSelectedTableData(SelectedTable);
            ScheduleManager.Instance.AddSchedule(HideUI, (GameManager.Instance.StartGameDelay - 2F));
        }

    }
}