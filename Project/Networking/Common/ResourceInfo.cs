namespace Networking.Common
{
    public abstract class ResourceInfo
    {
		public uint Coin
		{
			get;
			private set;
		}

		public uint Point
		{
			get;
			private set;
		}

		public ResourceInfo(uint Coin, uint Point)
		{
			this.Coin = Coin;
			this.Point = Point;
		}
	}
}