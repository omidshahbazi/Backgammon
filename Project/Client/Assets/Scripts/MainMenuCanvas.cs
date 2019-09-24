using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviorBase
{
	private Button matchMakingButton = null;

	protected override void Awake()
	{
		base.Awake();

		matchMakingButton = GameObject.Find("Button").GetComponent<Button>();
		matchMakingButton.onClick.AddListener(OnMatchMaking);
	}

	private void OnMatchMaking()
	{
		NetworkCommands.GetAMatch();
	}
}