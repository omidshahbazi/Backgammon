
using UnityEngine;
using System.Collections;
using System;
using OnePF;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic.Shop;

namespace ClientUtilities.IAP
{
	public enum BillingState
	{
		Supported,
		NotSupported,
		NoAnswer
	}
	public interface IStore
    {
		BillingState StoreState
		{
			set;
			get;
		}
		bool IsInitializes
        {
            get;
            set;
        }

        void AddListeners();
        void Initialize(string storeName);
		void Deinitialize();

		void SetErrorHandler(Action<string> onError);
        void PurchaseItem(ShopPack item, Action<bool, Purchase> onPurchaseDone);
		void PurchaseItem(ShopPack item, Action<string, string> onPurchaseDone);
		
		void ConsumeItem(ShopPack item, Action<bool, Purchase> onPurchaseDone);
        void QueryInventory();
    }
}
