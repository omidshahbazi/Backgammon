
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

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;
         

            MainPanel = GetComponent<UITweenMover>();
            contentPanel = transform.FindDeep("ContentReward").GetComponent<UITweenMover>();
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideUI);
            prizeWheel = transform.FindDeep("DailyRewardWheel").GetComponent<PrizeWheel>();
            text = transform.FindDeep("ContentRewardTxt").GetComponent<RTLTextMeshPro>();
            MiniGame_Reward.OnGameOver += MiniGame_Controller_OnGameOver;
            Instance = this;
            base.SetUIRefrences();
        }

        private void MiniGame_Controller_OnGameOver()
        {
            UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinSprite, this.transform.position, InitialMenu.Instance.userCoinPanel.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            PopupTextMenu.Instance.ShowPopUpText("+" + GameManager.Instance.DailyRewardInfo.Reward.Coin.ToString());
            contentPanel.OnAnimateInsideIn();
            //backButton.enabled = true;
            text.text = GameManager.Instance.DailyRewardInfo.Reward.Coin.ToString();
         
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
            contentPanel.OnAnimateInsideOut();
            
           
            if (Args != null && Args.Length != 0)
            {         
                OnClose = (Action)Args[0];
            }else
            {
                HideUI();
            }

            GameManager.Instance.UpdateDailyReward(() =>
            {
                if (IsRewardShowed = !GameManager.Instance.DailyRewardInfo.IsClaimed)
                    SimpleHide();
                else
                {
                    if (GameManager.Instance.DailyRewardInfo.Reward == null)
                    {
                        IsRewardShowed = true;
                        SimpleHide();
                        return;
                    }
                    prizeWheel.UpdateReward((int)GameManager.Instance.DailyRewardInfo.Reward.Coin);


                    base.ShowUI(Args);
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
            contentPanel.OnAnimateInsideOut();

            MainPanel.OnAnimateInsideOut(() =>
            {
                base.HideUI();
                OnClose?.Invoke();
            });

        }
    }
}