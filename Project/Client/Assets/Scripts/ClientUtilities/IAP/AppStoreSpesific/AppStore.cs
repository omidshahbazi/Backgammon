
using System;
using UnityEngine;
using System.Collections.Generic;
using OnePF;
using Assets.Scripts.GamePlayLogic.UserData;

namespace ClientUtilities.IAP
{
    public class AppStore : IStore
    {
        /*-------AppStore specific params ----*/
        private GameObject appObject = null;
        private OpenIABEventManager eventManager;
        private Action<bool, Purchase> onPurchaseDone;
        private Action<string> onError;
 /*-------params to test purchased items-------*/
        private Purchase lastConsumeProduct;
        private PurchaseItem lastPurchaseItem;
        private bool retry = false;

        public AppStore()
        {
            appObject = new GameObject("OpenIABEventManager");
            MonoBehaviour.DontDestroyOnLoad(appObject);
            eventManager = appObject.AddComponent<OpenIABEventManager>();
            AddListeners();
            Initialize(OpenIAB_iOS.STORE);
        } 
        public bool IsInitializes
        {
            get;
            set;
        }

		public BillingState StoreState
		{
			get;
			set;
		}

		public void AddListeners()
        {
            // Listen to all events for illustration purposes
            OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
            OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
            OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
            OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
            OpenIABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
            OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
            OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
            OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
        }

        public void ConsumeItem(ShopPack item, Action<bool, Purchase> onPurchaseDone)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string storeName)
        {
            OpenIAB.init(storeName, "");

        }

        public void PurchaseItem(ShopPack item, Action<bool, Purchase> onPurchaseDone)
        {
            lastPurchaseItem = new PurchaseItem(item);
            OpenIAB.purchaseProduct(item.SKU);
            this.onPurchaseDone = onPurchaseDone;
        }


		public void PurchaseItem(ShopPack pack, Action<string, string> onPurchaseDone)
		{

		}

		public void QueryInventory()
        {
            try
            {
               Debug.Log("StartQueryInventory");
                
                OpenIAB.queryInventory();
            }
            catch
            {
               Debug.Log("error on query Inventory");
             
            }
        }

        public void SetErrorHandler(Action<string> onError)
        {
            this.onError = onError;
        }

        private void billingSupportedEvent()
        {
            IsInitializes = true;
           Debug.Log("billingSupportedEvent");
        

            QueryInventory();

        }


        private void billingNotSupportedEvent(string error)
        {
           Debug.Log("billingNotSupportedEvent: " + error);
           
        }
        private void queryInventorySucceededEvent(Inventory inventory)
        {
           Debug.Log("queryInventorySucceededEvent: " + inventory);
          
            if (inventory != null)
            {
                List<Purchase> purchases = inventory.GetAllPurchases();
                foreach (Purchase p in purchases)
                {
                    lastConsumeProduct = p;
                    OpenIAB.consumeProduct(p);
                }
            }
        }
        private void queryInventoryFailedEvent(string error)
        {
           Debug.Log("queryInventoryFailedEvent: " + error);
           
        }
        private void purchaseSucceededEvent(Purchase purchase)
        {
           Debug.Log("purchaseSucceededEvent: " + purchase);
           
            if (purchase.Sku != null)
            {
                lastPurchaseItem.SetPurchasedData(purchase.Token, purchase.DeveloperPayload);
                OpenIAB.consumeProduct(purchase);
                lastConsumeProduct = purchase;
                return;
            }
        }
        private void purchaseFailedEvent(int errorCode, string errorMessage)
        {
           Debug.Log("purchaseFailedEvent: " + errorCode + " : " + errorMessage);
           
            DisposeSavedPurchaseItem();
            onError(errorMessage);
        }
        private void consumePurchaseSucceededEvent(Purchase purchase)
        {
           Debug.Log("consumePurchaseSucceededEvent: " + purchase);
         
            onPurchaseDone(true, purchase);
            DisposeSavedPurchaseItem();
            retry = false;
        }
        private void consumePurchaseFailedEvent(string error)
        {
           Debug.Log("consumePurchaseFailedEvent: " + error);
          
            if (retry)
            {
                if (lastPurchaseItem != null && lastPurchaseItem.PurchaseToken != null && lastPurchaseItem.ShopPack != null)
                {
                    //To Do we Should Check with Server
                }
                retry = false;
                DisposeSavedPurchaseItem();
                onError(error);
            }
            else
            {
                retry = true;
                if (lastConsumeProduct != null)
                    OpenIAB.consumeProduct(lastConsumeProduct);
            }
        }

        private void DisposeSavedPurchaseItem()
        {
            lastConsumeProduct = null;
            lastPurchaseItem = null;
        }

		public void Deinitialize()
		{
			OpenIAB.unbindService();
		}
	}
}
