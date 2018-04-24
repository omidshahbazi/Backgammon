public class UserManager : MonoBehaviorBase
{
	public static UserManager Instance
	{
		get;
		private set;
	}

	public bool Authenticated
	{
		get;
		private set;
	}

	public bool UserInfoReady
	{
		get;
		private set;
	}

	protected override void Awake()
	{
		Instance = this;

		base.Awake();
	}

	public void AuthenticateUser()
	{
		NetworkCommands.Authenticate().Then((Parameters) =>
		{
			Authenticated = true;
		});

		NetworkCommands.GetUserInfo().Then((Parameters) =>
		{
			UserInfoReady = true;
		});
	}
}