namespace GameServer.Common
{
	public enum ParameterTypes : byte
	{
		UDID = 0,
		MessageType,
		MessageNumber,
		FindMatchResult,
		Dice1,
		Dice2
	}

	public enum MessageTypes : int
	{
		Authenticate = 0,
		GetUserInfo,
		GetAMatch,
		MatchFound,
		StartYourTurn,
		StopYourTurn,
		MovesCompleted,
		TimeoutReached
	}
}