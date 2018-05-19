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

	private bool HasBead
	{
		get { return (beads.Count != 0); }
	}

	private Bead.Colors CurrentColor
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
		if (!GameController.Instance.YourController.IsActive)
			return;

		if (InputWrapper.LastObject != gameObject)
			return;

		BeadLine nextLine = BoardManager.Instance.GetNextLine(this);

		if (nextLine == null)
			return;

		if (nextLine.HasBead && nextLine.CurrentColor != cur)
	}

	public void Add(Bead.Colors Color)
	{
		GameObject obj = GameObject.Instantiate(Color == Bead.Colors.White ? whiteBeadPrefab : blackBeadPrefab);
		obj.transform.parent = transform;
		obj.transform.localPosition = Vector3.forward * (2.5F + (beads.Count * 5.0F)) * (Direction == Directions.Up ? 1 : -1);
		beads.Add(obj.GetComponent<Bead>());
	}

	public void Reset()
	{
		for (int i = 0; i < beads.Count; ++i)
			Destroy(beads[i]);

		for (int i = 0; i < InitialCount; ++i)
			Add(InitialColor);
	}
}