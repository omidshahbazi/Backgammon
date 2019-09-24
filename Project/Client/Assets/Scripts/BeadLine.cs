using System.Collections.Generic;
using UnityEngine;

public class BeadLine : MonoBehaviorBase
{
	public enum Directions
	{
		Up = 0,
		Down
	}

	private GameObject whiteBeadPrefab = null;
	private GameObject blackBeadPrefab = null;
	private List<Bead> beads = new List<Bead>();

	public int Number;
	public Directions Direction = Directions.Up;
	public int InitialCount = 0;
	public Bead.Colors InitialColor = Bead.Colors.White;

	private int BeadCount
	{
		get { return beads.Count; }
	}

	public Bead.Colors CurrentColor
	{
		get
		{
			if (beads.Count == 0)
				return Bead.Colors.Black;

			return beads[0].Color;
		}
	}

	protected override void Awake()
	{
		base.Awake();

		whiteBeadPrefab = Resources.Load<GameObject>("Prefabs/WhiteBead");
		blackBeadPrefab = Resources.Load<GameObject>("Prefabs/BlackBead");

		Reset();

		InputWrapper.Tap += OnTap;
	}

	protected override void Start()
	{
		base.Start();

		BoardManager.Instance.AddBeadLine(this);
	}

	private void OnTap(Vector2 Position)
	{
		if (!GameController.Instance.YourController.IsActive || !GameController.Instance.HasMoreMove)
			return;

		if (InputWrapper.LastObject != gameObject)
			return;

		if (CurrentColor != BoardManager.YourColor)
			return;

		if (BeadCount == 0)
			return;

		BeadLine targetLine = BoardManager.Instance.GetNextLine(this, GameController.Instance.CurrentMoveDice);

		if (targetLine == null)
			return;

		if (targetLine.BeadCount != 0 && targetLine.CurrentColor != CurrentColor)
		{
			if (targetLine.BeadCount == 1)
				targetLine.Remove();
			else
				return;
		}

		targetLine.Add(CurrentColor);
		Remove();

		GameController.Instance.FinishMove(targetLine);
	}

	public void Add(Bead.Colors Color)
	{
		GameObject obj = GameObject.Instantiate(Color == Bead.Colors.White ? whiteBeadPrefab : blackBeadPrefab);
		obj.transform.parent = transform;
		obj.transform.localPosition = Vector3.forward * (2.5F + (beads.Count * 5.0F)) * (Direction == Directions.Up ? 1 : -1);
		beads.Add(obj.GetComponent<Bead>());
	}

	public void Remove()
	{
		int lastIndex = beads.Count - 1;
		Bead bead = beads[lastIndex];
		Destroy(bead.gameObject);
		beads.RemoveAt(lastIndex);
	}

	public void Reset()
	{
		for (int i = 0; i < beads.Count; ++i)
			Destroy(beads[i].gameObject);
		beads.Clear();

		for (int i = 0; i < InitialCount; ++i)
			Add(InitialColor);
	}
}