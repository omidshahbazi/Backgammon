


namespace ClientUtilities.IAP
{
	public enum ProductType
	{
	    Curreny=0
	};

	public class PurchaseItem
	{
		private PackData packData;
		private string purchaseToken;
		private string payload;
		public PackData PackData
		{
			get { return packData; }
		}
		public string PurchaseToken
		{
			get { return purchaseToken; }
		}

		public string Payload
		{
			get { return payload; }
		}

		public PurchaseItem(PackData pack)
		{
			packData = pack;
		}

		public void SetPurchasedData(string token, string pay)
		{
			purchaseToken = token;
			payload = pay;
		}

		public override string ToString()
		{
			return string.Format("PackData:[{0}],tocken:{1},payload:{2}", PackData, PurchaseToken, Payload);
		}
	}

	public class PackData
	{
	
		public int ID
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public ProductType Type
		{
			get;
			private set;
		}

		public uint Price
		{
			get;
			private set;
		}

        public uint Reward
        {
            get;
            private set;
        }


		public uint Discount
		{
			get;
			private set;
		}

		public uint OriginalPrice
		{
			get { return (uint)(Price * (100.0F / (100 - Discount))); }
		}

	
		public string SKU
		{
			get;
			private set;
		}

		public double ExpireTime
		{
			get;
			private set;
		}

		public PackData(int ID, string Name, ProductType Type, uint Price,uint Reward ,uint Discount, string SKU, double ExpireTime)
		{
			this.ID = ID;
			this.Name = Name;
			this.Type = Type;
			this.Price = Price;
			this.Discount = Discount;
			this.SKU = SKU;
			this.ExpireTime = ExpireTime;
            this.Reward = Reward;
		}
	}
}