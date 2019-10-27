


using Assets.Scripts.GamePlayLogic.UserData;

namespace ClientUtilities.IAP
{
	public enum ProductType
	{
	    Curreny=0
	};

	public class PurchaseItem
	{
		private ShopPack shopPack;
		private string purchaseToken;
		private string payload;
		public ShopPack ShopPack
		{
			get { return shopPack; }
		}
		public string PurchaseToken
		{
			get { return purchaseToken; }
		}

		public string Payload
		{
			get { return payload; }
		}

		public PurchaseItem(ShopPack pack)
		{
			shopPack = pack;
		}

		public void SetPurchasedData(string token, string pay)
		{
			purchaseToken = token;
			payload = pay;
		}

		public override string ToString()
		{
			return string.Format("ShopPack:[{0}],tocken:{1},payload:{2}", ShopPack, PurchaseToken, Payload);
		}
	}


}