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
            if (!Dice.isDiceRolled)
                return;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider!=null)
            {
                Beed tempBeed = SelectedBeed;
                SelectedBeed =  hit.transform.gameObject.GetComponent<Beed>();
                if (SelectedBeed != null && SelectedBeed.BeedColor == SimulationManager.Instance.Simulator.Board.TurnColor)
                {
                    Debug.Log("Beed Selected");
                    PointVisualizerManager.Instance.HidePossibleMoves();
                    PointVisualizerManager.Instance.ShowPossibleMoves(Logic.GetPossibleBoardToBoardMoves(SimulationManager.Instance.Simulator.Board, SelectedBeed.ID));
                    tempBeed = null;
                    return;
                }

                if(tempBeed !=null && SelectedBeed == null)
                {
                    PointVisualizer point = hit.transform.gameObject.GetComponent<PointVisualizer>();
                    if (point != null)
                    {
                      PointData[]points = Logic.GetPossibleBoardToBoardMoves(SimulationManager.Instance.Simulator.Board, tempBeed.ID);
                      for(int i = 0; i<points.Length;++i)
                        {
                            if (points[i].ID == point.PointData.ID)
                                MoveTo(PointVisualizerManager.Instance.Points[tempBeed.Index] ,point);
                        }
                        
                    }
                }
            }
        }

        private void MoveTo(PointVisualizer Orgin ,PointVisualizer Destination)
        {
           GameObject go = Orgin.pointBeeds.Peek();
            if (go!=null)
            {
                go.transform.SetParent(null);
                go.transform.position= Destination.FindPosition(Destination.pointBeeds.Count);
                go.transform.SetParent(Destination.transform);
             
            }
        }
    }
}