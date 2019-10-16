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
using System.IO;
using ClientUtilities.ResourceManager;

namespace Assets.Scripts.GamePlayLogic
{
    public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        public int Count
        {
            get { return Pool.Count; }
        }

        private string TemplatePrefabPath;


        private Stack<T> Pool = null;

        public void InitiliazePool(string Path, int Count = 0)
        {
            Debug.Assert(TemplatePrefabPath != string.Empty, "Path is Empty");
            TemplatePrefabPath = Path;
            Pool = new Stack<T>(Count);

            for (int i = 0; i < Count; ++i)
                SendToPool(Instantiate(GameResourceManager.Instance.LoadPrefab(Path),
                                       Vector3.zero, Quaternion.identity).GetComponent<T>());
        }

        public void SendToPool(T Item)
        {

            Debug.Assert(!Contains(Item), "Item exist in the pool");

            if (Item == null)
                return;


            Item.gameObject.SetActive(false);
            Pool.Push(Item);
        }

        public T GetFromPull()
        {
            Debug.Assert(TemplatePrefabPath != string.Empty, "First of all intilize the pool");
            if (Pool.Count == 0)
                SendToPool(Instantiate(GameResourceManager.Instance.LoadPrefab(TemplatePrefabPath),
                                     Vector3.zero, Quaternion.identity).GetComponent<T>());
            Pool.Peek().gameObject.SetActive(true);
            return Pool.Pop();
        }

        public bool Contains(T Item)
        {
            if (Item == null)
                return false;

            return Pool.Contains(Item);
        }

        public void Clear()
        {
            Pool.Clear();
        }

