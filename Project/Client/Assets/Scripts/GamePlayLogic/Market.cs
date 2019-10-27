
using ClientUtilities.Singleton;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using UnityEngine;


namespace Assets.Scripts.GamePlayLogic
{

    public class Market : MonoBehaviorSingleton<Market>
    {    
        [SerializeField]
        public Markets market;
    }
}