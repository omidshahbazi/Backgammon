using System;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviorBase : MonoBehaviour
{
	private class DelayedCallInfo
	{
		public Action Action
		{
			get;
			private set;
		}

		public float ActionTime
		{
			get;
			private set;
		}

		public DelayedCallInfo(Action Action, float ActionTime)
		{
			this.Action = Action;
			this.ActionTime = ActionTime;
		}
	}

	private List<DelayedCallInfo> delayedCalls = new List<DelayedCallInfo>();

	new public Transform transform
	{
		get;
		private set;
	}

	protected virtual void Awake()
	{
		transform = GetComponent<Transform>();
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void Update()
	{
		for (int i = 0; i < delayedCalls.Count; ++i)
		{
			DelayedCallInfo info = delayedCalls[i];

			if (info.ActionTime > Time.time)
				continue;

			info.Action();

			delayedCalls.RemoveAt(i--);
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D Collider)
	{
	}

	public void DelayedCall(Action Action, float Seconds)
	{
		delayedCalls.Add(new DelayedCallInfo(Action, Time.time + Seconds));
	}
}
