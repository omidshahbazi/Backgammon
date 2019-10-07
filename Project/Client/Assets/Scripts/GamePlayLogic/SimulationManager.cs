using Assets.Scripts.ClientUtilities;
using ClientUtilities.Singleton;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
	public delegate void DiceRolled();
	public delegate void ActionsUndo();
	public class SimulationManager : MonoBehaviorSingleton<SimulationManager>
	{
		public event DiceRolled OnDiceRolled = null;
		public event ActionsUndo OnActionsUndo = null;

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

		public void UndoActions()
		{
			Shot.Clone(simulator.Frame.Board);
			OnActionsUndo?.Invoke();
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
			simulator.OnTurnChanged += Simulator_OnTurnChanged;
			ResetGame(1134123);

			PointVisualizerManager pvmi = PointVisualizerManager.Instance;

		}

		private void Update()
		{
			if (Input.GetKeyUp(KeyCode.D))
			{
				FileSystem.WriteBytes("dump.bin", serializer.Data);
			}
		}

		private void Simulator_OnTurnChanged()
		{
			Shot.Clone(simulator.Frame.Board);
			OnDiceRolled?.Invoke();
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
	}
}