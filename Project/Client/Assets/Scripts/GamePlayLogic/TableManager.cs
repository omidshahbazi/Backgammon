#define BACKGAMOON_NEW_GAME_PLAY_VERSION
using Simulation.Data.Game;
using UnityEngine;
using ClientUtilities.Tap;
using Simulation.Logic;
using System.Collections.Generic;
using Simulation.Data.Event;
using Simulation.Common;
using Assets.Scripts.GamePlayLogic.UI;
using ClientUtilities.Singleton;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Tables;
using Networking.Common;

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

#if !BACKGAMOON_NEW_GAME_PLAY_VERSION
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

		public WhiteBeadPool WhiteBeads = null;
		public BlackBeadPool BlackBeads = null;
		private List<MoveInfo> possibleMoves = new List<MoveInfo>();
		private List<TableEvent> movesEvents = new List<TableEvent>();
		private SimulationManager simInstance = null;
		private PointVisualizerManager pvmInstance = null;
		private List<Beed> possibleBeeds = new List<Beed>();
		private BarOff selectBar;

		private void Awake()
		{
			simInstance = SimulationManager.Instance;
			pvmInstance = PointVisualizerManager.Instance;


			WhiteBeads = new WhiteBeadPool();
			BlackBeads = new BlackBeadPool();
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
		}


		private void Instance_OnTableReady()
		{
			Dice.Instance.OnDiceRolledFinished += Instance_OnDiceRolledFinished;
			IsGameStarted = true;
		}

		private void OnDisable()
		{
			if (Tap.Instance != null)
				Tap.Instance.OnTapBegin -= OnTap;
			InGameMenu.OnChangeTurnEventClick -= OnChangeTurn;
			InGameMenu.OnUndoEventClick -= OnUndoEventClick;


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

		private void ShowPossibleBarToBoard()
		{
			ResetPossibleMoves();
			if (simInstance.CurrentSimulator.Frame.Board.TurnColor != simInstance.YourColor)
				return;
			int beardOff = 0;

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


			if (beardOff != 0)
			{
				FindPossibleBarToBoardMoves();
			}
			pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());
		}

		private void ShowBeedGlow()
		{
			HideBeedGlow(true);
			if (simInstance.CurrentSimulator.Frame.Board.TurnColor != simInstance.YourColor)
				return;

			int beardOff = 0;
			int moveCount = 0;
			switch (simInstance.CurrentSimulator.Frame.Board.TurnColor)
			{
				case PlayerColors.White:
					beardOff = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
					moveCount = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.MoveCount;
					break;
				case PlayerColors.Black:
					beardOff = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;
					moveCount = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.MoveCount;


					break;
				default:
					break;
			}

			if (beardOff != 0 || moveCount == 0)
				return;

			for (int i = 0; i < pvmInstance.Points.Length; ++i)
			{
				PointVisualizer pv = pvmInstance.Points[i];
				if (pv.pointBeeds == null || pv.pointBeeds.Count == 0)
					continue;
				MoveInfo[] ef = Logic.GetPossibleBoardToBoardMoves(simInstance.CurrentSimulator.Frame.Board, pv.PointData.ID);
				MoveInfo[] ef1 = Logic.GetPossibleBearedOffMoves(simInstance.CurrentSimulator.Frame.Board, pv.PointData.ID);


				if ((ef == null || ef.Length == 0) && (ef1 == null || ef1.Length == 0))
					continue;
				Beed bd = pv.pointBeeds[pv.pointBeeds.Count - 1];
				possibleBeeds.Add(bd);
				bd.GlowObject.gameObject.SetActive(true);
			}

		}

		private void HideBeedGlow(bool ClearList = false)
		{
			for (int i = 0; i < possibleBeeds.Count; ++i)
				possibleBeeds[i].GlowObject.SetActive(false);

			if (ClearList)
				possibleBeeds.Clear();
		}

		private void Instance_OnDiceRolledFinished()
		{

			ShowBeedGlow();
			ShowPossibleBarToBoard();
		}

		private void Instance_OnGameFinished(PlayerColors WinnerColor, int Score)
		{
			Dice.Instance.OnDiceRolledFinished -= Instance_OnDiceRolledFinished;
			IsGameStarted = false;
			HideBeedGlow();
		}


		private void Instance_OnBoardToBoardMove(Identifier From, Identifier To)
		{
			pvmInstance.HidePossibleMoves();
			pvmInstance.BoardToBoardMove(From, To);
			ShowBeedGlow();
		}

		private void Instance_OnBoardToBarMove(Identifier From)
		{
			pvmInstance.BoardToBarMove(From);
			ShowBeedGlow();
			ShowPossibleBarToBoard();
		}

		private void Instance_OnBearedOff(Identifier From)
		{
			pvmInstance.BeardOff(From);
			pvmInstance.DeactiveBeardedOffHighlight();
			ShowBeedGlow();
		}


		private void Instance_OnBarToBoardMove(Identifier To)
		{
			pvmInstance.BarToBoardMove(To);
			ShowBeedGlow();
		}


		private void OnUndoEventClick()
		{
			ResetPossibleMoves();

			movesEvents.Clear();
			SimulationManager.Instance.UndoActions();
			ShowBeedGlow();
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
				SelectedBead = hit.transform.gameObject.GetComponentInParent<PointVisualizer>();
				if (SelectedBead == null)
					selectBar = hit.transform.gameObject.GetComponentInParent<BarOff>();

				if ((beardOff == 0) && tempBead != null && SelectedBead != null && tempBead.PointData.ID != SelectedBead.PointData.ID && possibleMoves.Count != 0)
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
					
					if (SelectedBead != null)
						for (int i = 0; i < possibleMoves.Count; ++i)
						{
							if (SelectedBead.PointData.ID != possibleMoves[i].To.ID)
								continue;

							MoveTo(null, SelectedBead.PointData);
							SelectedBead = tempBead = null;
							break;
						}
					return;
				}
				else if ((GetBeadOutofBase - beardedOff) == 0 && selectBar != null && tempBead != null)
				{
					
					MoveTo(tempBead.PointData, null);
					selectBar = null;
					SelectedBead = tempBead = null;
					return;
				}

				ResetPossibleMoves();
				HideBeedGlow(true);
				if (SelectedBead != null && SelectedBead.PointData.CheckerCount != 0 && SelectedBead.PointData.Color == simInstance.CurrentSimulator.Frame.Board.TurnColor)
				{

					tempBead = null;

					FindPossibleMoves();
					FindPossibleBearedOff();
					pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());

					return;
				}
			}


			ShowBeedGlow();
			ShowPossibleBarToBoard();
			SelectedBead = null;
		}

		private void ResetPossibleMoves()
		{
			possibleMoves.Clear();
			pvmInstance.HidePossibleMoves();
			pvmInstance.DeactiveBeardedOffHighlight();
		}

		private void FindPossibleBearedOff()
		{
			if (SelectedBead == null)
				return;
			MoveInfo[] mi = Logic.GetPossibleBearedOffMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBead.PointData.ID);

			if (mi == null || mi.Length == 0)
				return;
			pvmInstance.ActiveBeardedOffHighlight();

		}

		private void FindPossibleBarToBoardMoves()
		{
			possibleMoves.AddRange(Logic.GetTotalPossibleBarToBoardMoves(simInstance.CurrentSimulator.Frame.Board));
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
			movesEvents.Add(new TableEvent(new BearOffEvent(From), IsSendBywetWork));
			simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1].Event);
		}

		public void BoardToBoardMoveEvent(Identifier From, Identifier To, bool IsSendByNetwork = false)
		{
			ResetMyActions(IsSendByNetwork);
			movesEvents.Add(new TableEvent(new BoardToBoardMoveEvent(From, To), IsSendByNetwork));
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
						--i;
					}
				}
			}
		}

		public void OnChangeTurn(bool IsRecivedFromNetwork = false)
		{
			ResetMyActions(IsRecivedFromNetwork);

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



			ResetPossibleMoves();
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

}
#else
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

        public WhiteBeadPool WhiteBeads = null;
        public BlackBeadPool BlackBeads = null;
        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<TableEvent> movesEvents = new List<TableEvent>();
        private SimulationManager simInstance = null;
        private PointVisualizerManager pvmInstance = null;
        private List<Beed> possibleBeeds = new List<Beed>();
        private BarOff selectBar;

        private void Awake()
        {
            simInstance = SimulationManager.Instance;
            pvmInstance = PointVisualizerManager.Instance;
            WhiteBeads = new WhiteBeadPool();
            BlackBeads = new BlackBeadPool();
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
        }


        private void Instance_OnTableReady()
        {
            Dice.Instance.OnSelectedDiceChanged += OnSelectedDiceChanged;
            Dice.Instance.OnDiceRolledFinished += Instance_OnDiceRolledFinished;
            IsGameStarted = true;
        }

        private void OnDisable()
        {
            if (Tap.Instance != null)
                Tap.Instance.OnTapBegin -= OnTap;
            InGameMenu.OnChangeTurnEventClick -= OnChangeTurn;
            InGameMenu.OnUndoEventClick -= OnUndoEventClick;
            Dice.Instance.OnSelectedDiceChanged -= OnSelectedDiceChanged;
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

        private void ShowPossibleBarToBoard()
        {
            ResetPossibleMoves();
            if (simInstance.CurrentSimulator.Frame.Board.TurnColor != simInstance.YourColor)
                return;
            int beardOff = 0;

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

            if (beardOff != 0)
            {
                FindPossibleBarToBoardMoves();
            }
            pvmInstance.ShowPossibleMoves(possibleMoves.ToArray());
        }

        private void ShowBeedGlow()
        {
            HideBeedGlow(true);

            if (!Dice.Instance.IsDiceRolled ||simInstance.CurrentSimulator.Frame.Board.TurnColor != simInstance.YourColor)
                return;

            int beardOff = 0;
            int moveCount = 0;
            switch (simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                case PlayerColors.White:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    moveCount = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.MoveCount;
                    break;
                case PlayerColors.Black:
                    beardOff = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;
                    moveCount = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.MoveCount;
                    break;
                default:
                    break;
            }

            if (moveCount == 0)
                return;

            if (beardOff == 0)
            {
                List<MoveInfo> ef = new List<MoveInfo>();

        
                ef.AddRange(Logic.GetPossibleBoardToBoardMoves(simInstance.CurrentSimulator.Frame.Board, Dice.Instance.SelectedDice));
                ef.AddRange(Logic.GetPossibleBearedOffMoves(simInstance.CurrentSimulator.Frame.Board, Dice.Instance.SelectedDice));
                if ((ef == null || ef.Count == 0))
                    return;
                for (int i = 0; i < pvmInstance.Points.Length; ++i)
                {
                    PointVisualizer pv = pvmInstance.Points[i];
                    if (pv.pointBeeds == null || pv.pointBeeds.Count == 0)
                        continue;
                    for (int j = 0; j < ef.Count; ++j)
                    {
                        MoveInfo mi = ef[j];
                        if (mi.From.ID != pv.PointData.ID)
                            continue;

                        Beed bd = pv.pointBeeds[pv.pointBeeds.Count - 1];
                        if (!possibleBeeds.Contains(bd))
                            possibleBeeds.Add(bd);
                        bd.GlowObject.gameObject.SetActive(true);
                        break;
                    }
                }

            }
            else
            {
                for (int i = pvmInstance.ExtraBar.Length / 2; i < pvmInstance.ExtraBar.Length; ++i)
                {
                    if (pvmInstance.ExtraBar[i].Color != simInstance.YourColor)
                        continue;

                    MoveInfo[] mi = Logic.GetPossibleBarToBoardMoves(SimulationManager.Instance.CurrentSimulator.Frame.Board, Dice.Instance.SelectedDice);
                    if (mi == null || mi.Length == 0)
                        break;
                    BarOff bo = pvmInstance.ExtraBar[i];
                    Beed bd = bo.pointBeeds[bo.pointBeeds.Count - 1];
                    possibleBeeds.Add(bd);
                    bd.GlowObject.gameObject.SetActive(true);
                }
            }

        }

        private void HideBeedGlow(bool ClearList = false)
        {
            for (int i = 0; i < possibleBeeds.Count; ++i)
                possibleBeeds[i].GlowObject.SetActive(false);

            if (ClearList)
                possibleBeeds.Clear();
        }

        private void Instance_OnDiceRolledFinished()
        {
            //  AutoMove();
            ShowBeedGlow();
            ShowPossibleBarToBoard();
        }

        private void Instance_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, int Score)
        {
            Dice.Instance.OnSelectedDiceChanged -= OnSelectedDiceChanged;
            Dice.Instance.OnDiceRolledFinished -= Instance_OnDiceRolledFinished;
            IsGameStarted = false;
            HideBeedGlow();
        }

        private void Instance_OnBoardToBoardMove(Identifier From, Identifier To)
        {
            pvmInstance.HidePossibleMoves();
            pvmInstance.BoardToBoardMove(From, To);
            Dice.Instance.SetDiceState();
            ShowBeedGlow();
        }

        private void Instance_OnBoardToBarMove(Identifier From)
        {
            pvmInstance.BoardToBarMove(From);
            Dice.Instance.SetDiceState();

            ShowBeedGlow();
            ShowPossibleBarToBoard();
        }

        private void Instance_OnBearedOff(Identifier From)
        {
            pvmInstance.BeardOff(From);
            pvmInstance.DeactiveBeardedOffHighlight();
            Dice.Instance.SetDiceState();

            ShowBeedGlow();
        }

        private void Instance_OnBarToBoardMove(Identifier To)
        {
            pvmInstance.BarToBoardMove(To);
            Dice.Instance.SetDiceState();

            ShowBeedGlow();
        }

        private void OnUndoEventClick()
        {
            ResetPossibleMoves();

            movesEvents.Clear();
            SimulationManager.Instance.UndoActions();
            Dice.Instance.SetDiceState();
            ShowBeedGlow();
        }

        private void OnSelectedDiceChanged()
        {
            ResetPossibleMoves();
            ShowBeedGlow();
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
            {
                return;
            }
            selectBar = null;
            SelectedBead = null;
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

            if (hit.collider == null)
            {
                return;
            }


            SelectedBead = hit.transform.GetComponentInParent<PointVisualizer>();


            if (SelectedBead != null && beardOff == 0 && ExecuteBoardToBoardMove())
            {
                return;
            }

            selectBar = hit.transform.GetComponent<BarOff>();
            if (beardOff != 0 && selectBar != null && selectBar.Color == simInstance.YourColor && (selectBar.BarSide == BarOff.Side.Down || selectBar.BarSide == BarOff.Side.UP))
            {
                MoveInfo[] mi = Logic.GetPossibleBarToBoardMoves(SimulationManager.Instance.CurrentSimulator.Frame.Board, Dice.Instance.SelectedDice);

                if (mi == null || mi.Length == 0)
                {
                    return;
                }

                MoveTo(null, mi[0].To);
                return;
            }


            if (SelectedBead != null && (GetBeadOutofBase - beardedOff) == 0)
            {

                MoveInfo[] mi = Logic.GetPossibleBearedOffMoves(SimulationManager.Instance.CurrentSimulator.Frame.Board, SelectedBead.PointData.ID);

                if (mi == null || mi.Length == 0)
                {
                    return;
                }

                MoveTo(mi[0].From, null);
                return;
            }

        }

        private bool ExecuteBoardToBoardMove()
        {
            // MoveInfo[] mi = Logic.GetPossibleBoardToBoardMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBead.PointData.ID);

            MoveInfo[] mi = Logic.GetPossibleBoardToBoardMoves(simInstance.CurrentSimulator.Frame.Board, Dice.Instance.SelectedDice);

            if (mi == null || mi.Length == 0)
                return false;

            int dir = Utilities.GetDirection(simInstance.YourColor);
            for (int i = 0; i < mi.Length; ++i)
            {

                MoveInfo mit = mi[i];
              
                int move = 0;
                if (dir < 0)
                    move = SelectedBead.PointData.Index - mit.To.Index;
                else
                    move = mit.To.Index - SelectedBead.PointData.Index;
                if (move != Dice.Instance.SelectedDice)
                    continue;
            
                MoveTo(SelectedBead.PointData, mit.To);
                return true;
            }

            return false;
        }

        private void ResetPossibleMoves()
        {
            possibleMoves.Clear();
            pvmInstance.HidePossibleMoves();
            pvmInstance.DeactiveBeardedOffHighlight();
        }

        private void FindPossibleBearedOff()
        {
            if (SelectedBead == null)
                return;
            MoveInfo[] mi = Logic.GetPossibleBearedOffMoves(simInstance.CurrentSimulator.Frame.Board, SelectedBead.PointData.ID);

            if (mi == null || mi.Length == 0)
                return;
            pvmInstance.ActiveBeardedOffHighlight();

        }

        private void FindPossibleBarToBoardMoves()
        {
            possibleMoves.AddRange(Logic.GetTotalPossibleBarToBoardMoves(simInstance.CurrentSimulator.Frame.Board));
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
            movesEvents.Add(new TableEvent(new BearOffEvent(From), IsSendBywetWork));
            simInstance.SendCurrentEvent(movesEvents[movesEvents.Count - 1].Event);
        }

        public void BoardToBoardMoveEvent(Identifier From, Identifier To, bool IsSendByNetwork = false)
        {
            ResetMyActions(IsSendByNetwork);
            movesEvents.Add(new TableEvent(new BoardToBoardMoveEvent(From, To), IsSendByNetwork));
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
                        --i;
                    }
                }
            }
        }

        public void OnChangeTurn(bool IsRecivedFromNetwork = false)
        {
            ResetMyActions(IsRecivedFromNetwork);

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



            ResetPossibleMoves();
        }

        private void AutoMove()
        {
            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
                return;


            int moveCount = 0;
            switch (simInstance.YourColor)
            {
                case PlayerColors.White:
                    moveCount = simInstance.Board.WhitePlayer.MoveCount;
                    break;
                case PlayerColors.Black:
                    moveCount = simInstance.Board.BlackPlayer.MoveCount;
                    break;
                default:
                    break;
            }

            if (Logic.GetTotalPossibleMoveCount(simInstance.CurrentSimulator.Frame.Board) > moveCount)
                return;

            MoveInfo[] mo = Logic.GetTotalPossibleMoves(simInstance.CurrentSimulator.Frame.Board);
            if (mo == null || mo.Length == 0)
                return;

            for (int i = 0; i < mo.Length; ++i)
            {
                MoveInfo mot = mo[i];
                MoveTo(mot.From, mot.To);
            }
            OnChangeTurn(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }

#endif
}