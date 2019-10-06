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

        private bool diceValueFilled = false;
        private int dice1Value = 0;
        private int dice2Value = 0;

        private List<PointData> possibleMoves = new List<PointData>();
        private List<EventBase> movesEvents = new List<EventBase>();
        private SimulationManager.SnapShot snapShot;


        private void Start()
        {
            snapShot = SimulationManager.Instance.Shot;
            diceValueFilled = false;
        }



        private void OnEnable()
        {
            Tap.Instance.OnTapBegin += OnTap;
            InGameUI.OnChangeTurnEventClick += OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick += OnUndoEventClick;
        }


        private void OnDisable()
        {
            if (Tap.Instance != null)
                Tap.Instance.OnTapBegin -= OnTap;
            InGameUI.OnChangeTurnEventClick -= OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick -= OnUndoEventClick;
        }

        private void OnUndoEventClick()
        {
            diceValueFilled = false;
            movesEvents.Clear();
            SimulationManager.Instance.UndoActions();
        }


        private void OnChangeTurnEventClick()
        {

            //if (dice1Value != 0 && dice2Value!= 0)
            //    return;

            for (int i = 0; i < movesEvents.Count; ++i)
            {
                EventBase ev = movesEvents[i];
                SimulationManager.Instance.Simulator.SendEvent(ev);
            }

            movesEvents.Clear();
            SimulationManager.Instance.Simulator.SendEvent(new FinishTurnEvent(snapShot.BoardData.TurnColor));
            diceValueFilled = false;
        }


        private void OnTap(Vector2 Position)
        {
            if (!Dice.Instance.IsDiceRolled)
                return;

            if (!diceValueFilled)
            {
                if (!snapShot.BoardData.TurnDice.AreSame)
                {
                    dice1Value = snapShot.BoardData.TurnDice.Dice1;
                    dice2Value = snapShot.BoardData.TurnDice.Dice2;
                }
                else
                    dice1Value = dice2Value = snapShot.BoardData.TurnDice.Dice1 * 2;
                diceValueFilled = true;
            }

            int beardOff = 0;
            int GetBeedOutofBase = Logic.GetOutOfBaseCheckerCount(snapShot.BoardData, snapShot.BoardData.TurnColor);
         
            switch (snapShot.BoardData.TurnColor)
            {
                case PlayerColors.White:
                    beardOff = snapShot.BoardData.WhitePlayer.BarCheckerCount;
                    break;
                case PlayerColors.Black:
                    beardOff = snapShot.BoardData.BlackPlayer.BarCheckerCount;
                    break;
                default:
                    break;
            }
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider != null)
            {
                PointVisualizer tempBeed = SelectedBeed;
                SelectedBeed = hit.transform.gameObject.GetComponent<PointVisualizer>();

                if (GetBeedOutofBase!=0 && tempBeed != null && tempBeed.PointData.ID != SelectedBeed.PointData.ID && possibleMoves.Count != 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].ID)
                            continue;

                        if (beardOff != 0)
                            tempBeed = null;
                        MoveTo(tempBeed, SelectedBeed.PointData);
                        SelectedBeed = tempBeed = null;
                        PointVisualizerManager.Instance.HidePossibleMoves();
                        return;
                    }

                }if(GetBeedOutofBase == 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].ID)
                            continue;
                        
                        MoveTo(tempBeed, null);
                        SelectedBeed = tempBeed = null;
                        PointVisualizerManager.Instance.HidePossibleMoves();
                        return;
                    }
                }




                possibleMoves.Clear();
                PointVisualizerManager.Instance.HidePossibleMoves();
                if (beardOff != 0)
                {
                    tempBeed = null;
                    FindPossibleBarToBoardMoves();
                    PointVisualizerManager.Instance.ShowPossibleMoves(possibleMoves.ToArray());
                    return;
                }else if(GetBeedOutofBase == 0 && SelectedBeed!=null)
                {
                    tempBeed = null;
                    possibleMoves.AddRange(Logic.GetPossibleBearedOffs(snapShot.BoardData, SelectedBeed.PointData.ID));
                    return;
                }
                else if (SelectedBeed != null && SelectedBeed.PointData.CheckerCount != 0 && SelectedBeed.PointData.Color == snapShot.BoardData.TurnColor)
                {

                    tempBeed = null;
                    Debug.Log("Beed Selected");
                    FindPossibleMoves();
                    PointVisualizerManager.Instance.ShowPossibleMoves(possibleMoves.ToArray());

                    return;
                }
            }

            PointVisualizerManager.Instance.HidePossibleMoves();
            SelectedBeed = null;
        }

        private void FindPossibleBarToBoardMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleBarToBoardMoves(snapShot.BoardData, snapShot.BoardData.TurnColor));

        }
        private void FindPossibleMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value + dice2Value));
            possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value));
            possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice2Value));

            //if (!snapShot.BoardData.TurnDice.AreSame)
            //{
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value + dice2Value));
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value));
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice2Value));
            //}
            //else
            //{
            //    for(int i =0; i<dice1Value+dice2Value;)
            //    {
            //        i += snapShot.BoardData.TurnDice.Dice1;
            //        possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID,i));

            //    }
            //}

        }

        private void MoveTo(PointVisualizer Orgin = null, PointData Destination = null)
        {
            PointVisualizer finalPoint = null;
            EventBase.Types type = EventBase.Types.FinishTurn;
            if (Destination != null)
                finalPoint = PointVisualizerManager.Instance.FindPoint(Destination);
            if (Orgin != null && finalPoint != null && Orgin.pointBeeds.Count != 0)
            {
                GameObject go = Orgin.pointBeeds.Peek();
                if (go != null)
                {
                    go.transform.SetParent(null);
                    go.transform.position = finalPoint.FindPosition(Destination.CheckerCount);
                    go.transform.SetParent(finalPoint.transform);

                    Orgin.PointData.CheckerCount--;
                    Destination.CheckerCount++;
                    Destination.Color = Orgin.PointData.Color;

                    Destination.Color = snapShot.BoardData.TurnColor;
                    finalPoint.pointBeeds.Push(Orgin.pointBeeds.Pop());

                    ConsumeDice(Orgin.Index, finalPoint.Index);
                    type = EventBase.Types.BoardToBoardMove;

                    //Move Events Should Add to This List
                    //movesEvents.Add()????
                }
            }
            else if (Orgin == null && finalPoint != null)
            {
                BarOff temp = null;
                for (int i = PointVisualizerManager.Instance.ExtraBar.Length / 2;
                    i < PointVisualizerManager.Instance.ExtraBar.Length; ++i)
                {
                    if (PointVisualizerManager.Instance.ExtraBar[i].Color != snapShot.BoardData.TurnColor)
                        continue;

                    temp = PointVisualizerManager.Instance.ExtraBar[i];
                    break;
                }

                if (temp != null)
                {
                    GameObject go = temp.pointBeeds.Peek();
                    if (go != null)
                    {
                        go.transform.SetParent(null);
                        go.transform.position = finalPoint.FindPosition(Destination.CheckerCount);
                        go.transform.SetParent(finalPoint.transform);

                        temp.BarCheckerCount--;
                        switch (snapShot.BoardData.TurnColor)
                        {
                            case PlayerColors.White:
                                snapShot.BoardData.WhitePlayer.BarCheckerCount--;
                                break;
                            case PlayerColors.Black:
                                snapShot.BoardData.BlackPlayer.BarCheckerCount--;
                                break;
                            default:
                                break;
                        }
                        Destination.CheckerCount++;
                        Destination.Color = Orgin.PointData.Color;

                        Destination.Color = snapShot.BoardData.TurnColor;
                        finalPoint.pointBeeds.Push(Orgin.pointBeeds.Pop());

                        ConsumeDice(0, finalPoint.Index);
                        type = EventBase.Types.BearOff;

                    }
                }else if(Orgin!=null && Destination == null)
                {
                    GameObject go = Orgin.pointBeeds.Peek();
                    if (go != null)
                    {
                        go.transform.SetParent(null);
                        BarOff tempBar = null;
                       
                        switch (snapShot.BoardData.TurnColor)
                        {
                            case PlayerColors.White:
                                // snapShot.BoardData.WhitePlayer.BarCheckerCount--;
                                tempBar = PointVisualizerManager.Instance.ExtraBar[0];
                              

                                break;
                            case PlayerColors.Black:
                                //snapShot.BoardData.BlackPlayer.BarCheckerCount--;
                                tempBar = PointVisualizerManager.Instance.ExtraBar[1];

                      
                                break;
                            default:
                                break;
                        }
                        go.transform.position = tempBar.FindPosition(tempBar.BarCheckerCount);
                        go.transform.SetParent(tempBar.transform);
                        tempBar.BarCheckerCount++;
                        Orgin.PointData.CheckerCount--;
                        ConsumeDice(Orgin.PointData.Index, 0);
                        type = EventBase.Types.BearedOff;

                    }
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
            if (dice1Value == 0 && dice2Value == 0)
                return;

            int iteration = (snapShot.BoardData.TurnDice.AreSame ? 4 : 2);


            if (snapShot.BoardData.TurnDice.AreSame)
            {
                for (int i = 0; i < moveCount; ++i)
                {
                    if (dice1Value != 0)
                        dice1Value--;
                    else if (dice2Value != 0)
                        dice2Value--;
                }
            }
            else
            {

                if (moveCount == dice1Value)
                    dice1Value = 0;
                else if (moveCount == dice2Value)
                    dice2Value = 0;
                else if (dice1Value + dice2Value == moveCount)
                    dice1Value = dice2Value = 0;
            }


            //if (dice1Value == 0)
            //    snapShot.BoardData.TurnDice.Dice1 = 0;
            //if (dice2Value == 0)
            //    snapShot.BoardData.TurnDice.Dice2 = 0;
            //if (dice1Count > 0 && moveCount <= dice1Count)
            //    diceData.Dice1 -= Mathf.RoundToInt(moveCount / iteration);
            //else if (dice2Count > 0 && moveCount <= dice2Count)
            //    diceData.Dice2 -= Mathf.RoundToInt(moveCount / iteration);
            //else if ((dice1Count + dice2Count) == (moveCount))
            //    diceData.Dice1 = diceData.Dice2 = 0;



        }
    }
}