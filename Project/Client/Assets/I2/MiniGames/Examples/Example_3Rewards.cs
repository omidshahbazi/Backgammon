using UnityEngine;
using System.Collections;
using I2.MiniGames;

public class Example_3Rewards : MonoBehaviour 
{
	public MiniGame _Game;
	public UnityEventString _OnResult = new UnityEventString();

	public void CollectReward( int Index )
	{
		int count = _Game.mRewards.Count;
		int id1 = (Index+count-1)%count;  MiniGame_Reward r1 = _Game.mRewards [id1];
		int id2 = Index;   			MiniGame_Reward r2 = _Game.mRewards [id2];
		int id3 = (Index+1)%count; 	MiniGame_Reward r3 = _Game.mRewards [id3];
		string result = string.Format ("{0}({1}), {2}({3}), {4}({5})", id1, r1.name, id2, r2.name, id3, r3.name);

		_OnResult.Invoke (result);
	}
}
