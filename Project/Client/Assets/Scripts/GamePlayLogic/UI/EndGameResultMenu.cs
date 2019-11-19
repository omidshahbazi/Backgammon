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
using Simulation.Data.Game;
using Assets.Scripts.GamePlayLogic.UI.ItemPool;
using ClientUtilities.AudioMangaer;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class EndGameResultMenu : UIBase
    {
        // private UIButton backButton;
        private RTLTextMeshPro Uname;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro OName;
        private RTLTextMeshPro OLevel;
        private GameObject uPanel;
        private GameObject OPanel;
        private CanvasGroup oPanelCG;
        private CanvasGroup uPanelCG;
        private UITweenMover mainPanelEffect;
        private UITweenMover opanelCrownEffect;
        private UITweenMover upanelCrownEffect;
        private PlayerColors winnerColor;
        private int bet = 100;

        private Audio winSound = null;
        private Audio loseSound = null;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            RegisterUI("EndGameResultMenu", this);
            OPanel = transform.FindDeep("OponentPanel").gameObject;
            uPanel = transform.FindDeep("YourPanel").gameObject;
            mainPanelEffect = transform.FindDeep("MainPanel").GetComponent<UITweenMover>();
            //backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            Uname = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();
            OName = OPanel.transform.FindDeep("OName").GetComponent<RTLTextMeshPro>();
            OLevel = OPanel.transform.FindDeep("OLevel").GetComponent<RTLTextMeshPro>();
            opanelCrownEffect = OPanel.transform.FindDeep("CrownPanel").GetComponent<UITweenMover>();
            upanelCrownEffect = uPanel.transform.FindDeep("CrownPanel").GetComponent<UITweenMover>();

            oPanelCG = OPanel.GetComponent<CanvasGroup>();
            uPanelCG = uPanel.GetComponent<CanvasGroup>();
            ResetUI();
            base.SetUIRefrences();
        }

        //protected override void Update()
        //{
        //    base.Update();

        //    if (Input.GetKeyDown(KeyCode.B))
        //        ShowEffect();

        //    if (Input.GetKeyDown(KeyCode.C))
        //        ResetUI();
        //}


        public override void ShowUI(params object[] Args)
        {
            if (Args != null && Args.Length != 0)
            {
                winnerColor = (PlayerColors)Args[0];
                bet = (ushort)Args[1];

            }

            if (winSound == null || loseSound == null)
            {
                winSound = AudioManager.Instance.Load("Win", AudioManager.SoundTypes.Effect);
                loseSound = AudioManager.Instance.Load("Lose", AudioManager.SoundTypes.Effect);
                winSound.AutoUnload = loseSound.AutoUnload = false;
                winSound.Volume = loseSound.Volume = 100;
            }

            Uname.text = UserInfoManager.Instance.User.UserName;
            uLevel.text = GameDataManager.GetString("Level") + UserInfoManager.Instance.User.Level.ToString();
            OName.text = UserInfoManager.Instance.Opponnent.UserName;
            OLevel.text = GameDataManager.GetString("Level")+ UserInfoManager.Instance.Opponnent.Level.ToString();
            ShowEffect();
            base.ShowUI(Args);



        }

        private void ResetUI()
        {
            oPanelCG.alpha = uPanelCG.alpha = 0;
            mainPanelEffect.OnAnimateInsideOut();
            opanelCrownEffect.OnAnimateInsideOut();
            upanelCrownEffect.OnAnimateInsideOut();
        }

        private void ShowEffect()
        {
            mainPanelEffect.OnAnimateInsideIn(() => LeanTween.value(0, 1, 0.5F).setOnUpdate(OnUpdate).setOnComplete(OnAlphaEffectComplete));
        }

        private void OnAlphaEffectComplete()
        {
            if (winnerColor == SimulationManager.Instance.YourColor)
            {
                winSound.Stop();
                winSound.Play();
                upanelCrownEffect.OnAnimateInsideIn();
                UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinAudioPath, UIEffect.Instance.CoinSprite, this.transform.position, upanelCrownEffect.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            }
            else
            {
                loseSound.Stop();
                loseSound.Play();
                opanelCrownEffect.OnAnimateInsideIn();
                UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinAudioPath, UIEffect.Instance.CoinSprite, this.transform.position, opanelCrownEffect.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            }

            PopupTextMenu.Instance.ShowPopUpText(bet.ToString());
            ScheduleManager.Instance.AddSchedule(() =>
            {
                ShowMainMenu();
            }, 4F);
        }

        private void ShowMainMenu()
        {
            CloseEffect();
        }

        private void OnUpdate(float Val)
        {
            oPanelCG.alpha = uPanelCG.alpha = Val;
        }

        private void CloseEffect()
        {
            mainPanelEffect.OnAnimateInsideOut(() =>
            {
                ResetUI();
                HideUI();
                UIManager.Instance.ShowUI("InitialMenu");
            });
        }
    }

}