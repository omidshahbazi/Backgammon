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
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Tables;

namespace Assets.Scripts.GamePlayLogic
{

    public class TableEvent
    {
        public EventBase Event
        {
            get;
            private set;
        }

        public bool IsSendByNetWork
        {
            get;
            private set;
        }

        public TableEvent(EventBase Event, bool IsSendByNetWork)
        {
            this.Event = Event;
            this.IsSendByNetWork = IsSendByNetWork;
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

        public TablesDataManager.Table SelectedTable
        {
            get;
            private set;
        }

        public bool IsGameStarted
        {
            get;
            private set;
        }

        public WhiteBeadPool WhiteBeads = new WhiteBeadPool();
        public BlackBeadPool BlackBeads = new BlackBeadPool();

      
        //private int dice1Value = 0;
        //private int dice2Value = 0;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<TableEvent> movesEvents = new List<TableEvent>();
        private SimulationManager simInstance = null;
        private PointVisualizerManager pvmInstance = null;


        private void Awake()
        {
            simInstance = SimulationManager.Instance;
            pvmInstance = PointVisualizerManager.Instance;
     

            WhiteBeads.InitiliazePool("WhiteBead", 15);
            BlackBeads.InitiliazePool("BlackBead", 15);

        }


        private void OnEnable()
        {
            Tap.Instance.OnTapBegin += OnTap;
            InGameMenu.OnChangeTurnEventClick += OnChangeTurn;
            InGameMenu.OnUndoEventClick += OnUndoEventClick;


            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnTableReady += Instance_OnTableReady;
                SimulationManager.Instance.OnBoardToBoardMove += Instance_OnBoardToBoardMove;
                SimulationManager.Instance.OnBoardToBarMove += Instance_OnBoardToBarMove;
                SimulationManager.Instance.OnBarToBoardMove += Instance_OnBarToBoardMove;
                SimulationManager.Instance.OnBearedOff += Instance_OnBearedOff;
                SimulationManager.Instance.OnReplayEnd += Instance_OnReplayEnd;
                SimulationManager.Instance.OnReplayIsLoadingFailed += Instance_OnReplayIsLoadingFailed;
                SimulationManager.Instance.OnReplayIsReady += Instance_OnReplayIsReady;
                SimulationManager.Instance.OnGameFinished += Instance_OnGameFinished;

            }
            //if (RequestManager.Instance != null)
            //    RequestManager.Instance.OnMatchFound += Instance_OnMatchFound;

        }

        private void Instance_OnTableReady()
        {
            IsGameStarted = true;
        }

        private void OnDisable()
        {
            if (Tap.Instance != null)
                Tap.Instance.OnTapBegin -= OnTap;
            InGameMenu.OnChangeTurnEventClick -= OnChangeTurn;
            InGameMenu.OnUndoEventClick -= OnUndoEventClick;


            //if (RequestManager.Instance != null)
            //    RequestManager.Instance.OnMatchFound -= Instance_OnMatchFound;

            if (SimulationManager.Instance != null)
            {

                SimulationManager.Instance.OnBoardToBoardMove -= Instance_OnBoardToBoardMove;
                SimulationManager.Instance.OnBoardToBarMove -= Instance_OnBoardToBarMove;
                SimulationManager.Instance.OnBarToBoardMove -= Instance_OnBarToBoardMove;
                SimulationManager.Instance.OnBearedOff -= Instance_OnBearedOff;
                SimulationManager.Instance.OnReplayEnd -= Instance_OnReplayEnd;
                SimulationManager.Instance.OnReplayIsLoadingFailed -= Instance_OnReplayIsLoadingFailed;
                SimulationManager.Instance.OnReplayIsReady -= Instance_OnReplayIsReady;
                SimulationManager.Instance.OnGameFinished -= Instance_OnGameFinished;
                SimulationManager.Instance.OnTableReady -= Instance_OnTableReady;


            }
        }

        public void SetSelectedTableData(TablesDataManager.Table Table)
        {
            SelectedTable = Table;
        }

