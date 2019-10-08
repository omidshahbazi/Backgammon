using ClientUtilities.Singleton;
using GameFramework.Common.FileLayer;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolled();
    public delegate void ActionsUndo();
    public delegate void BarToBoardMove(Identifier To);
    public delegate void BearedOff(Identifier From);
    public delegate void BoardToBarMove(Identifier From);
    public delegate void BoardToBoardMove(Identifier From, Identifier To);
    public delegate void GameFinished(PlayerColors WinnerColor, int Score);

    public class SimulationManager : MonoBehaviorSingleton<SimulationManager>
    {
        public event DiceRolled OnDiceRolled = null;
        public event ActionsUndo OnActionsUndo = null;
        public event BoardToBoardMove OnBoardToBoardMove = null;
        public event BarToBoardMove OnBarToBoardMove = null;
        public event BearedOff OnBearedOff = null;
        public event BoardToBarMove OnBoardToBarMove = null;
        public event GameFinished OnGameFinished = null;

        private Simulator simulator = null;
        private SessionSerializer serializer = null;

        public class SnapShot
        {
            public BoardData BoardData
            {
                get;
                private set;
            }

            public void Clone(BoardData BoardData)
            {
                this.BoardData = null;
                SerializerVisitor serializer = new SerializerVisitor();
                BoardData.Visit(serializer);
                this.BoardData = Deserializer.DeserializeBoardData(serializer.Data);
            }
        }

        public BoardData Board
        {
            get { return simulator.Frame.Board; }
        }

        public SnapShot Shot
        {
            get;
            private set;
        }

        public TableManager TableManager
        {
            get;
            private set;
        }


        public void SendEvent(EventBase Event)
        {
            simulator.SendEvent(Event);

            serializer.SerializeFullStep(simulator.Frame);
        }


        public void ResetGame(int Seed = 0)
        {
            simulator.Reset(Seed);
            //These lines used to for the tests
            //Simulator.Frame.Board.TurnDice.Dice1 = Simulator.Frame.Board.TurnDice.Dice2 = 2;
            //Simulator.Frame.Board.TurnDice.AreSame = true;
            //Simulator.Frame.Board.BlackPlayer.BarCheckerCount = 5;
            //Simulator.Frame.Board.WhitePlayer.BarCheckerCount = 5;
            Shot.Clone(simulator.Frame.Board);

            serializer.SerializeConfigState(simulator.Config);
            serializer.SerializeInitialState(simulator.Frame);

        }



        private void Awake()
        {
            serializer = new SessionSerializer();

            if (TableManager == null)
                TableManager = TableManager.Instance;
            if (simulator == null)
                simulator = new Simulator();
            if (Shot == null)
                Shot = new SnapShot();
            AddSimulatorEvents();
            ResetGame(1134123);

            PointVisualizerManager pvmi = PointVisualizerManager.Instance;

        }


        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.D))
            {
                FileSystem.Write("dump.bin", serializer.Data);
            }
        }

        private void AddSimulatorEvents()
        {
            simulator.OnTurnChanged += Simulator_OnTurnChanged;
            simulator.OnBoardToBoardMove += Simulator_OnBoardToBoardMove;
            simulator.OnBarToBoardMove += Simulator_OnBarToBoardMove;
            simulator.OnBearedOff += Simulator_OnBearedOff;
            simulator.OnBoardToBarMove += Simulator_OnBoardToBarMove;
            simulator.OnGameFinished += Simulator_OnGameFinished;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearSimulatorEvents();
        }

        private void ClearSimulatorEvents()
        {
            if (simulator == null)
                return;

            // To do remove all the event handler register to an event
        }


        public void UndoActions()
        {
            Shot.Clone(simulator.Frame.Board);
            OnActionsUndo?.Invoke();
        }


        private void Simulator_OnTurnChanged()
        {
            Shot.Clone(simulator.Frame.Board);
            OnDiceRolled?.Invoke();
        }

        private void Simulator_OnBarToBoardMove(Identifier To)
        {
            OnBarToBoardMove?.Invoke(To);
        }


        private void Simulator_OnBearedOff(Identifier From)
        {
            OnBearedOff?.Invoke(From);
        }

        private void Simulator_OnBoardToBoardMove(Identifier From, Identifier To)
        {
            OnBoardToBoardMove?.Invoke(From, To);
        }

        private void Simulator_OnBoardToBarMove(Identifier From)
        {
            OnBoardToBarMove?.Invoke(From);
        }

        private void Simulator_OnGameFinished(PlayerColors WinnerColor, int Score)
        {
            OnGameFinished?.Invoke(WinnerColor, Score);
        }

    }
}