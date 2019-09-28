using ClientUtilities.Singleton;
using Simulation.Logic;

namespace Assets.Scripts.GamePlayLogic
{
	public class SimulationManager : MonoBehaviorSingleton<SimulationManager>
	{
		private Simulator simulator = null;

		private void Awake()
		{
			simulator = new Simulator();
			simulator.Reset(1134123);
		}
    }
}