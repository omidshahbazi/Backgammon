using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;

namespace Assets.Scripts.GamePlayLogic
{
	public class BeedSelector : MonoBehaviour
    {
        

        private void OnEnable()
        {
            Tap.OnTapBegin += OnTap;
        }

        private void OnDisable()
        {
            Tap.OnTapBegin -= OnTap;
        }


        private void OnTap(Vector2 Position)
        {
    
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider!=null)
            {
                if (hit.transform.gameObject.GetComponent<Beed>() != null)
                {
                    Debug.Log("Beed Selected");

                }
            }
        }
    }
}