using System;
using System.Collections;
using System.Collections.Generic;

namespace Simulation.Common.Visitor
{
	public class VisitorBase : IVisitor
	{
		private List<byte> data = new List<byte>();

		public byte[] Data
		{
			get { return data.ToArray(); }
		}

		public virtual void Reset()
		{
			data.Clear();
		}

		protected virtual void AddBytes(params byte[] Bytes)
		{
			data.AddRange(Bytes);
		}

		protected virtual void AddInt64(long Value)
		{
		}

		public virtual void BeginVisitArray(ICollection Collection)
		{
		}

		public virtual void EndVisitArray()
		{
		}

		public virtual void BeginVisitArrayElement()
		{
		}

		public virtual void EndVisitArrayElement()
		{
		}

		public virtual void VisitBool(bool Value)
		{
			data.AddRange(BitConverter.GetBytes(Value));
		}

		public virtual void VisitInt32(int Value)
		{
			data.AddRange(BitConverter.GetBytes(Value));
		}

		public virtual void VisitIdentifier(Identifier Value)
		{
			VisitInt32(Value);
		}
	}
}
