namespace Networking.Common
{
	public class RewardInfo : ResourceInfo
	{
		public uint XP
		{
			get;
			private set;
		}

		public RewardInfo(uint Coin, uint XP) : base(Coin)
		{
			this.XP = XP;
		}
	}
}