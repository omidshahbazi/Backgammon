using UnityEngine;

public class GameController : MonoBehaviorBase
{
	public static GameController Instance
	{
		get;
		private set;
	}

	private GameObject loadingCanvas = null;

	protected override void Awake()
	{
		Instance = this;

		base.Awake();

		loadingCanvas = GameObject.Find("LoadingCanvas");
	}

	protected override void Start()
	{
		base.Start();

		NetworkManager.Instance.Connect();
	}

	public void HideLoadingWindow()
	{
		loadingCanvas.SetActive(false);
	}
}