using Simulation.Common.Visitor;

namespace Simulation.Data.Game
{
	public abstract class DataBase : IVisitee
	{
		public virtual void Visit(IVisitor Visitor)
		{
		}
	}
}
