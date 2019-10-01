using ClientUtilities.Singleton;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolled(int Dice1Value, int Dice2Value);
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
            Shot.Clone(Simulator.Board);
            OnActionsUndo?.Invoke();
        }

        private void Awake()
        {
            if (Simulator == null)
                Simulator = new Simulator();
            if (Shot == null)
                Shot = new SnapShot();

            ResetGame(1134123);
           
            Simulator.OnTurnChanged += Simulator_OnTurnChanged;
            PointVisualizerManager pvmi = PointVisualizerManager.Instance;
        }




        private void Simulator_OnTurnChanged()
        {
            OnDiceRolled?.Invoke(Simulator.Board.TurnDice.Dice1, Simulator.Board.TurnDice.Dice2);
            Shot.Clone(Simulator.Board);
        }

        public void ResetGame(int Seed = 0)
        {
            Simulator.Reset(Seed);
            Shot.Clone(Simulator.Board);
        }
    }
}