using GameFramework.ASCIISerializer;

namespace Networking.Common
{
	public class RewardInfo : ResourceInfo
	{
		private const string KEY_XP = "XP";
		private const string KEY_DICE_ID = "DID";
		private const string KEY_CHAT_PACKAGE_ID = "CPID";

		public const int INVALID_DICE_ID = 0;
		public const int INVALID_CHAT_PACK_ID = 0;

		public uint XP
		{
			get;
			private set;
		}

		public int DiceID
		{
			get;
			private set;
		}

		public int ChatPackID
		{
			get;
			private set;
		}

		public RewardInfo() : base()
		{
			DiceID = INVALID_DICE_ID;
			ChatPackID = INVALID_CHAT_PACK_ID;
		}

		public RewardInfo(uint Coin, uint XP, int DiceID = INVALID_DICE_ID, int ChatPackID = INVALID_CHAT_PACK_ID) : base(Coin)
		{
			this.XP = XP;
			this.DiceID = DiceID;
			this.ChatPackID = ChatPackID;
		}

		public void SetXP(uint Value)
		{
			XP = Value;
		}

		public void SetDiceID(int Value)
		{
			DiceID = Value;
		}

		public void SetChatPackageID(int Value)
		{
			ChatPackID = Value;
		}

		public override void Serialize(ISerializeObject Object)
		{
			base.Serialize(Object);

			if (XP != 0)
				Object.Set(KEY_XP, XP);

			if (DiceID != INVALID_DICE_ID)
				Object.Set(KEY_DICE_ID, DiceID);

			if (ChatPackID != INVALID_CHAT_PACK_ID)
				Object.Set(KEY_CHAT_PACKAGE_ID, DiceID);
		}

		public override void Deserialize(ISerializeObject Object)
		{
			base.Deserialize(Object);

			XP = Object.Get<uint>(KEY_XP);
			DiceID = Object.Get<int>(KEY_DICE_ID, INVALID_DICE_ID);
			ChatPackID = Object.Get<int>(KEY_CHAT_PACKAGE_ID, INVALID_CHAT_PACK_ID);
		}
	}
}