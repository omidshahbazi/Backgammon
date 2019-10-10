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

        public class SnapShot
        {
            public void Clone(Simulator OrginSimulator, Simulator TempSimulator)
            {

                SerializerVisitor Serializer = new SerializerVisitor();
                OrginSimulator.Frame.Visit(Serializer);
                TempSimulator.SetFrame(Deserializer.DeserializeFrameData(Serializer.Data));
                SerializerVisitor configSerializer = new SerializerVisitor();
                TempSimulator.Config.Seed = OrginSimulator.Config.Seed;
                TempSimulator.Config.Random = OrginSimulator.Config.Random;

            }
        }

        public Simulator CurrentSimulator
        {
            get;
            private set;
        }

        public BoardData Board
        {
            get { return simulator.Frame.Board; }
        }


        public TableManager TableManager
        {
            get;
            private set;
        }


        private Simulator simulator = null;
        private SessionSerializer serializer = null;


        private SnapShot shot = null;

        public void SendEvent(EventBase Event)
        {
            //if (Event is FinishTurnEvent)
            //    simulator.Frame.Board.BlackPlayer.MoveCount = simulator.Frame.Board.WhitePlayer.MoveCount = 0;

            simulator.SendEvent(Event);

            serializer.SerializeFullStep(simulator.Frame);
        }

        public void SendCurrentEvent(EventBase Event)
        {
            //if (Event is FinishTurnEvent)
            //    CurrentSimulator.Frame.Board.BlackPlayer.MoveCount = CurrentSimulator.Frame.Board.WhitePlayer.MoveCount = 0;
            CurrentSimulator.SendEvent(Event);

            // serializer.SerializeFullStep(CurrentSimulator.Frame);
        }


        public void ResetGame(int Seed = 0)
        {
            simulator.Reset(Seed);
            CurrentSimulator.Reset(Seed);
            //These lines used to for the tests
            //Simulator.Frame.Board.TurnDice.Dice1 = Simulator.Frame.Board.TurnDice.Dice2 = 2;
            //Simulator.Frame.Board.TurnDice.AreSame = true;
            //Simulator.Frame.Board.BlackPlayer.BarCheckerCount = 5;
            //Simulator.Frame.Board.WhitePlayer.BarCheckerCount = 5;
            shot.Clone(simulator, CurrentSimulator);

            serializer.SerializeConfigState(simulator.Config);
            serializer.SerializeInitialState(simulator.Frame);

        }



        private void Awake()
        {
            FileSystem.DataPath = Application.dataPath + "\\..\\MemoryCard\\";

            serializer = new SessionSerializer();

            if (TableManager == null)
                TableManager = TableManager.Instance;
            if (simulator == null)
                simulator = new Simulator();
            if (CurrentSimulator == null)
                CurrentSimulator = new Simulator();
            if (shot == null)
                shot = new SnapShot();
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
            CurrentSimulator.OnTurnChanged += Simulator_OnTurnChanged;
            CurrentSimulator.OnBoardToBoardMove += Simulator_OnBoardToBoardMove;
            CurrentSimulator.OnBarToBoardMove += Simulator_OnBarToBoardMove;
            CurrentSimulator.OnBearedOff += Simulator_OnBearedOff;
            CurrentSimulator.OnBoardToBarMove += Simulator_OnBoardToBarMove;
            CurrentSimulator.OnGameFinished += Simulator_OnGameFinished;
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
            shot.Clone(simulator, CurrentSimulator);
            OnActionsUndo?.Invoke();
        }


        private void Simulator_OnTurnChanged()
        {
            shot.Clone(simulator, CurrentSimulator);
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