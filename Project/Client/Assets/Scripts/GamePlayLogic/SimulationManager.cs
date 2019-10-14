using ClientUtilities.Singleton;
using GameFramework.Common.FileLayer;
using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolled();
    public delegate void ActionsUndo();
    public delegate void ReplayLoadingIsFailed();
    public delegate void ReplayIsReady();
    public delegate void ReplayEnd();
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
        public event ReplayEnd OnReplayEnd = null;
        public event ReplayIsReady OnReplayIsReady = null;
        public event ReplayLoadingIsFailed OnReplayIsLoadingFailed = null;

        public class SnapShot
        {
            public void Clone(Simulator OrginSimulator, Simulator TempSimulator)
            {

                SerializerVisitor Serializer = new SerializerVisitor();
                OrginSimulator.Frame.Visit(Serializer);
                TempSimulator.SetFrame(Deserializer.DeserializeFrameData(Serializer.Data));
                SerializerVisitor configSerializer = new SerializerVisitor();
                TempSimulator.Config.Seed = OrginSimulator.Config.Seed;
                TempSimulator.Config.Random = new GameFramework.Common.Utilities.Random(TempSimulator.Config.Seed);

            }
        }

        public class Replay
        {
            private SessionDeserializer deserializer = null;
            private ConfigData config = null;
            private FrameData frame = null;
            private List<FrameData> frames = new List<FrameData>();

            public Replay(byte[] Data,Simulator Simulator)
            {
                if (Data == null || Data.Length == 0)
                {
                    Instance.OnReplayIsLoadingFailed?.Invoke();
                    return;
                }
                deserializer = new SessionDeserializer(Data);
                config = deserializer.DeserializeConfigDataState();
                frame = deserializer.DeserializeInitialState();
                Utilities.InitializeBoard(config, frame.Board);

                FrameData stepFrame = null;
                while ((stepFrame = deserializer.DeserializeFullStep()) != null)
                    frames.Add(stepFrame);

                Instance.OnReplayIsReady?.Invoke();
            }

            //To Do make interval between frames
            public void SimulateReplay(Simulator Simulator)
            {
                if (frames == null || frames.Count == 0)
                    Instance.OnReplayEnd?.Invoke();

                Simulator.SetConfig(config);
                Simulator.SetFrame(frame);
                for (int i = 0; i < frames.Count; ++i)
                {
                    FrameData simulatedFrame = frames[i];

                    Simulator.SendEvent(simulatedFrame.Events[0]);
 
                }

                Instance.OnReplayEnd?.Invoke();
            }
        }


        public Simulator CurrentSimulator
        {
            get;
            private set;
        }

        public BoardData Board
        {
            get { return Simulator.Frame.Board; }
        }


        public TableManager TableManager
        {
            get;
            private set;
        }


        private Simulator Simulator = null;
      
        private SessionSerializer serializer = null;
        //private SessionSerializer serializer1 = null;
        private SnapShot shot = null;

        public void SendEvent(EventBase Event)
        {
            //if (Event is FinishTurnEvent)
            //    simulator.Frame.Board.BlackPlayer.MoveCount = simulator.Frame.Board.WhitePlayer.MoveCount = 0;

            Simulator.SendEvent(Event);

            serializer.SerializeFullStep(Simulator.Frame);
        }

        public void SendCurrentEvent(EventBase Event)
        {
            //if (Event is FinishTurnEvent)
            //    CurrentSimulator.Frame.Board.BlackPlayer.MoveCount = CurrentSimulator.Frame.Board.WhitePlayer.MoveCount = 0;
            CurrentSimulator.SendEvent(Event);

            //serializer1.SerializeFullStep(CurrentSimulator.Frame);
        }


        public void ResetGame(int Seed = 0)
        {
            Simulator.Reset(Seed);
            CurrentSimulator.Reset(Seed);
            //These lines used to for the tests
            //Simulator.Frame.Board.TurnDice.Dice1 = Simulator.Frame.Board.TurnDice.Dice2 = 2;
            //Simulator.Frame.Board.TurnDice.AreSame = true;
            //Simulator.Frame.Board.BlackPlayer.BarCheckerCount = 5;
            //Simulator.Frame.Board.WhitePlayer.BarCheckerCount = 5;
            //simulator.Frame.Board.BlackPlayer.
            shot.Clone(Simulator, CurrentSimulator);

            serializer.SerializeConfigState(Simulator.Config);
            serializer.SerializeInitialState(Simulator.Frame);
        }



        private void Awake()
        {
            FileSystem.DataPath = Application.dataPath + "\\..\\MemoryCard\\";

            serializer = new SessionSerializer();
            //  serializer1 = new SessionSerializer();
            if (TableManager == null)
                TableManager = TableManager.Instance;
            if (Simulator == null)
                Simulator = new Simulator();
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
                //FileSystem.Write("dumb2.bin", serializer1.Data);
            }

            //Replay Test
            if (Input.GetKeyUp(KeyCode.R))
            {
                Replay a = new Replay(File.ReadAllBytes("..\\Client\\MemoryCard\\dump.bin"), CurrentSimulator);
                a.SimulateReplay(CurrentSimulator);
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
            if (Simulator == null)
                return;

            // To do remove all the event handler register to an event
        }


        public void UndoActions()
        {
            shot.Clone(Simulator, CurrentSimulator);
            OnActionsUndo?.Invoke();
        }


        private void Simulator_OnTurnChanged()
        {
            shot.Clone(Simulator, CurrentSimulator);
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