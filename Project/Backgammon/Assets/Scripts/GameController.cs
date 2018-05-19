using GameServer.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviorBase
{
	public static GameController Instance
	{
		get;
		private set;
	}

	public class InGameController
	{
		public const float COUNTDOWN = 5.0F;

		public delegate void CountdownFinishedEventHandler();

		public enum Sides
		{
			Your,
			Opponent
		}

		private Text dice1Text = null;
		private Text dice2Text = null;
		private Text countdownText = null;
		private GameObject turnSign = null;

		private float finishCountdown = 0.0F;
		private bool isCountdownRunning = false;

		public event CountdownFinishedEventHandler CountdownFinished;

		public Sides Side
		{
			get;
			private set;
		}

		public int Dice1
		{
			set { dice1Text.text = value.ToString(); }
		}

		public int Dice2
		{
			set { dice2Text.text = value.ToString(); }
		}

		public bool IsActive
		{
			get { return turnSign.activeSelf; }
		}




		public InGameController(Sides Side)
		{
			this.Side = Side;

			dice1Text = GameObject.Find(Side + "Dice1Text").GetComponent<Text>();
			dice2Text = GameObject.Find(Side + "Dice2Text").GetComponent<Text>();

			turnSign = GameObject.Find(Side + "TurnSign");

			countdownText = GameObject.Find(Side + "Countdown").GetComponent<Text>();
			StopCountdown();
		}

		public void Update()
		{
			if (isCountdownRunning)
			{
				float remainTime = Mathf.Max(0, finishCountdown - Time.realtimeSinceStartup);
				countdownText.text = Mathf.Ceil(remainTime) + "s";

				if (remainTime == 0.0F)
					StopCountdown();
			}
		}

		public void StartCountdown()
		{
			turnSign.SetActive(true);
			finishCountdown = Time.realtimeSinceStartup + COUNTDOWN;
			isCountdownRunning = true;
			countdownText.gameObject.SetActive(true);
		}

		public void StopCountdown()
		{
			turnSign.SetActive(false);
			isCountdownRunning = false;
			countdownText.gameObject.SetActive(false);
			OnCountdownFinished();
		}

		private void OnCountdownFinished()
		{
			if (CountdownFinished != null)
				CountdownFinished();
		}
	}

	private GameObject loadingCanvas = null;
	private GameObject mainMenuCanvas = null;
	private GameObject inGameCanvas = null;

	public InGameController YourController
	{
		get;
		private set;
	}

	public InGameController OpponentController
	{
		get;
		private set;
	}

	protected override void Awake()
	{
		Instance = this;

		base.Awake();

		loadingCanvas = GameObject.Find("LoadingCanvas");
		mainMenuCanvas = GameObject.Find("MenuCanvas");
		inGameCanvas = GameObject.Find("InGameCanvas");

		YourController = new InGameController(InGameController.Sides.Your);
		OpponentController = new InGameController(InGameController.Sides.Opponent);

		YourController.CountdownFinished += YourHUDController_CountdownFinished;
		OpponentController.CountdownFinished += OpponentHUDController_CountdownFinished;

		HideMainMenu();
		HideInGameCanvas();
	}

	protected override void Start()
	{
		base.Start();

		NetworkManager.Instance.Connect();

		NetworkManager.Instance.RegisterMessageTypeCallback(MessageTypes.MatchFound, OnMatchFound);
	}

	protected override void Update()
	{
		base.Update();

		YourController.Update();
		OpponentController.Update();
	}

	public void HideLoadingWindow()
	{
		loadingCanvas.SetActive(false);
	}

	public void ShowMainMenu()
	{
		mainMenuCanvas.SetActive(true);
	}

	public void HideMainMenu()
	{
		mainMenuCanvas.SetActive(false);
	}

	public void ShowInGameCanvas()
	{
		inGameCanvas.SetActive(true);
	}

	public void HideInGameCanvas()
	{
		inGameCanvas.SetActive(false);
	}

	private void OnMatchFound(Dictionary<byte, object> Parameters)
	{
		HideMainMenu();
		ShowInGameCanvas();

		int yourDice = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice1);
		int opponentDice = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice2);

		YourController.Dice1 = yourDice;
		OpponentController.Dice1 = opponentDice;

		BoardManager.Instance.ResetAllLines();

		if (yourDice > opponentDice)
			YourController.StartCountdown();
		else
			OpponentController.StartCountdown();
	}

	private void YourHUDController_CountdownFinished()
	{
		OpponentController.StartCountdown();
	}

	private void OpponentHUDController_CountdownFinished()
	{
		YourController.StartCountdown();
	}
}