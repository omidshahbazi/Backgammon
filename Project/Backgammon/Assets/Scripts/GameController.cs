using GameServer.Common;
using System;
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
		public const float COUNTDOWN = 10.0F;

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

			dice1Text.gameObject.SetActive(true);
			dice2Text.gameObject.SetActive(true);

		}

		public void StopCountdown()
		{
			turnSign.SetActive(false);
			isCountdownRunning = false;
			countdownText.gameObject.SetActive(false);

			dice1Text.gameObject.SetActive(false);
			dice2Text.gameObject.SetActive(false);

			OnCountdownFinished();
		}

		private void OnCountdownFinished()
		{
			if (CountdownFinished != null)
				CountdownFinished();
		}
	}

	public class MoveInfo
	{
		public int Dice
		{
			get;
			private set;
		}

		public BeadLine Line
		{
			get;
			private set;
		}

		public MoveInfo(int Dice)
		{
			this.Dice = Dice;
		}

		public void SetLine(BeadLine Line)
		{
			this.Line = Line;
		}
	}

	private GameObject loadingCanvas = null;
	private GameObject mainMenuCanvas = null;
	private GameObject inGameCanvas = null;
	private Button revertButton = null;
	private Button applyButton = null;

	private MoveInfo[] moveInfos = null;
	private int moveIndex = 0;

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

	public int CurrentMoveDice
	{
		get { return moveInfos[moveIndex].Dice; }
	}

	public bool HasMoreMove
	{
		get { return (moveIndex < moveInfos.Length); }
	}

	protected override void Awake()
	{
		Instance = this;

		base.Awake();

		loadingCanvas = GameObject.Find("LoadingCanvas");
		mainMenuCanvas = GameObject.Find("MenuCanvas");
		inGameCanvas = GameObject.Find("InGameCanvas");

		revertButton = GameObject.Find("RevertButton").GetComponent<Button>();
		revertButton.onClick.AddListener(OnRevertButton);
		applyButton = GameObject.Find("ApplyButton").GetComponent<Button>();
		applyButton.onClick.AddListener(OnApplyButton);

		YourController = new InGameController(InGameController.Sides.Your);
		OpponentController = new InGameController(InGameController.Sides.Opponent);

		YourController.CountdownFinished += YourHUDController_CountdownFinished;
		OpponentController.CountdownFinished += OpponentHUDController_CountdownFinished;

		HideMainMenu();
		HideInGameCanvas();

		UpdateButtons();
	}

	protected override void Start()
	{
		base.Start();

		NetworkManager.Instance.Connect();

		NetworkManager.Instance.RegisterMessageTypeCallback(MessageTypes.MatchFound, OnMatchFound);
		NetworkManager.Instance.RegisterMessageTypeCallback(MessageTypes.StopYourTurn, OnStopYourTurn);
		NetworkManager.Instance.RegisterMessageTypeCallback(MessageTypes.StartYourTurn, OnStartYourTurn);
		NetworkManager.Instance.RegisterMessageTypeCallback(MessageTypes.OtherSideQuit, OnOtherSideQuit);
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

	public void FinishMove(BeadLine FinalLine)
	{
		moveInfos[moveIndex].SetLine(FinalLine);

		if (!HasMoreMove)
			return;

		++moveIndex;

		UpdateButtons();
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
		{
			YourController.StartCountdown();

			SetYourDices(yourDice, opponentDice);
		}
		else
		{
			OpponentController.StartCountdown();

			OpponentController.Dice1 = yourDice;
			OpponentController.Dice2 = opponentDice;
		}
	}

	private void OnStopYourTurn(Dictionary<byte, object> Parameters)
	{
		int dice1 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice1);
		int dice2 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice2);

		OpponentController.Dice1 = dice1;
		OpponentController.Dice2 = dice2;

		OpponentController.StartCountdown();

		moveIndex = 0;
		UpdateButtons();
	}

	private void OnStartYourTurn(Dictionary<byte, object> Parameters)
	{
		int dice1 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice1);
		int dice2 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice2);

		SetYourDices(dice1, dice2);

		YourController.StartCountdown();
	}

	private void OnOtherSideQuit(Dictionary<byte, object> Parameters)
	{
		ShowMainMenu();
	}

	private void YourHUDController_CountdownFinished()
	{
		NetworkCommands.TimeoutReached();
	}

	private void OpponentHUDController_CountdownFinished()
	{
	}

	private void OnRevertButton()
	{
		for (int i = moveIndex - 1; i >= 0; --i)
		{
			MoveInfo info = moveInfos[i];

			BeadLine line = BoardManager.Instance.GetPrevtLine(info.Line, info.Dice);

			line.Add(info.Line.CurrentColor);
			info.Line.Remove();
		}

		moveIndex = 0;
		UpdateButtons();
	}

	private void OnApplyButton()
	{
	}

	private void SetYourDices(int Dice1, int Dice2)
	{
		YourController.Dice1 = Dice1;
		YourController.Dice2 = Dice2;

		bool isSame = (Dice1 == Dice2);

		moveInfos = new MoveInfo[(isSame ? 4 : 2)];

		if (isSame)
		{
			for (int i = 0; i < moveInfos.Length; ++i)
				moveInfos[i] = new MoveInfo(Dice1);
		}
		else
		{
			moveInfos[0] = new MoveInfo(Dice1);
			moveInfos[1] = new MoveInfo(Dice2);
		}

		moveIndex = 0;

		UpdateButtons();
	}

	private void UpdateButtons()
	{
		if (moveIndex == 0)
		{
			revertButton.gameObject.SetActive(false);
			applyButton.gameObject.SetActive(false);
		}
		else if (!HasMoreMove)
		{
			revertButton.gameObject.SetActive(true);
			applyButton.gameObject.SetActive(true);
		}
		else
		{
			revertButton.gameObject.SetActive(true);
			applyButton.gameObject.SetActive(false);
		}
	}
}