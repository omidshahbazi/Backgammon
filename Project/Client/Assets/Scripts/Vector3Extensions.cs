using UnityEngine;

public static class Vector3Extensions
{
	public static Vector3 AddX(this Vector3 a, float Value)
	{
		a.x += Value;
		return a;
	}

	public static Vector3 AddY(this Vector3 a, float Value)
	{
		a.y += Value;
		return a;
	}

	public static Vector3 AddZ(this Vector3 a, float Value)
	{
		a.z += Value;
		return a;
	}
}