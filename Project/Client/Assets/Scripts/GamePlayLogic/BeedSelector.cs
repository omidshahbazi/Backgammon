using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;
using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
	public class BeedSelector : MonoBehaviour
    {
        public Beed SelectedBeed
        {
            get;
            private set;
        }

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
               
                SelectedBeed =  hit.transform.gameObject.GetComponent<Beed>();
                if (Dice.isDiceRolled && SelectedBeed != null && SelectedBeed.BeedColor == SimulationManager.Instance.Simulator.Board.TurnColor)
                {
                    Debug.Log("Beed Selected");
                    PointVisualizerManager.Instance.HidePossibleMoves();
                    PointVisualizerManager.Instance.ShowPossibleMoves(Logic.GetPossibleBoardToBoardMoves(SimulationManager.Instance.Simulator.Board, SelectedBeed.ID));
                }
            }
        }
    }
}