
using UnityEngine;
using System.Collections;
using System;
using OnePF;

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
        void PurchaseItem(PackData item, Action<bool, Purchase> onPurchaseDone);
		void PurchaseItem(PackData item, Action<string, string> onPurchaseDone);
		
		void ConsumeItem(PackData item, Action<bool, Purchase> onPurchaseDone);
        void QueryInventory();
    }
}
