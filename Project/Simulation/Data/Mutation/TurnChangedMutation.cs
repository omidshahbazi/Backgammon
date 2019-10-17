using Simulation.Data.Game;

namespace Simulation.Data.Mutation
{
	public class TurnChangedMutation : MutationBase
	{
		public PlayerColors Color
		{
			get;
			private set;
		}

		public TurnChangedMutation(PlayerColors Color)
		{
			this.Color = Color;
		}

		public override Types GetType()
		{
			return Types.TurnChanged;
		}
	}
}
