using Simulation.Data.Game;

namespace Simulation.Data.Mutation
{
	public class GameFinishedMutation : MutationBase
	{
		public PlayerColors WinnerColor
		{
			get;
			private set;
		}

		public int Score
		{
			get;
			private set;
		}

		public GameFinishedMutation(PlayerColors WinnerColor, int Score)
		{
			this.WinnerColor = WinnerColor;
			this.Score = Score;
		}

		public override Types GetType()
		{
			return Types.GameFinished;
		}
	}
}
