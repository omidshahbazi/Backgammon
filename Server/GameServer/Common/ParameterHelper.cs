// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using System.Collections.Generic;
using System.Diagnostics;

namespace GameServer.Common
{
	public class ParameterHelper
	{
		public static Dictionary<byte, object> MakeMap(params object[] Parameters)
		{
			Debug.Assert(Parameters.Length % 2 == 0, "Parameters must be pair of items");

			Dictionary<byte, object> parameters = null;

			if (Parameters != null && Parameters.Length != 0)
			{
				parameters = new Dictionary<byte, object>();
				for (int i = 0; i < Parameters.Length; i += 2)
					parameters[(byte)Parameters[i]] = Parameters[i + 1];
			}

			return parameters;
		}

		public static Dictionary<byte, object> Combine(Dictionary<byte, object> Original, Dictionary<byte, object> Other)
		{
			if (Other != null)
			{
				var it = Other.GetEnumerator();
				while (it.MoveNext())
					Original[it.Current.Key] = it.Current.Value;
			}

			return Original;
		}

		public static T GetParameter<T>(Dictionary<byte, object> Parameters, ParameterTypes Type)
		{
			return (T)Parameters[(byte)Type];
		}
	}
}
