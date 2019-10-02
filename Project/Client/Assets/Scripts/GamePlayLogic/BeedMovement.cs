using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;
using Simulation.Logic;
using System.Collections.Generic;
using Simulation.Data.Event;

namespace Assets.Scripts.GamePlayLogic
{
    public class BeedMovement : MonoBehaviour
    {

        public PointVisualizer SelectedBeed
        {
            get;
            private set;
        }

        private List<PointData> possibleMoves = new List<PointData>();
        private List<EventBase> movesEvents = new List<EventBase>();
        private SimulationManager.SnapShot snapShot;


        private void Start()
        {
            snapShot = SimulationManager.Instance.Shot;
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
            if (!Dice.Instance.IsDiceRolled)
                return;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider != null)
            {
                PointVisualizer tempBeed = SelectedBeed;
                SelectedBeed = hit.transform.gameObject.GetComponent<PointVisualizer>();
                if (tempBeed != null && possibleMoves.Count != 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].ID)
                            continue;

                        MoveTo(tempBeed, SelectedBeed.PointData);
                        SelectedBeed = tempBeed = null;
                        PointVisualizerManager.Instance.HidePossibleMoves();
                        return;
                    }

                }

                if (SelectedBeed != null && SelectedBeed.PointData.CheckerCount != 0 && SelectedBeed.PointData.Color == snapShot.BoardData.TurnColor)
                {
                    possibleMoves.Clear();
                    tempBeed = null;
                    Debug.Log("Beed Selected");
                    PointVisualizerManager.Instance.HidePossibleMoves();
                    possibleMoves.AddRange(Logic.GetPossibleBoardToBoardMoves(snapShot.BoardData, SelectedBeed.PointData.ID));
                    PointVisualizerManager.Instance.ShowPossibleMoves(possibleMoves.ToArray());

                    return;
                }


            }
        }

        private void DiceUsed()
        {

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
                    Destination.Color = snapShot.BoardData.TurnColor;
                    finalPoint.pointBeeds.Push(Orgin.pointBeeds.Pop());

                    ConsumeDice(Orgin.Index, finalPoint.Index);
                    //Move Events Should Add to This List
                    //movesEvents.Add()????
                }
            }
        }

        private void ConsumeDice(int OrginIndex, int DestinationIndex)
        {
            int moveCount = Mathf.Abs(OrginIndex - DestinationIndex);
            DiceData diceData = snapShot.BoardData.TurnDice;
            if (diceData.Dice1 == 0 && diceData.Dice2 == 0)
                return;

            int multiply = Dice.Instance.IsPair ? 4 : 1;

            if (multiply * (diceData.Dice1 + diceData.Dice2) == moveCount)
                diceData.Dice1 = diceData.Dice2 = 0;
            else if (multiply*diceData.Dice1 == moveCount)
                diceData.Dice1 = 0;
            else
                diceData.Dice2 = 0;


        }
    }
}