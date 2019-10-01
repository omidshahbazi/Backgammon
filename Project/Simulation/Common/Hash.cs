using Simulation.Common.Visitor;
using System;
using System.Collections;
using System.Text;
using GameFramework.Common.Utilities;

namespace Simulation.Common
{
	public static class Hash
	{
		public static int Get(string Value)
		{
			return (int)CRC32.CalculateHash(Encoding.ASCII.GetBytes(Value));
		}

		public static int Get(byte[] Value)
		{
			return (int)CRC32.CalculateHash(Value);
		}
	}

	public class HasherVisitor : IVisitor
	{
		public int Value
		{
			get;
			private set;
		}

		protected void AddBytes(params byte[] Bytes)
		{
			Value += Hash.Get(Bytes);
		}

		protected void AddFloat32(float Value)
		{
		}

		public void Reset()
		{
			Value = 0;
		}

		public void BeginVisitArray(ICollection Collection)
		{
		}

		public void EndVisitArray()
		{
		}

		public void BeginVisitArrayElement()
		{
		}

		public void EndVisitArrayElement()
		{
		}

		public void VisitBool(bool Bool)
		{
			Value += Hash.Get(BitConverter.GetBytes(Bool));
		}

		public void VisitInt32(int Int)
		{
			Value += Hash.Get(BitConverter.GetBytes(Int));
		}

		public void VisitIdentifier(Identifier Identifier)
		{
			VisitInt32(Identifier);
		}
	}
}