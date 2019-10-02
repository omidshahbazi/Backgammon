namespace Networking.Common
{
	public class RewardInfo : ResourceInfo
	{
		public uint XP
		{
			get;
			private set;
		}

		public uint Point
		{
			get;
			private set;
		}

		public RewardInfo(uint Coin, uint XP, uint Point) : base(Coin)
		{
			this.XP = XP;
			this.Point = Point;
		}
	}
}