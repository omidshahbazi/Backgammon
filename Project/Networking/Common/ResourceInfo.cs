using GameFramework.ASCIISerializer;

namespace Networking.Common
{
	public abstract class ResourceInfo
	{
		protected const string KEY_COIN = "C";

		public uint Coin
		{
			get;
			private set;
		}

		public ResourceInfo()
		{
		}

		public ResourceInfo(uint Coin)
		{
			this.Coin = Coin;
		}

		public virtual void Serialize(ISerializeObject Object)
		{
			Object.Set(KEY_COIN, Coin);
		}

		public ISerializeObject Serialize()
		{
			ISerializeObject obj = Creator.Create<ISerializeObject>();
			Serialize(obj);
			return obj;
		}

		public virtual void Deserialize(ISerializeObject Object)
		{
			Coin = Object.Get<uint>(KEY_COIN);
		}
	}
}