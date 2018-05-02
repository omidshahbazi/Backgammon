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

	private class InGameHUDController
	{
		public const float COUNTDOWN = 30.0F;

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

		public Sides Side
		{
			get;
			private set;
		}

		public int Dice1
		{
			set { dice1Text.text = value.ToString(); }
		}

		public bool TurnSignEnabled
		{
			set { turnSign.SetActive(value); }
		}

		public InGameHUDController(Sides Side)
		{
			this.Side = Side;

			dice1Text = GameObject.Find(Side + "Dice1Text").GetComponent<Text>();
			dice2Text = GameObject.Find(Side + "Dice2Text").GetComponent<Text>();

			turnSign = GameObject.Find(Side + "TurnSign");
			TurnSignEnabled = false;

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
			finishCountdown = Time.realtimeSinceStartup + COUNTDOWN;
			isCountdownRunning = true;
			countdownText.gameObject.SetActive(true);
		}

		public void StopCountdown()
		{
			countdownText.gameObject.SetActive(false);
		}
	}

	private GameObject loadingCanvas = null;
	private GameObject mainMenuCanvas = null;
	private GameObject inGameCanvas = null;

	private InGameHUDController yourHUDController = null;
	private InGameHUDController opponentHUDController = null;

	protected override void Awake()
	{
		Instance = this;

		base.Awake();

		loadingCanvas = GameObject.Find("LoadingCanvas");
		mainMenuCanvas = GameObject.Find("MenuCanvas");
		inGameCanvas = GameObject.Find("InGameCanvas");

		yourHUDController = new InGameHUDController(InGameHUDController.Sides.Your);
		opponentHUDController = new InGameHUDController(InGameHUDController.Sides.Opponent);

		HideMainMenu();
		HideInGameCanvas();
	}

	protected override void Start()
	{
		base.Start();

		NetworkManager.Instance.Connect();

		NetworkManager.Instance.RegisterMessageTypeCallback(GameServer.Common.MessageTypes.MatchFound, OnMatchFound);
	}

	protected override void Update()
	{
		base.Update();

		yourHUDController.Update();
		opponentHUDController.Update();
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

		yourHUDController.Dice1 = yourDice;
		opponentHUDController.Dice1 = opponentDice;

		bool isYourTurn = (yourDice > opponentDice);
		yourHUDController.TurnSignEnabled = isYourTurn;
		opponentHUDController.TurnSignEnabled = !isYourTurn;

		if (isYourTurn)
			yourHUDController.StartCountdown();
		else
			opponentHUDController.StartCountdown();
	}
}