using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using DG.Tweening;

namespace Assets.Scripts.GamePlayLogic.UI
{

    public class InitialMenu : UIBase
    {
        protected override void Awake()
        {
            base.Awake();

            RegisterUI("InitialMenu", this);
        }

    }
}