        public T GetItemTypeOfPoolObject()
        {
            if (Pool.Count == 0)
                return null;

            return Pool.Peek();
        }
    }

    public class BeedObjectPool : ObjectPool<Beed>
    {
    }

    public class WhiteBeadPool : BeedObjectPool
    {
    }

    public class BlackBeadPool : BeedObjectPool
    {
    }

    public class TableManager : MonoBehaviorSingleton<TableManager>
    {

        public PointVisualizer SelectedBead
        {
            get;
            private set;
        }

        public WhiteBeadPool WhiteBeads = new WhiteBeadPool();
        public BlackBeadPool BlackBeads = new BlackBeadPool();

        private bool diceValueFilled = false;
        //private int dice1Value = 0;
        //private int dice2Value = 0;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<EventBase> movesEvents = new List<EventBase>();
        private SimulationManager simInstance = null;
        private PointVisualizerManager pvmInstance = null;

        private void Awake()
        {
            simInstance = SimulationManager.Instance;
            pvmInstance = PointVisualizerManager.Instance;
            diceValueFilled = false;

            WhiteBeads.InitiliazePool("WhiteBead", 15);
            BlackBeads.InitiliazePool("BlackBead", 15);

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
                SimulationManager.Instance.OnBearedOff += Instance_OnBearedOff;
                SimulationManager.Instance.OnReplayEnd += Instance_OnReplayEnd;
                SimulationManager.Instance.OnReplayIsLoadingFailed += Instance_OnReplayIsLoadingFailed;
                SimulationManager.Instance.OnReplayIsReady += Instance_OnReplayIsReady;
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
                SimulationManager.Instance.OnBearedOff -= Instance_OnBearedOff;
                SimulationManager.Instance.OnReplayEnd -= Instance_OnReplayEnd;
                SimulationManager.Instance.OnReplayIsLoadingFailed -= Instance_OnReplayIsLoadingFailed;
                SimulationManager.Instance.OnReplayIsReady -= Instance_OnReplayIsReady;
            }
        }

        private void Instance_OnBoardToBoardMove(Identifier From, Identifier To)
        {

            pvmInstance.HidePossibleMoves();
            pvmInstance.BoardToBoardMove(From, To);
            // pvmInstance.UpdateAllPointVisualizer();
            //ConsumeDice(pvmInstance.FindPointIndex(From), pvmInstance.FindPointIndex(To));
            //MoveTo(pvmInstance.FindPoint(From).PointData
            //      , pvmInstance.FindPoint(To).PointData);
        }

        private void Instance_OnBoardToBarMove(Identifier From)
        {
            pvmInstance.BoardToBarMove(From);
            // MoveTo(pvmInstance.FindPoint(From).PointData);
            //pvmInstance.UpdateAllPointVisualizer();

        }

        private void Instance_OnBearedOff(Identifier From)
        {
            pvmInstance.BeardOff(From);
           // pvmInstance.UpdateAllPointVisualizer();
        }


        private void Instance_OnBarToBoardMove(Identifier To)
        {
            pvmInstance.BarToBoardMove(To);
            //MoveTo(null, pvmInstance.FindPoint(To).PointData);
            // pvmInstance.UpdateAllPointVisualizer();
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


        private void Instance_OnReplayIsReady()
        {

        }

        private void Instance_OnReplayIsLoadingFailed()
        {

        }

        private void Instance_OnReplayEnd()
        {

        }
        private void OnTap(Vector2 Position)
        {
            if (!Dice.Instance.IsDiceRolled)
                return;


            int beardOff = 0;
            int beardedOff = 0;
            int GetBeadOutofBase = Utilities.GetOutOfBaseCheckerCount(simInstance.CurrentSimulator.Frame.Board.Points, simInstance.CurrentSimulator.Frame.Board.TurnColor);

            switch (simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                case PlayerColors.White:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    beardedOff = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                    break;
                case PlayerColors.Black:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;
                    beardedOff = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;

                    break;
                default:
                    break;
            }
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);
            if (hit.collider != null)
            {
                PointVisualizer tempBead = SelectedBead;
                SelectedBead = hit.transform.gameObject.GetComponent<PointVisualizer>();

                if ((beardOff == 0 && GetBeadOutofBase != 0) && tempBead != null && tempBead.PointData.ID != SelectedBead.PointData.ID && possibleMoves.Count != 0)
                {
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBead.PointData.ID != possibleMoves[i].To.ID)
                            continue;

                        MoveTo(tempBead.PointData, SelectedBead.PointData);

                        SelectedBead = tempBead = null;

                        return;
                    }

                }

                if (beardOff != 0)
                {
                    ResePossibleMoves();
                    FindPossibleBarToBoardMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());
                    if (SelectedBead != null)
                        for (int i = 0; i < possibleMoves.Count; ++i)
                        {
                            if (SelectedBead.PointData.ID != possibleMoves[i].To.ID)
                                continue;

                            MoveTo(null, SelectedBead.PointData);
                            SelectedBead = tempBead = null;
                            pvmInstance.HidePossibleMoves();
                            break;
                        }
                    return;
                }
                else if ((GetBeadOutofBase - beardedOff) == 0 && SelectedBead != null)
                {
                    ResePossibleMoves();
                    FindPossibleBearedOff();
                    FindPossibleMoves();
                    pvmInstance.ShowPossibleMovesOut(possibleMoves.ToArray());
                    for (int i = 0; i < possibleMoves.Count; ++i)
                    {
                        if (SelectedBead.PointData.ID != possibleMoves[i].From.ID)
                            continue;

                        MoveTo(SelectedBead.PointData, null);
                        SelectedBead = tempBead = null;
                        //pvmInstance.HidePossibleMoves();
                        return;
                    }
                }


                ResePossibleMoves();

                if (SelectedBead != null && SelectedBead.PointData.CheckerCount != 0 && SelectedBead.PointData.Color == simInstance.CurrentSimulator.Frame.Board.TurnColor)
                {

                    tempBead = null;

                    FindPossibleMoves();
                    pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());

                    return;
                }
            }

            pvmInstance.HidePossibleMoves();
            SelectedBead = null;
        }

        private void ResePossibleMoves()
        {
            possibleMoves.Clear();
            pvmInstance.HidePossibleMoves();
        }

        private void FindPossibleBearedOff()
        {
            //if (Utilities.GetOutOfBaseCheckerCount(simInstance.CurrentSimulator.Frame.Board.Points, simInstance.CurrentSimulator.Frame.Board.TurnColor) != 0)
            //    return;

            for (int i = 0; i < pvmInstance.Points.Length; ++i)
            {
                MoveInfo[] mi = Logic.GetPossibleBearedOffs(simInstance.CurrentSimulator.Frame.Board, pvmInstance.Points[i].PointData.ID);
                if (mi != null)
                    possibleMoves.AddRange(mi);

            }

        }

        private void FindPossibleBarToBoardMoves()
        {
            possibleMoves.AddRange(Logic.GetPossibleBarToBoardMoves(simInstance.CurrentSimulator.Frame.Board));
        }

        private void FindPossibleMoves()
        {
            if (SelectedBead == null)
                return;
            possibleMoves.AddRange(Logic.GetPossibleBoardToBoardMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBead.PointData.ID));
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