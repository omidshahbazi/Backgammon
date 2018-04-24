using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviorBase
{
	private Text loadingPercentText = null;
	private int lastPercent = 0;
	private int percent = 1;

	protected override void Awake()
	{
		base.Awake();

		loadingPercentText = GameObject.Find("LoadingPercentText").GetComponent<Text>();
	}

	protected override void Start()
	{
		base.Start();

		GameController.Instance.HideMainMenu();
	}

	protected override void Update()
	{
		base.Update();

		if (percent == 1 && NetworkManager.Instance.IsConnected)
		{
			percent = 10;
		}
		else if (percent == 10)
		{
			percent = 11;
			UserManager.Instance.AuthenticateUser();
		}
		else if (percent == 11 && UserManager.Instance.Authenticated)
		{
			percent = 20;
		}
		else if (percent == 20 && UserManager.Instance.UserInfoReady)
		{
			percent = 21;
		}
		else if (percent == 21)
		{
			percent = 100;
		}

		if (lastPercent != percent)
		{
			lastPercent = percent;

			loadingPercentText.text = percent + "%";

			if (percent == 100)
			{
				gameObject.SetActive(false);
				GameController.Instance.HideLoadingWindow();
				GameController.Instance.ShowMainMenu();
			}
		}
	}
}