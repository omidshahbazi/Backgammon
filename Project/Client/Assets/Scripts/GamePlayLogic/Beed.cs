using Simulation.Common;
using Simulation.Data.Game;
using UnityEngine;


namespace Assets.Scripts.GamePlayLogic
{
 
    public class Beed : MonoBehaviour
    {

        [SerializeField]
        public PlayerColors BeedColor;

        public TrailRenderer Trail
        {
            get;
            private set;
        }

        public Identifier ID
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        private void Awake()
        {
            Trail = GetComponent<TrailRenderer>();
            Trail.enabled = false;
        }
    }

}