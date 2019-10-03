using ClientUtilities.Singleton;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolled();
    public delegate void ActionsUndo();
    public class SimulationManager : MonoBehaviorSingleton<SimulationManager>
    {
        public event DiceRolled OnDiceRolled = null;
        public event ActionsUndo OnActionsUndo = null;
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
    
        public Simulator Simulator
        {
            get;
            private set;
        }

        public SnapShot Shot
        {
            get;
            private set;
        }

        public void UndoActions()
        {
            Shot.Clone(Simulator.Frame.Board);
            OnActionsUndo?.Invoke();
        }

        private void Awake()
        {
            if (Simulator == null)
                Simulator = new Simulator();
            if (Shot == null)
                Shot = new SnapShot();
            Simulator.OnTurnChanged += Simulator_OnTurnChanged;
            ResetGame(1134123);
           
            
            PointVisualizerManager pvmi = PointVisualizerManager.Instance;
        }




        private void Simulator_OnTurnChanged()
        {
            OnDiceRolled?.Invoke();
          
            Shot.Clone(Simulator.Frame.Board);
        }

        public void ResetGame(int Seed = 0)
        {
            Simulator.Reset(Seed);
            //Simulator.Frame.Board.TurnDice.Dice1 = Simulator.Frame.Board.TurnDice.Dice2 = 2;
            //Simulator.Frame.Board.TurnDice.AreSame = true;
            Shot.Clone(Simulator.Frame.Board);

        }
    }
}