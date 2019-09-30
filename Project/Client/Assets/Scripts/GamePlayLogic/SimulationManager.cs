using ClientUtilities.Singleton;
//using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolled(int Dice1Value, int Dice2Value);
    public class SimulationManager : MonoBehaviorSingleton<SimulationManager>
    {
        public event DiceRolled OnDiceRolled = null;
        
        //public Simulator Simulator
        //{
        //    get;
        //    private set;
        //}

       
        //private void Awake()
        //{
        //    if (Simulator == null)
        //        Simulator = new Simulator();

        //    PointVisualizerManager pvmi = PointVisualizerManager.Instance;
        //    ResetGame(1134123);
        //    Simulator.OnTurnChanged += Simulator_OnTurnChanged;
         
        //}

   

        //private void Simulator_OnTurnChanged()
        //{
        //    OnDiceRolled?.Invoke(Simulator.Board.TurnDice.Dice1, Simulator.Board.TurnDice.Dice2);
        //}

        //public void ResetGame(int Seed = 0)
        //{
        //    Simulator.Reset(Seed);
        //    //simulator.Reset();
        //}
    }
}