using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;
using Simulation.Logic;
using System.Collections.Generic;

namespace Assets.Scripts.GamePlayLogic
{
    public class BeedMovement : MonoBehaviour
    {
        public PointVisualizer SelectedBeed
        {
            get;
            private set;
        }

        public List<PointData> PossibleMoves = new List<PointData>();

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
            if (hit.collider != null)
            {
                PointVisualizer tempBeed = SelectedBeed;
                SelectedBeed = hit.transform.gameObject.GetComponent<PointVisualizer>();
                if (tempBeed != null && PossibleMoves.Count != 0)
                {
                    for (int i = 0; i < PossibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != PossibleMoves[i].ID)
                            continue;

                        MoveTo(tempBeed, SelectedBeed.PointData);
                        SelectedBeed = tempBeed = null;
                        PointVisualizerManager.Instance.HidePossibleMoves();
                        return;
                    }
     
                }

                if (SelectedBeed != null && SelectedBeed.PointData.CheckerCount !=0 && SelectedBeed.PointData.Color == SimulationManager.Instance.Simulator.Board.TurnColor)
                {
                    PossibleMoves.Clear();
                    tempBeed = null;
                    Debug.Log("Beed Selected");
                    PointVisualizerManager.Instance.HidePossibleMoves();
                    PossibleMoves.AddRange(Logic.GetPossibleBoardToBoardMoves(SimulationManager.Instance.Shot.BoardData, SelectedBeed.PointData.ID));
                    PointVisualizerManager.Instance.ShowPossibleMoves(PossibleMoves.ToArray());

                    return;
                }


            }
        }

        private void MoveTo(PointVisualizer Orgin, PointData Destination)
        {
            PointVisualizer finalPoint = PointVisualizerManager.Instance.FindPoint(Destination);
            if (Orgin.pointBeeds.Count != 0)
            {
                GameObject go = Orgin.pointBeeds.Peek();
                if (go != null)
                {
                    go.transform.SetParent(null);
                    go.transform.position = finalPoint.FindPosition(Destination.CheckerCount);
                    go.transform.SetParent(finalPoint.transform);
                    
                    Orgin.PointData.CheckerCount--;
                    Destination.CheckerCount++;
                    Destination.Color = SimulationManager.Instance.Simulator.Board.TurnColor;
                    finalPoint.pointBeeds.Push(Orgin.pointBeeds.Pop());
                }
            }
        }
    }
}