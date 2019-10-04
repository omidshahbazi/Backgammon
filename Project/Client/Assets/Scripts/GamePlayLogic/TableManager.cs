using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;
using Simulation.Logic;
using System.Collections.Generic;
using Simulation.Data.Event;
using Simulation.Common;
using Assets.Scripts.GamePlayLogic.UI;
using System;
using ClientUtilities.Singleton;

namespace Assets.Scripts.GamePlayLogic
{
    public class TableManager : MonoBehaviorSingleton<TableManager>
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
            InGameUI.OnChangeTurnEventClick += OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick += OnUndoEventClick;
        }

      
        private void OnDisable()
        {
            Tap.OnTapBegin -= OnTap;
            InGameUI.OnChangeTurnEventClick -= OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick -= OnUndoEventClick;
        }

        private void OnUndoEventClick()
        {
            movesEvents.Clear();
            SimulationManager.Instance.UndoActions();
        }


        private void OnChangeTurnEventClick()
        {
            if (snapShot.BoardData.TurnDice.Dice1 != 0 && snapShot.BoardData.TurnDice.Dice2 != 0)
                return;

            for (int i = 0; i < movesEvents.Count; ++i)
            {
                EventBase ev = movesEvents[i];              
                SimulationManager.Instance.Simulator.SendEvent(ev);
            }

            movesEvents.Clear();
            SimulationManager.Instance.Simulator.SendEvent(new FinishTurnEvent(snapShot.BoardData.TurnColor));
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

                if (tempBeed != null && tempBeed.PointData.ID != SelectedBeed.PointData.ID && possibleMoves.Count != 0)
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

            PointVisualizerManager.Instance.HidePossibleMoves();
            SelectedBeed = null;
        }



        private void MoveTo(PointVisualizer Orgin = null, PointData Destination = null)
        {
            PointVisualizer finalPoint = null;
            EventBase.Types type = EventBase.Types.FinishTurn;
            if (Destination != null)
                finalPoint = PointVisualizerManager.Instance.FindPoint(Destination);
            if (Orgin != null && Orgin.pointBeeds.Count != 0)
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
                    type = EventBase.Types.BoardToBoardMove;
                    //Move Events Should Add to This List
                    //movesEvents.Add()????
                }
            }



            switch (type)
            {
                case EventBase.Types.BoardToBoardMove:
                    BoardToBoardMoveEvent(Orgin.PointData.ID, finalPoint.PointData.ID);
                    break;
                case EventBase.Types.BearOff:
                    BearOff(finalPoint.PointData.ID);
                    break;
                case EventBase.Types.BearedOff:
                    BearedOff(Orgin.PointData.ID);
                    break;
                default:
                    break;
            }
        }

        private void BearedOff(Identifier From)
        {
            movesEvents.Add(new BearOffEvent(From));
        }

        private void BearOff(Identifier To)
        {
            movesEvents.Add(new BarToBoardMoveEvent(snapShot.BoardData.TurnColor, To));
        }

        private void BoardToBoardMoveEvent(Identifier From, Identifier To)
        {
            movesEvents.Add(new BoardToBoardMoveEvent(From, To));
        }

        private void ConsumeDice(int OrginIndex, int DestinationIndex)
        {
            int moveCount = Mathf.Abs(OrginIndex - DestinationIndex);
            DiceData diceData = snapShot.BoardData.TurnDice;
            if (diceData.Dice1 == 0 && diceData.Dice2 == 0)
                return;

            int iteration = (snapShot.BoardData.TurnDice.AreSame ? 4 : 2) / 2;
            int dice1Count = 0;
            int dice2Count = 0;
            for (int i = 0; i < iteration; ++i)
            {
                dice1Count = diceData.Dice1 * (i + 1);
                dice2Count = diceData.Dice2 * (i + 1);
            }

            if ((dice1Count + dice2Count) == moveCount)
                diceData.Dice1 = diceData.Dice2 = 0;
            else if (dice1Count > 0 && moveCount <= dice1Count)
                diceData.Dice1 -= Mathf.RoundToInt(moveCount / iteration);
            else if (dice2Count > 0 && moveCount <= dice2Count)
                diceData.Dice2 -= Mathf.RoundToInt(moveCount / iteration);
        }
    }
}