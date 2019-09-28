namespace Simulation.Data.Mutation
{
	public class TurnChangedMutation : MutationBase
	{
		public TurnChangedMutation()
		{
		}

		public override Types GetType()
		{
			return Types.TurnChanged;
		}
	}
}
