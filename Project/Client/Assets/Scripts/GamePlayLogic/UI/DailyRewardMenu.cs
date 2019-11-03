
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic.UI.UIItems;
using RTLTMPro;
using I2.MiniGames;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class DailyRewardMenu : UIBase
    {
        public static DailyRewardMenu Instance = null;
        public bool IsRewardShowed = false;
        private PrizeWheel prizeWheel;
        private UIButton backButton;
        private UITweenMover MainPanel;
        private UITweenMover contentPanel;
        private RTLTextMeshPro text;
        private Action OnClose = null;
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();

            MainPanel = GetComponent<UITweenMover>();
            contentPanel = transform.FindDeep("ContentReward").GetComponent<UITweenMover>();
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideUI);
            prizeWheel = transform.FindDeep("DailyRewardWheel").GetComponent<PrizeWheel>();
            text = transform.FindDeep("ContentRewardTxt").GetComponent<RTLTextMeshPro>();
            MiniGame_Controller.OnGameOver += MiniGame_Controller_OnGameOver;
            Instance = this;

        }

        private void MiniGame_Controller_OnGameOver()
        {
            contentPanel.OnAnimateInsideIn();
            //backButton.enabled = true;
            text.text = GameManager.Instance.DailyRewardData.Reward.Coin.ToString();
            ScheduleManager.Instance.AddSchedule(HideUI, 2);
            GameManager.Instance.UpdateDailyReward(()=> { });
            IsRewardShowed = true;
        }


        //protected override void Update()
        //{
        //    base.Update();

        //    if(Input.GetKeyDown(KeyCode.B))
        //    {
        //        MainPanel.OnAnimateInsideIn();
        //        contentPanel.OnAnimateInsideIn();
        //    }

        //    if (Input.GetKeyDown(KeyCode.C))
        //    {
        //        MainPanel.OnAnimateInsideOut();
        //        contentPanel.OnAnimateInsideOut();

        //    }
        //}


        public override void ShowUI(params object[] Args)
        {

            base.ShowUI(Args);
            base.HideUI();
            if (Args != null && Args.Length != 0)
            {         
                OnClose = (Action)Args[0];
            }

            GameManager.Instance.UpdateDailyReward(() =>
            {
                if (IsRewardShowed = GameManager.Instance.DailyRewardData.IsClaimed)
                    SimpleHide();
                else
                {
                    if (GameManager.Instance.DailyRewardData.Reward == null)
                    {
                        IsRewardShowed = true;
                        SimpleHide();
                        return;
                    }
                    prizeWheel.UpdateReward((int)GameManager.Instance.DailyRewardData.Reward.Coin);
                    MainPanel.OnAnimateInsideIn();
                    backButton.enabled = false;


                }
            });
            //MainPanel.OnAnimateInsideIn();
            //backButton.enabled = false;


        }

        private void SimpleHide()
        {
            base.HideUI();
            OnClose?.Invoke();
        }


        public override void HideUI()
        {
            MainPanel.OnAnimateInsideOut(() =>
            {
                base.HideUI();
                OnClose?.Invoke();
            });

        }




    }
}