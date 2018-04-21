using System.Collections.Generic;
using UnityEngine;

public class BeadLine : MonoBehaviorBase
{
	public enum BeadColor
	{
		White = 0,
		Black
	}

	public enum Directions
	{
		Up = 0,
		Down
	}

	private GameObject whiteBeadPrefab = null;
	private GameObject blackBeadPrefab = null;
	private List<GameObject> beads = new List<GameObject>();

	public Directions Direction = Directions.Up;
	public int InitialCount = 0;
	public BeadColor InitialColor = BeadColor.White;

	protected override void Awake()
	{
		base.Awake();

		whiteBeadPrefab = Resources.Load<GameObject>("Prefabs/WhiteBead");
		blackBeadPrefab = Resources.Load<GameObject>("Prefabs/BlackBead");

		for (int i = 0; i < InitialCount; ++i)
			Add(InitialColor);

		InputWrapper.Tap += OnTap;
	}

	private void OnTap(Vector2 Position)
	{
		if (InputWrapper.LastObject != gameObject)
			return;

		//NetworkCommands.Get_Dice().Then((Parameters) =>
		//{

		//});
	}

	public void Add(BeadColor Color)
	{
		GameObject obj = GameObject.Instantiate(Color == BeadColor.White ? whiteBeadPrefab : blackBeadPrefab);
		obj.transform.parent = transform;
		obj.transform.localPosition = Vector3.forward * (2.5F + (beads.Count * 5.0F)) * (Direction == Directions.Up ? 1 : -1);
		beads.Add(obj);
	}
}