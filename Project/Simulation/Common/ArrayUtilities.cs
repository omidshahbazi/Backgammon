using System;
using System.Diagnostics;

namespace Deterministic.Common
{
	public static class ArrayUtilities
	{
		public static void Add<T>(ref T[] Arr, T Value)
		{
			Array.Resize(ref Arr, (Arr == null ? 0 : Arr.Length) + 1);

			Arr[Arr.Length - 1] = Value;
		}

		public static void Add<T>(ref T[] Arr, T[] Value)
		{
			if (Value == null)
				return;

			Array.Resize(ref Arr, (Arr == null ? 0 : Arr.Length) + Value.Length);

			for (int i = 0; i < Value.Length; ++i)
				Arr[Arr.Length - (Value.Length - i)] = Value[i];
		}

		public static void AddUnique<T>(ref T[] Arr, T Value)
		{
			if (Arr != null && Array.IndexOf(Arr, Value) != -1)
				return;

			Array.Resize(ref Arr, (Arr == null ? 0 : Arr.Length) + 1);

			Arr[Arr.Length - 1] = Value;
		}

		public static void AddUnique<T>(ref T[] Arr, T[] Value)
		{
			if (Value == null)
				return;

			for (int i = 0; i < Value.Length; ++i)
				AddUnique(ref Arr, Value[i]);
		}

		public static void Remove<T>(ref T[] Arr, T Value)
		{
			int index;
			while ((index = Array.IndexOf(Arr, Value)) != -1)
				RemoveAt(ref Arr, index);
		}

		public static void RemoveAt<T>(ref T[] Arr, int Index)
		{
			Debug.Assert(Index < Arr.Length, "Index cannot be greater than length of array");

			T[] newArr = new T[Arr.Length - 1];

			if (Index > 0)
				Array.Copy(Arr, 0, newArr, 0, Index);

			if (Index < newArr.Length)
				Array.Copy(Arr, Index + 1, newArr, Index, newArr.Length - Index);

			Arr = newArr;
		}

		public static OutT[] Cast<InT, OutT>(InT[] Arr) where OutT : InT
		{
			OutT[] outArr = new OutT[Arr.Length];

			for (int i = 0; i < outArr.Length; ++i)
				outArr[i] = (OutT)Arr[i];

			return outArr;
		}
	}
}