        //private void Instance_OnMatchFound()
        //{

        //}

        private void Instance_OnGameFinished(PlayerColors WinnerColor, int Score)
        {
            IsGameStarted = false;
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
         
            movesEvents.Clear();
            SimulationManager.Instance.UndoActions();
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

            if (!IsGameStarted || simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor || !Dice.Instance.IsDiceRolled)
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

        public void BarToBoardMove(Identifier From, bool IsSendByNetwork = false)
        {
            ResetMyActions(IsSendByNetwork);
            movesEvents.Add(new TableEvent(new BarToBoardMoveEvent(simInstance.CurrentSimulator.Frame.Board.TurnColor, From), IsSendByNetwork));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1].Event);
        }

        public void BearOff(Identifier From, bool IsSendBywetWork = false)
        {
            ResetMyActions(IsSendBywetWork);
            movesEvents.Add(new TableEvent( new BearOffEvent(From), IsSendBywetWork));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1].Event);
        }

        public void BoardToBoardMoveEvent(Identifier From, Identifier To, bool IsSendByNetwork = false)
        {
            ResetMyActions(IsSendByNetwork);
            movesEvents.Add(new TableEvent( new BoardToBoardMoveEvent(From, To),IsSendByNetwork));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1].Event);
        }


        private void ResetMyActions(bool IsRecivedFromNetwork)
        {
            if (IsRecivedFromNetwork)
            {
                for (int i = 0; i < movesEvents.Count; ++i)
                {
                    TableEvent ev = movesEvents[i];
                    if (!ev.IsSendByNetWork)
                    {
                        SimulationManager.Instance.UndoActions();
                        movesEvents.Remove(ev);
                    }
                }
            }
        }

        public void OnChangeTurn(bool IsRecivedFromNetwork = false)
        {
            ResetMyActions(IsRecivedFromNetwork);

            //if (dice1Value != 0 && dice2Value!= 0)
            //    return;

            for (int i = 0; i < movesEvents.Count; ++i)
            {
                TableEvent ev = movesEvents[i];
                SimulationManager.Instance.SendEvent(ev.Event);

                switch (ev.Event.GetType())
                {
                    case EventBase.Types.BoardToBoardMove:
                        BoardToBoardMoveEvent btbe = (BoardToBoardMoveEvent)ev.Event;
                        if (!ev.IsSendByNetWork)
                        {
                            Debug.Log("BoardToBoardMove sent to the server");
                            RequestManager.Instance.Network.BoardToBoardMove(simInstance.Hash, btbe.From, btbe.To);
                        }
                        break;
                    case EventBase.Types.BearOff:
                        BearOffEvent boe = (BearOffEvent)ev.Event;
                        if (!ev.IsSendByNetWork)
                        {
                            Debug.Log("BearOff sent to the server");
                            RequestManager.Instance.Network.BearOff(simInstance.Hash, boe.From);
                        }

                        break;
                    case EventBase.Types.BarToBoardMove:
                        BarToBoardMoveEvent btb = (BarToBoardMoveEvent)ev.Event;
                        if (!ev.IsSendByNetWork)
                        {
                            Debug.Log("BardToBoardMove sent to the server");
                            RequestManager.Instance.Network.BardToBoardMove(simInstance.Hash, btb.Color, btb.To);
                        }
                        break;
                    default:
                        break;
                }

            }

            movesEvents.Clear();


            simInstance.SendEvent(new FinishTurnEvent(simInstance.Board.TurnColor));
            if (!IsRecivedFromNetwork)
            {
                Debug.Log("FinishTurn sent to the server");
                RequestManager.Instance.Network.FinishTurn(simInstance.Hash, simInstance.CurrentSimulator.Frame.Board.TurnColor);
            }
            simInstance.SendCurrentEvent(new FinishTurnEvent(simInstance.CurrentSimulator.Frame.Board.TurnColor));

          

            ResePossibleMoves();
        }

    }
}