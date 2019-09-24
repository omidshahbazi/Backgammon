using UnityEngine;

static class InputWrapper
{
	private class InputController : MonoBehaviorBase
	{
		private const float DRAG_THRESHOLD = 0.1F;
		private const float DRAG_THRESHOLD_SQRD = DRAG_THRESHOLD * DRAG_THRESHOLD;

		private float tapBeginTime = 0.0F;
		private Vector2 prevPosition = Vector2.zero;
		private float prevTime = 0.0F;
		private bool isDraging = false;

		protected override void Update()
		{
			base.Update();

#if UNITY_EDITOR || UNITY_STANDALONE
			Vector2 position = Input.mousePosition;

			UpdateLastObject(position);

			if (Input.GetMouseButtonDown(0))
			{
				OnTapBegin(position);

				prevPosition = position;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				OnTapEnd(position);
				OnTap(position);

				if (isDraging)
				{
					OnDragEnd(position, Vector2.zero, 0.0F);
					isDraging = false;
				}
			}
			else if (Input.GetMouseButton(0))
			{
				Vector3 diff = prevPosition - position;

				if (diff.sqrMagnitude >= DRAG_THRESHOLD_SQRD)
				{
					if (!isDraging)
					{
						OnDragBegin(position, Vector2.zero, 0.0F);
						prevTime = Time.time;
						isDraging = true;
					}
				}

				if (isDraging && diff.sqrMagnitude != 0.0F)
				{
					OnDrag(position, diff.normalized, Time.time - prevTime);

					prevTime = Time.time;
					prevPosition = position;
				}
			}
#else
			if (Input.touchCount == 0)
				return;

			Touch touch = Input.GetTouch(0);
			Vector2 position = touch.position;

			UpdateLastObject(position);

			if (touch.phase == TouchPhase.Began)
			{
				OnTapBegin(position);

				prevPosition = position;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				OnTapEnd(position);
				OnTap(position);

				if (isDraging)
				{
					OnDragEnd(position, Vector2.zero, 0.0F);
					isDraging = false;
				}
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				Vector3 diff = prevPosition - position;

				if (diff.sqrMagnitude >= DRAG_THRESHOLD_SQRD)
				{
					if (!isDraging)
					{
						OnDragBegin(position, Vector2.zero, 0.0F);
						prevTime = Time.time;
						isDraging = true;
					}
				}

				if (isDraging && diff.sqrMagnitude != 0.0F)
				{
					OnDrag(position, diff.normalized, Time.time - prevTime);

					prevTime = Time.time;
					prevPosition = position;
				}
			}
#endif
		}

		private void OnTapBegin(Vector2 Position)
		{
			tapBeginTime = Time.time;

			if (InputWrapper.TapBegin != null)
				InputWrapper.TapBegin(Position);
		}

		private void OnTapEnd(Vector2 Position)
		{
			InputWrapper.HoldTime = Time.time - tapBeginTime;

			if (InputWrapper.TapEnd != null)
				InputWrapper.TapEnd(Position);
		}

		private void OnTap(Vector2 Position)
		{
			if (InputWrapper.Tap != null)
				InputWrapper.Tap(Position);
		}

		private void OnDragBegin(Vector2 Position, Vector2 Direction, float DeltaTime)
		{
			if (InputWrapper.DragBegin != null)
				InputWrapper.DragBegin(Position, Direction, DeltaTime);
		}

		private void OnDragEnd(Vector2 Position, Vector2 Direction, float DeltaTime)
		{
			if (InputWrapper.DragEnd != null)
				InputWrapper.DragEnd(Position, Direction, DeltaTime);
		}

		private void OnDrag(Vector2 Position, Vector2 Direction, float DeltaTime)
		{
			if (InputWrapper.Drag != null)
				InputWrapper.Drag(Position, Direction, DeltaTime);
		}

		private void UpdateLastObject(Vector2 Position)
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Position), out hit, 1000))
			{
				InputWrapper.LastObject = hit.transform.gameObject;
				LastWorldPosition = hit.point;
				LastWorldNormal = hit.normal;
				Hit = true;
				return;
			}

			InputWrapper.LastObject = null;
			LastWorldPosition = Vector3.zero;
			LastWorldNormal = Vector3.zero;
			Hit = false;
		}
	}

	public delegate void TapEventHandler(Vector2 Position);
	public delegate void DragEventHandler(Vector2 Position, Vector2 Direction, float DeltaTime);

	public static bool Hit
	{
		get;
		private set;
	}

	public static GameObject LastObject
	{
		get;
		private set;
	}

	public static Vector3 LastWorldPosition
	{
		get;
		private set;
	}

	public static Vector3 LastWorldNormal
	{
		get;
		private set;
	}

	public static float HoldTime
	{
		get;
		private set;
	}

	public static event TapEventHandler TapBegin;
	public static event TapEventHandler TapEnd;
	public static event TapEventHandler Tap;

	public static event DragEventHandler DragBegin;
	public static event DragEventHandler DragEnd;
	public static event DragEventHandler Drag;

	static InputWrapper()
	{
		GameObject obj = new GameObject("InputWrapper");
		obj.AddComponent<InputController>();
		GameObject.DontDestroyOnLoad(obj);
	}
}
