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

#if !BACKGAMOON_NEW_GAME_PLAY_VERSION
        private BoxCollider2D collider2D;
#endif

        private void Awake()
        {
            GlowObject = transform.FindDeep("Glow",true).gameObject;
            Trail = GetComponent<TrailRenderer>();
            Trail.enabled = false;

#if !BACKGAMOON_NEW_GAME_PLAY_VERSION
            collider2D = GlowObject.GetComponent<BoxCollider2D>();
            collider2D.enabled = false;
#endif
        }

        private void OnDisable()
        {
            if (GlowObject != null)
                GlowObject.SetActive(false);
        }
    }

}