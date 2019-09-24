using Simulation.Common;
using Simulation.Common.Serialization;
using Simulation.Common.Visitor;
using System.Collections;
using System.IO;

namespace Simulation.Data.Serialization
{
	public class SerializerVisitor : IVisitor
	{
		private Serializer serializer = null;

		public byte[] Data
		{
			get { return serializer.Data.ToArray(); }
		}

		public SerializerVisitor()
		{
			Reset();
		}

		public void Reset()
		{
			serializer = new Serializer(new MemoryStream());
		}

		public void BeginVisitArray(ICollection Collection)
		{
			serializer.BeginWriteArray(Collection == null ? 0 : Collection.Count);
		}

		public void EndVisitArray()
		{
			serializer.EndWriteArray();
		}

		public void BeginVisitArrayElement()
		{
			serializer.BeginWriteArrayElement();
		}

		public void EndVisitArrayElement()
		{
			serializer.EndWriteArrayElement();
		}

		public void VisitBool(bool Bool)
		{
			serializer.WriteBool(Bool);
		}

		public void VisitInt32(int Int)
		{
			serializer.WriteInt32(Int);
		}

		public void VisitIdentifier(Identifier Identifier)
		{
			VisitInt32(Identifier);
		}
	}
}
