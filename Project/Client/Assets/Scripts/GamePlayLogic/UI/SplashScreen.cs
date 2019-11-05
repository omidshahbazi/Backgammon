using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using System;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class SplashScreen : UIBase
    {
        public UITweenMover logo;
        public UITweenMover dice1;
        public UITweenMover dice2;

        private bool isHiding = false;
        private _2dxFX_SkyCloud cloud;
        private _2dxFX_Smoke smoke;

        protected override void Awake()
        {
            //logo.OnAnimateInsideOut();
            //dice1.OnAnimateInsideOut();
            //dice2.OnAnimateInsideOut();
            base.Awake();

            RegisterUI("SplashScreen", this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RequestManager.Instance.OnAuthenticated += Instance_OnAuthenticated;
            //RequestManager.Instance.OnInitialData += Instance_OnInitialData;


            ScheduleManager.Instance.AddSchedule(() => ShowEffect(), 2F);
        }



        private void Instance_OnAuthenticated(AuthenticateResults Result, int ID)
        {
            switch (Result)
            {
                case AuthenticateResults.Passed:



                    break;
                case AuthenticateResults.Banned:
                    // To Do Show Proper Message Window
                    break;
                case AuthenticateResults.Deleted:
                    // To Do Show Proper Message Window
                    break;
                default:
                    break;
            }
        }
        private void HideSplashScreen()
        {
            if (isHiding)
                return;
            cloud.enabled = false;
            isHiding = smoke.enabled = true;
            logo.OnAnimateInsideOut();
            dice1.OnAnimateInsideOut();
            dice2.OnAnimateInsideOut();
            UIManager.Instance.ShowUI("InitialMenu");
            LeanTween.value(this.gameObject, smoke._Value2, 1, 3).setOnUpdate(OnUpdate).setOnComplete(() =>
            {
                // RequestManager.Instance.Network.JoinToRoom(500, true);
                this.gameObject.SetActive(false);
            });
        }


        protected override void Update()
        {
            //base.Update();
            //if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.B))
            //{
            //    logo.OnAnimateInsideOut();
            //    dice1.OnAnimateInsideOut();
            //    dice2.OnAnimateInsideOut();
            //}
            //if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.C))
            //{
            //    ShowEffect();
            //}

            if (!GameManager.Instance.IsGameDataReady)
                return;
            HideSplashScreen();
        }

        private void ShowEffect()
        {
            logo.OnAnimateInsideIn(() =>
            {
                dice1.OnAnimateInsideIn();
                dice2.OnAnimateInsideIn();
                if (!RequestManager.Instance.IsAuthenticated)
                    RequestManager.Instance.InitilizeNetwork();
            });
        }

        private void OnUpdate(float Value)
        {
            smoke._Value2 = Value;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (RequestManager.Instance != null)
                RequestManager.Instance.OnAuthenticated -= Instance_OnAuthenticated;
        }

        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();
            cloud = GetComponent<_2dxFX_SkyCloud>();
            smoke = GetComponent<_2dxFX_Smoke>();
            cloud._AutoScrollX = true;
            smoke.enabled = false;
        }
    }
}