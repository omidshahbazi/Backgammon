using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using DG.Tweening;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class SplashScreen : UIBase
    {
        private _2dxFX_SkyCloud cloud;
        private _2dxFX_Smoke smoke;

        protected override void Awake()
        {
            base.Awake();

            RegisterUI("SplashScreen", this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RequestManager.Instance.OnAuthenticated += Instance_OnAuthenticated;
            if (!RequestManager.Instance.IsAuthenticated)
                ScheduleManager.Instance.ScheduleAction(RequestManager.Instance.InitilizeNetwork, 4F);
        }



        private void Instance_OnAuthenticated(AuthenticateResults Result, int ID, string Username)
        {
            switch (Result)
            {
                case AuthenticateResults.Passed:
                    cloud.enabled = false;
                    smoke.enabled = true;
                    DOTween.To(() => smoke._Value2, x => smoke._Value2 = x, 1, 3).OnComplete(() =>
                    {
                        //To DO
                        RequestManager.Instance.Network.JoinToRoom(500, true);
                        this.gameObject.SetActive(false);

                    });
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