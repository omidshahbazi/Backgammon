namespace Simulation.Data.Mutation
{
	public class DiceChangedMutation : MutationBase
	{
		public int Number
		{
			get;
			private set;
		}

		public int Value
		{
			get;
			private set;
		}

		public DiceChangedMutation(int Number, int Value)
		{
			this.Number = Number;
			this.Value = Value;
		}

		public override Types GetType()
		{
			return Types.DiceChanged;
		}
	}
}
