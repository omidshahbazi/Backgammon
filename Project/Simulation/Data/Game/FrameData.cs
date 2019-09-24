using Simulation.Common.Visitor;
using Simulation.Data.Event;

namespace Simulation.Data.Game
{
	public class FrameData : IVisitee
	{
		public BoardData Board;
		public EventBase[] Events;

		public int Hash;

		public void Visit(IVisitor Visitor)
		{
			Board.Visit(Visitor);

			Visitor.BeginVisitArray(Events);
			if (Events != null)
				for (int i = 0; i < Events.Length; ++i)
				{
					Visitor.BeginVisitArrayElement();

					Events[i].Visit(Visitor);

					Visitor.EndVisitArrayElement();
				}
			Visitor.EndVisitArray();
		}
	}
}
