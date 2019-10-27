using Assets.Scripts.ClientUtilities.Extensions;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UserData
{

    public class ShopPack
    {
        public ushort ID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string SKU
        {
            get;
            private set;
        }

        public ushort Price
        {
            get;
            private set;
        }

        public ushort Coin
        {
            get;
            private set;
        }

        public ushort DiscountPercent
        {
            get;
            private set;
        }

        public string SpriteName
        {
            get;
            private set;
        }

        public ushort OriginalPrice
        {
            get { return (ushort)(Price * (100.0F / (100 - DiscountPercent))); }
        }


        public ShopPack(ushort iD, string name, string sKU, ushort price, ushort coin, ushort discountPercent, string spriteName)
        {
            ID = iD;
            Name = name;
            SKU = sKU;
            Price = price;
            Coin = coin;
            DiscountPercent = discountPercent;
            SpriteName = spriteName;
        }
    }


    public class ShopManager : MonoBehaviorSingleton<ShopManager>
    {
        private const string ID_KEY = "ID";
        private const string NAME_KEY = "Name";
        private const string SKU_KEY = "SKU";
        private const string PRICE_KEY = "Price";
        private const string COIN_KEY = "Coin";
        private const string DISCOUNT_KEY = "DiscountPercent";
        private const string SPRITE_NAME_KEY = "SpriteName";

        public ShopPack[] Packs
        {
            get;
            private set;
        }

        private void InitilizePacks(uint Count)
        {
            if (Packs != null && Packs.Length != 0)
                return;

            Packs = new ShopPack[Count];
        }

        public void FillPacks(ISerializeArray Array)
        {
            GameAnalyticsManager.Instance.SendEvent("Shop Data Deserialize Begin");

            if (Array == null || Array.Count == 0)
                return;

            ushort id = ushort.MinValue;
            string name = string.Empty;
            string sku = string.Empty;
            ushort price = ushort.MinValue;
            ushort coin = ushort.MinValue;
            ushort discount = ushort.MinValue;
            string spriteName = string.Empty;

            InitilizePacks(Array.Count);
            if (Packs == null || Packs.Length == 0)
                return;
            for (uint i = 0; i < Packs.Length; ++i)
            { 
                ISerializeObject obj = Array.Get<ISerializeObject>(i);
                if (obj.IsContains(ID_KEY))
                    id = obj.Get<ushort>(ID_KEY);
                if (obj.IsContains(NAME_KEY))
                    name = obj.Get<string>(NAME_KEY);
                if (obj.IsContains(SKU_KEY))
                    sku = obj.Get<string>(SKU_KEY);
                if (obj.IsContains(PRICE_KEY))
                    price = obj.Get<ushort>(PRICE_KEY);
                if (obj.IsContains(COIN_KEY))
                    coin = obj.Get<ushort>(COIN_KEY);
                if (obj.IsContains(DISCOUNT_KEY))
                    discount = obj.Get<ushort>(DISCOUNT_KEY);
                if (obj.IsContains(SPRITE_NAME_KEY))
                   spriteName= obj.Get<string>(SPRITE_NAME_KEY);

                Packs[i] = new ShopPack(id, name, sku, price, coin, discount, spriteName);
            }

            GameAnalyticsManager.Instance.SendEvent("Shop Data Deserialize end");
        }
    }
}