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
            ConsumeDice(pvmInstance.FindPointIndex(From), pvmInstance.FindPointIndex(To));
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
                SimulationManager.Instance.SendEvent(ev);
            }

            movesEvents.Clear();
            simInstance.SendCurrentEvent(new FinishTurnEvent(simInstance.Board.TurnColor));
            simInstance.SendEvent(new FinishTurnEvent(simInstance.Board.TurnColor));
            diceValueFilled = false;
        }


        private void OnTap(Vector2 Position)
        {
            if (!Dice.Instance.IsDiceRolled)
                return;

            if (!diceValueFilled)
            {
                if (!simInstance.CurrentSimulator.Frame.Board.TurnDice.AreSame)
                {
                    dice1Value = simInstance.CurrentSimulator.Frame.Board.TurnDice.Dice1;
                    dice2Value = simInstance.CurrentSimulator.Frame.Board.TurnDice.Dice2;
                }
                else
                    dice1Value = dice2Value = simInstance.CurrentSimulator.Frame.Board.TurnDice.Dice1 * 2;
                diceValueFilled = true;
            }

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

                if (GetBeedOutofBase != 0 && tempBeed != null && tempBeed.PointData.ID != SelectedBeed.PointData.ID && possibleMoves.Count != 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].ID)
                            continue;

                        if (beardOff != 0)
                            tempBeed = null;
                        MoveTo(tempBeed.PointData, SelectedBeed.PointData);
                        SelectedBeed = tempBeed = null;
                        pvmInstance.HidePossibleMoves();
                        return;
                    }

                }
                if (GetBeedOutofBase == 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBeed.PointData.ID != possibleMoves[i].ID)
                            continue;

                        MoveTo(tempBeed.PointData, null);
                        SelectedBeed = tempBeed = null;
                        pvmInstance.HidePossibleMoves();
                        return;
                    }
                }


    

                possibleMoves.Clear();
                pvmInstance.HidePossibleMoves();
                if (beardOff != 0)
                {
                    tempBeed = null;
                    FindPossibleBarToBoardMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());
                    return;
                }
                else if (GetBeedOutofBase == 0 && SelectedBeed != null)
                {
                    tempBeed = null;
                    possibleMoves.AddRange(Logic.GetPossibleBearedOffs(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID));
                    return;
                }
                else if (SelectedBeed != null && SelectedBeed.PointData.CheckerCount != 0 && SelectedBeed.PointData.Color == simInstance.CurrentSimulator.Frame.Board.TurnColor)
                {

                    tempBeed = null;
                    Debug.Log("Beed Selected");
                    FindPossibleMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());

                    return;
                }
            }

            pvmInstance.HidePossibleMoves();
            SelectedBeed = null;
        }

        private void FindPossibleBarToBoardMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleBarToBoardMoves(simInstance.CurrentSimulator.Frame.Board, simInstance.CurrentSimulator.Frame.Board.TurnColor));

        }

        private void FindPossibleMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID, dice1Value + dice2Value));
            possibleMoves.AddRange(Logic.GetPossibleMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID, dice1Value));
            possibleMoves.AddRange(Logic.GetPossibleMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBeed.PointData.ID, dice2Value));

            //if (!sim .TurnDice.AreSame)
            //{
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value + dice2Value));
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice1Value));
            //    possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, dice2Value));
            //}
            //else
            //{
            //    for (int i = 0; i < dice1Value + dice2Value;)
            //    {
            //        i += snapShot.BoardData.TurnDice.Dice1;
            //        possibleMoves.AddRange(Logic.GetPossibleMoves(snapShot.BoardData, SelectedBeed.PointData.ID, i));

            //    }
            //}

        }



        private void MoveTo(PointData Orgin = null, PointData Destination = null)
        {
            PointVisualizer startPoint = null;
            PointVisualizer finalPoint = null;
            EventBase.Types type = EventBase.Types.FinishTurn;


            if (Destination != null)
                startPoint = pvmInstance.FindPoint(Orgin);
            if (Destination != null)
                finalPoint = pvmInstance.FindPoint(Destination);
            if (Orgin != null && finalPoint != null && startPoint.pointBeeds.Count != 0)
                type = EventBase.Types.BoardToBoardMove;
            else if (Orgin == null && finalPoint != null)
                type = EventBase.Types.BearOff;
            else if (Orgin != null && Destination == null)
                type = EventBase.Types.BearOff;

            switch (type)
            {
                case EventBase.Types.BoardToBoardMove:
                    BoardToBoardMoveEvent(startPoint.PointData.ID, finalPoint.PointData.ID);
                    break;
                case EventBase.Types.BearOff:
                    BearOff(finalPoint.PointData.ID);
                    break;
                case EventBase.Types.BarToBoardMove:
                    BarToBoardMove(startPoint.PointData.ID);
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

        private void ConsumeDice(int OrginIndex, int DestinationIndex)
        {

            int moveCount = Mathf.Abs(OrginIndex - DestinationIndex);
            DiceData diceData = simInstance.CurrentSimulator.Frame.Board.TurnDice;
            if (dice1Value == 0 && dice2Value == 0)
                return;

            int iteration = (simInstance.CurrentSimulator.Frame.Board.TurnDice.AreSame ? 4 : 2);


            if (simInstance.CurrentSimulator.Frame.Board.TurnDice.AreSame)
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