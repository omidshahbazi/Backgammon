using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviorBase
{
	public static BoardManager Instance
	{
		get;
		private set;
	}

	private class BeadLineMap : Dictionary<int, BeadLine>
	{ }

	private BeadLineMap beadLines = new BeadLineMap();

	public static Bead.Colors YourColor
	{
		get { return Bead.Colors.Black; }
	}

	protected override void Awake()
	{
		base.Awake();

		Instance = this;
	}

	public void AddBeadLine(BeadLine Line)
	{
		Debug.Assert(!beadLines.ContainsKey(Line.Number));

		beadLines[Line.Number] = Line;
	}

	public BeadLine GetNextLine(BeadLine Line, int Count)
	{
		Debug.Assert(Line != null);
		Debug.Assert(Count != 0);

		int number = Line.Number + Count;

		if (!beadLines.ContainsKey(number))
			return null;

		return beadLines[number];
	}

	public void ResetAllLines()
	{
		for (int i = 1; i <= beadLines.Count; ++i)
			beadLines[i].Reset();
	}
}