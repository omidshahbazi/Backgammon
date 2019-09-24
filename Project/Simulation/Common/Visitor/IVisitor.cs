using System.Collections;

namespace Simulation.Common.Visitor
{
	public interface IVisitor
	{
		void Reset();

		void BeginVisitArray(ICollection Collection);
		void EndVisitArray();

		void BeginVisitArrayElement();
		void EndVisitArrayElement();

		void VisitBool(bool Bool);
		void VisitInt32(int Int);

		void VisitIdentifier(Identifier Identifier);
	}
}
