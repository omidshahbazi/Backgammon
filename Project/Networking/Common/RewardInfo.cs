using GameFramework.ASCIISerializer;

namespace Networking.Common
{
	public class RewardInfo : ResourceInfo
	{
		private const string KEY_XP = "XP";

		public uint XP
		{
			get;
			private set;
		}

		public RewardInfo() : base()
		{
		}

		public RewardInfo(uint Coin, uint XP) : base(Coin)
		{
			this.XP = XP;
		}

		public void SetXP(uint Value)
		{
			XP = Value;
		}

		public override void Serialize(ISerializeObject Object)
		{
			base.Serialize(Object);

			if (XP != 0)
				Object.Set(KEY_XP, XP);
		}

		public override void Deserialize(ISerializeObject Object)
		{
			base.Deserialize(Object);

			XP = Object.Get<uint>(KEY_XP);
		}
	}
}