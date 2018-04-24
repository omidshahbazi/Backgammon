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

	private GameObject loadingCanvas = null;
	private GameObject mainMenuCanvas = null;
	private GameObject inGameCanvas = null;

	private Text yourDiceText = null;
	private Text otherDiceText = null;

	protected override void Awake()
	{
		Instance = this;

		base.Awake();

		loadingCanvas = GameObject.Find("LoadingCanvas");
		mainMenuCanvas = GameObject.Find("MenuCanvas");
		inGameCanvas = GameObject.Find("InGameCanvas");

		yourDiceText = GameObject.Find("YourDiceText").GetComponent<Text>();
		otherDiceText = GameObject.Find("OtherDiceText").GetComponent<Text>();

		HideMainMenu();
		HideInGameCanvas();
	}

	protected override void Start()
	{
		base.Start();

		NetworkManager.Instance.Connect();

		NetworkManager.Instance.RegisterMessageTypeCallback(GameServer.Common.MessageTypes.MatchFound, OnMatchFound);
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

		int dice1 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice1);
		int dice2 = ParameterHelper.GetParameter<int>(Parameters, ParameterTypes.Dice2);

		yourDiceText.text = NBidi.NBidi.LogicalToVisual(string.Format("تاس شما {0}", dice1));
		otherDiceText.text = NBidi.NBidi.LogicalToVisual(string.Format("تاس حریف {0}", dice2));
	}
}