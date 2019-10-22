using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;


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