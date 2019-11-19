using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using System;
using RTLTMPro;
using ClientUtilities.ResourceManager;
using ClientUtilities.AudioMangaer;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class SplashScreen : UIBase
    {
        public UITweenMover logo;
        public UITweenMover dice1;
        public UITweenMover dice2;
        public RTLTextMeshPro Status;
        public RTLTextMeshPro versionText;

        private bool isHiding = false;
        private _2dxFX_SkyCloud cloud;
        private _2dxFX_Smoke smoke;
        private int dotIndex;

        protected override void Awake()
        {
            //logo.OnAnimateInsideOut();
            //dice1.OnAnimateInsideOut();
            //dice2.OnAnimateInsideOut();
            base.Awake();
            AudioManager instance = AudioManager.Instance;
            Instantiate(GameResourceManager.Instance.LoadPrefab("ProjectConfigs"));
            versionText.text = "V" + ProjectConfigs.Instance.Version;
            RegisterUI("SplashScreen", this);

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RequestManager.Instance.OnAuthenticated += Instance_OnAuthenticated;
            //RequestManager.Instance.OnInitialData += Instance_OnInitialData;

            AnimateDots();
            ScheduleManager.Instance.AddSchedule(() => ShowEffect(), 2F);
        }



        private void Instance_OnAuthenticated(AuthenticateResults Result, int ID)
        {
            switch (Result)
            {
                case AuthenticateResults.Passed:

                    Status.text = "در حال دريافت اطلاعات ";

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

            isHiding = true;
            Status.text = " لطفا صبرکنيد ";
            ScheduleManager.Instance.AddSchedule(() =>
            {

                dice2.OnAnimateInsideOut();
                dice1.OnAnimateInsideOut(() =>
                {
                    versionText.gameObject.SetActive(false);
                    Status.gameObject.SetActive(false);
                    logo.OnAnimateInsideOut(() =>
                    {
                        cloud.enabled = false;
                        smoke.enabled = true;

                        LeanTween.value(this.gameObject, smoke._Value2, 1, 3).setOnUpdate(OnUpdate).setOnComplete(() =>
                        {
                            // RequestManager.Instance.Network.JoinToRoom(500, true);
                            this.gameObject.SetActive(false);
                            UIManager.Instance.ShowUI("InitialMenu");

                        });
                    });

                });
            }, 2);

        }


        protected override void Update()
        {
            //base.Update();
            //if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.B))
            //{
            //    dice1.OnAnimateInsideOut();
            //    dice2.OnAnimateInsideOut();
            //    logo.OnAnimateInsideOut();

            //}
            //if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.C))
            //{
            //    ShowEffect();
            //}

            if (!GameManager.Instance.IsGameDataReady)
                return;
            HideSplashScreen();
        }


        private void AnimateDots()
        {
            //if (LeanTween.isTweening(versionText.gameObject))
            //    return;

            if (dotIndex > 3)
                dotIndex = 1;

            switch (dotIndex)
            {
                case 1:
                    Status.text = "در حال اتصال" + ".";
                    break;
                case 2:
                    Status.text = "در حال اتصال" + "..";
                    break;
                case 3:
                    Status.text = "در حال اتصال" + "...";
                    break;
                default:
                    break;
            }

            dotIndex++;

            LeanTween.delayedCall(versionText.gameObject, 0.3F, () =>
             {
                 if (RequestManager.Instance.IsAuthenticated)
                     return;
                 AnimateDots();
             });
        }

        private void ShowEffect()
        {
            logo.OnAnimateInsideIn(() =>
            {
                PlayAudioEffect();
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

        private void PlayAudioEffect()
        {
            Audio click = AudioManager.Instance.Load("Begining", AudioManager.SoundTypes.Effect);
            click.Volume = 100;
            //click.Stop();
            click.AutoUnload = true;

            click.Play();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            cloud = GetComponent<_2dxFX_SkyCloud>();
            smoke = GetComponent<_2dxFX_Smoke>();
            cloud._AutoScrollX = true;
            smoke.enabled = false;
            base.SetUIRefrences();
        }
    }
}