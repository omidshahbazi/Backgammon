namespace Networking.Common
{
	public abstract class ResourceInfo
	{
		public uint Coin
		{
			get;
			private set;
		}

		public ResourceInfo(uint Coin)
		{
			this.Coin = Coin;
		}
	}
}