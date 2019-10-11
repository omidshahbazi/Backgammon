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
        //private int dice1Value = 0;
        //private int dice2Value = 0;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<EventBase> movesEvents = new List<EventBase>();
        private SimulationManager simInstance = null;
        private PointVisualizerManager pvmInstance = null;

        private void Start()
        {
            simInstance = SimulationManager.Instance;
            pvmInstance = PointVisualizerManager.Instance;
            diceValueFilled = false;
        }


        private void OnEnable()
        {
            Tap.Instance.OnTapBegin += OnTap;
            InGameUI.OnChangeTurnEventClick += OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick += OnUndoEventClick;

            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnBoardToBoardMove += Instance_OnBoardToBoardMove;
                SimulationManager.Instance.OnBoardToBarMove += Instance_OnBoardToBarMove;
                SimulationManager.Instance.OnBarToBoardMove += Instance_OnBarToBoardMove;
            }

        }


        private void OnDisable()
        {
            if (Tap.Instance != null)
                Tap.Instance.OnTapBegin -= OnTap;
            InGameUI.OnChangeTurnEventClick -= OnChangeTurnEventClick;
            InGameUI.OnUndoEventClick -= OnUndoEventClick;

            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnBoardToBoardMove -= Instance_OnBoardToBoardMove;
                SimulationManager.Instance.OnBoardToBarMove -= Instance_OnBoardToBarMove;
                SimulationManager.Instance.OnBarToBoardMove -= Instance_OnBarToBoardMove;


            }
        }

        private void Instance_OnBoardToBoardMove(Identifier From, Identifier To)
        {
            pvmInstance.UpdateAllPointVisualizer();
            //ConsumeDice(pvmInstance.FindPointIndex(From), pvmInstance.FindPointIndex(To));
            //MoveTo(pvmInstance.FindPoint(From).PointData
            //      , pvmInstance.FindPoint(To).PointData);
        }

        private void Instance_OnBoardToBarMove(Identifier From)
        {
            // MoveTo(pvmInstance.FindPoint(From).PointData);
            pvmInstance.UpdateAllPointVisualizer();

        }

        private void Instance_OnBarToBoardMove(Identifier To)
        {
            //MoveTo(null, pvmInstance.FindPoint(To).PointData);
            pvmInstance.UpdateAllPointVisualizer();
            // int beginIndex = simInstance.Board.TurnColor == PlayerColors.Black ? 24 : -1;
            //ConsumeDice(beginIndex,pvmInstance.FindPointIndex(To));
        }


        private void OnUndoEventClick()
        {
            ResePossibleMoves();
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
                SimulationManager.Instance.SendEvent(ev);
            }

            movesEvents.Clear();


            simInstance.SendEvent(new FinishTurnEvent(simInstance.Board.TurnColor));
            simInstance.SendCurrentEvent(new FinishTurnEvent(simInstance.CurrentSimulator.Frame.Board.TurnColor));

            diceValueFilled = false;
            ResePossibleMoves();
        }


        private void OnTap(Vector2 Position)
        {
            if (!Dice.Instance.IsDiceRolled)
                return;


            int beardOff = 0;
            int GetBeedOutofBase = Logic.GetOutOfBaseCheckerCount(simInstance.CurrentSimulator.Frame.Board, simInstance.CurrentSimulator.Frame.Board.TurnColor);

            switch (simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                case PlayerColors.White:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    break;
                case PlayerColors.Black:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;
                    break;
                default:
                    break;
            }
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider != null)
            {
                PointVisualizer tempBeed = SelectedBeed;
                SelectedBeed = hit.transform.gameObject.GetComponent<PointVisualizer>();

                if ((beardOff == 0 && GetBeedOutofBase != 0) && tempBeed != null && tempBeed.PointData.ID != SelectedBeed.PointData.ID && possibleMoves.Count != 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].To.ID)
                            continue;

                        MoveTo(tempBeed.PointData, SelectedBeed.PointData);

                        SelectedBeed = tempBeed = null;
                        pvmInstance.HidePossibleMoves();
                        return;
                    }

                }

                if (beardOff != 0)
                {
                    ResePossibleMoves();
                    FindPossibleBarToBoardMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());
                    if (SelectedBeed != null)
                        for (int i = 0; i < possibleMoves.Count; ++i)
                        {
                            if (SelectedBeed.PointData.ID != possibleMoves[i].To.ID)
                                continue;

                            MoveTo(null, SelectedBeed.PointData);
                            SelectedBeed = tempBeed = null;
                            pvmInstance.HidePossibleMoves();
                            break;
                        }
                    return;
                }
                else if (GetBeedOutofBase == 0 && SelectedBeed!=null)
                {
                    ResePossibleMoves();
                    FindPossibleBearedOff();
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].From.ID)
                            continue;

                        MoveTo(SelectedBeed.PointData, null);
                        SelectedBeed = tempBeed = null;
                        //pvmInstance.HidePossibleMoves();
                        return;
                    }
                }


                ResePossibleMoves();
    
                if (SelectedBeed != null && SelectedBeed.PointData.CheckerCount != 0 && SelectedBeed.PointData.Color == simInstance.CurrentSimulator.Frame.Board.TurnColor)
                {

                    tempBeed = null;

                    FindPossibleMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());

                    return;
                }
            }

            pvmInstance.HidePossibleMoves();
            SelectedBeed = null;
        }

        private void ResePossibleMoves()
        {
            possibleMoves.Clear();
            pvmInstance.HidePossibleMoves();
        }

        private void FindPossibleBearedOff()
        {
            if (Logic.GetOutOfBaseCheckerCount(simInstance.CurrentSimulator.Frame.Board, simInstance.CurrentSimulator.Frame.Board.TurnColor) != 0)
                return;

            for (int i = 0; i < pvmInstance.Points.Length; ++i)
                possibleMoves.AddRange(Logic.GetPossibleBearedOffs(simInstance.CurrentSimulator.Frame.Board, pvmInstance.Points[i].PointData.ID));

        }

        private void FindPossibleBarToBoardMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleBarToBoardMoves(simInstance.CurrentSimulator.Frame.Board, simInstance.CurrentSimulator.Frame.Board.TurnColor));
        }

        private void FindPossibleMoves()
        {

            int totalMoves = 0;
            bool isPair = simInstance.CurrentSimulator.Frame.Board.TurnDice.IsPair;
            for (int i = 0; i < simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length; ++i)
            {
                int move = simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[i];
                totalMoves += move;
                MoveInfo[] mi = Logic.GetPossibleMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID, isPair ? totalMoves : move);
                if ((mi == null || mi.Length==0) && isPair)
                    return;
                possibleMoves.AddRange(mi);
            }

            if (simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length > 1)
                possibleMoves.AddRange(Logic.GetPossibleMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID, totalMoves));

        }



        private void MoveTo(PointData Orgin = null, PointData Destination = null)
        {
            PointVisualizer startPoint = null;
            PointVisualizer finalPoint = null;
            EventBase.Types type = EventBase.Types.FinishTurn;


            if (Orgin != null)
                startPoint = pvmInstance.FindPoint(Orgin);
            if (Destination != null)
                finalPoint = pvmInstance.FindPoint(Destination);
            if (Orgin != null && finalPoint != null && startPoint.pointBeeds.Count != 0)
                type = EventBase.Types.BoardToBoardMove;
            else if (Orgin == null && finalPoint != null)
                type = EventBase.Types.BarToBoardMove;
            else if (Orgin != null && Destination == null)
                type = EventBase.Types.BearOff;

            switch (type)
            {
                case EventBase.Types.BoardToBoardMove:
                    BoardToBoardMoveEvent(startPoint.PointData.ID, finalPoint.PointData.ID);
                    break;
                case EventBase.Types.BearOff:
                    BearOff(startPoint.PointData.ID);
                    break;
                case EventBase.Types.BarToBoardMove:
                    BarToBoardMove(finalPoint.PointData.ID);
                    break;
                default:
                    break;
            }
        }

        private void BarToBoardMove(Identifier From)
        {
            movesEvents.Add(new BarToBoardMoveEvent(simInstance.CurrentSimulator.Frame.Board.TurnColor, From));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1]);
        }

        private void BearOff(Identifier From)
        {
            movesEvents.Add(new BearOffEvent(From));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1]);
        }

        private void BoardToBoardMoveEvent(Identifier From, Identifier To)
        {
            movesEvents.Add(new BoardToBoardMoveEvent(From, To));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1]);
        }

        //    private void ConsumeDice(int OrginIndex, int DestinationIndex)
        //    {

        //        int moveCount = Mathf.Abs(OrginIndex - DestinationIndex);
        //        DiceData diceData = simInstance.CurrentSimulator.Frame.Board.TurnDice;
        //        if (dice1Value == 0 && dice2Value == 0)
        //            return;

        //        int iteration = (simInstance.CurrentSimulator.Frame.Board.TurnDice.AreSame ? 4 : 2);


        //        if (simInstance.CurrentSimulator.Frame.Board.TurnDice.AreSame)
        //        {
        //            for (int i = 0; i < moveCount; ++i)
        //            {
        //                if (dice1Value != 0)
        //                    dice1Value--;
        //                else if (dice2Value != 0)
        //                    dice2Value--;
        //            }
        //        }
        //        else
        //        {

        //            if (moveCount == dice1Value)
        //                dice1Value = 0;
        //            else if (moveCount == dice2Value)
        //                dice2Value = 0;
        //            else if (dice1Value + dice2Value == moveCount)
        //                dice1Value = dice2Value = 0;
        //        }


        //        //if (dice1Value == 0)
        //        //    snapShot.BoardData.TurnDice.Dice1 = 0;
        //        //if (dice2Value == 0)
        //        //    snapShot.BoardData.TurnDice.Dice2 = 0;
        //        //if (dice1Count > 0 && moveCount <= dice1Count)
        //        //    diceData.Dice1 -= Mathf.RoundToInt(moveCount / iteration);
        //        //else if (dice2Count > 0 && moveCount <= dice2Count)
        //        //    diceData.Dice2 -= Mathf.RoundToInt(moveCount / iteration);
        //        //else if ((dice1Count + dice2Count) == (moveCount))
        //        //    diceData.Dice1 = diceData.Dice2 = 0;
        //    }
    }
}