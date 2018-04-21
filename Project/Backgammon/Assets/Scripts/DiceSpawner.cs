using UnityEngine;

public class DiceSpawner : MonoBehaviorBase
{
	private GameObject dice1 = null;
	private GameObject dice2 = null;

	private Animation dice1Anim = null;
	private Animation dice2Anim = null;

	public AnimationClip Clip1 = null;
	public AnimationClip Clip2 = null;

	protected override void Awake()
	{
		base.Awake();

		GameObject dicePrefab = Resources.Load<GameObject>("Prefabs/Dice");
		dice1 = GameObject.Instantiate(dicePrefab);
		dice2 = GameObject.Instantiate(dicePrefab);

		dice1.transform.parent = transform;
		dice2.transform.parent = transform;

		dice1.transform.localPosition = Vector3.zero;
		dice2.transform.localPosition = Vector3.zero;

		dice1.SetActive(false);
		dice2.SetActive(false);

		dice1Anim = dice1.GetComponent<Animation>();
		dice2Anim = dice2.GetComponent<Animation>();

		Spawn();
	}

	public void Spawn()
	{
		//dice1.SetActive(true);
		//dice1Anim.clip = Clip1;
		//dice1Anim.Play();

		//dice2.SetActive(true);
		//dice2Anim.clip = Clip2;
		//dice2Anim.Play();
	}
}