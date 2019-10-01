using Simulation.Common;
using Simulation.Common.Visitor;
using System.Collections;
using System.IO;
using GameFramework.BinarySerializer;

namespace Simulation.Data.Serialization
{
	public class SerializerVisitor : IVisitor
	{
		private BufferStream buffer = null;

		public byte[] Data
		{
			get { return buffer.Buffer; }
		}

		public SerializerVisitor()
		{
			Reset();
		}

		public void Reset()
		{
			buffer = new BufferStream(new MemoryStream());
		}

		public void BeginVisitArray(ICollection Collection)
		{
			buffer.BeginWriteArray(Collection == null ? 0 : Collection.Count);
		}

		public void EndVisitArray()
		{
			buffer.EndWriteArray();
		}

		public void BeginVisitArrayElement()
		{
			buffer.BeginWriteArrayElement();
		}

		public void EndVisitArrayElement()
		{
			buffer.EndWriteArrayElement();
		}

		public void VisitBool(bool Bool)
		{
			buffer.WriteBool(Bool);
		}

		public void VisitInt32(int Int)
		{
			buffer.WriteInt32(Int);
		}

		public void VisitIdentifier(Identifier Identifier)
		{
			VisitInt32(Identifier);
		}
	}
}
