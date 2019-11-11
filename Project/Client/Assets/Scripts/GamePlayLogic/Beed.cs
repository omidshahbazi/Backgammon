using Assets.Scripts.ClientUtilities.Extensions;
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

        public GameObject GlowObject
        {
            get;
            private set;
        }

        private void Awake()
        {
            GlowObject = transform.FindDeep("Glow",true).gameObject;
            Trail = GetComponent<TrailRenderer>();
            Trail.enabled = false;
        }
    }

}