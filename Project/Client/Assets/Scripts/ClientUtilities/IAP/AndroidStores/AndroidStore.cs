
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using OnePF;
using Assets.Scripts.GamePlayLogic.UserData;

namespace ClientUtilities.IAP
{
    public class AndroidStore : IStore
    {
        /*-------Android specific params---------*/
        private GameObject AndroidPurchaseObject = null;
        private string publicKey;
        private OpenIABEventManager eventManager;
        private Action<bool, Purchase> onPurchaseDone;
        private Action<string> onError;
        /*-------params to test purchased items-------*/
        private Purchase lastConsumeProduct;
        private PurchaseItem lastPurchaseItem;
        private bool retry = false;

        public AndroidStore(string storeName, string publicKey)
        {

            AndroidPurchaseObject = new GameObject("OpenIABEventManager");
            MonoBehaviour.DontDestroyOnLoad(AndroidPurchaseObject);
            eventManager = AndroidPurchaseObject.AddComponent<OpenIABEventManager>();
            this.publicKey = publicKey;

            AddListeners();
            Initialize(storeName);
        }


        public void SetErrorHandler(Action<string> onError)
        {
            this.onError = onError;
        }

        public BillingState StoreState
        {
            set;
            get;
        }

        public bool IsInitializes
        {
            get;
            set;
        }


        public void PurchaseItem(ShopPack pack, Action<bool, Purchase> onPurchaseDone)
        {
            //if (ApplicationCheck.IsAppExist(ApplicationCheck.AppType.Bazaar))
            //{
            lastPurchaseItem = new PurchaseItem(pack);
            OpenIAB.purchaseProduct(pack.SKU);
            this.onPurchaseDone = onPurchaseDone;
            //}
            //else
            //{
            //	onError("No Bazaar");
            //	Development.Log("[BazaarStore] PurchaseItem No Package");
            //	Debug.Log("[BazaarStore] PurchaseItem No Package");
            //}
        }

        public void PurchaseItem(ShopPack pack, Action<string, string> onPurchaseDone)
        {

        }

        public void QueryInventory()
        {
            try
            {
                //if (ApplicationCheck.IsAppExist(ApplicationCheck.AppType.Bazaar))
                //{
                Debug.Log("[AndroidStore] StartQueryInventory");
                Debug.Log("[AndroidStore] StartQueryInventory");
                OpenIAB.queryInventory();
                //}
                //else
                //{
                //	onError("No Bazaar");
                //	Development.Log("[BazaarStore] QueryInventory No Package");
                //	Debug.Log("[BazaarStore] QueryInventory No Package");
                //}
            }
            catch
            {
                Debug.Log("[AndroidStore] error on query Inventory");
                Debug.Log("[AndroidStore] error on query Inventory");
            }
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

        public void Initialize(string storeName)
        {
            StoreState = BillingState.NoAnswer;
            Debug.Log("[AndroidStore] Initialize OpenIAB");
            OpenIAB.init(storeName, publicKey);
            //OpenIAB.enableDebugLogging(true);

        }

        public void ConsumeItem(ShopPack item, Action<bool, Purchase> onPurchaseDone)
        {
        }

        private void billingSupportedEvent()
        {
            IsInitializes = true;
            StoreState = BillingState.Supported;

            Debug.Log("[AndroidStore] billingSupportedEvent");



        }


        private void billingNotSupportedEvent(string error)
        {
            StoreState = BillingState.NotSupported;
            if (error != null)
            {
                Debug.Log("[AndroidStore] billingNotSupportedEvent: " + error);

            }
            else
            {
                Debug.Log("[AndroidStore] billingNotSupportedEvent: Null");
            }
        }
        private void queryInventorySucceededEvent(Inventory inventory)
        {
            Debug.Log("[AndroidStore] queryInventorySucceededEvent: " + inventory);

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
            if (error != null)
            {
                Debug.Log("[AndroidStore] queryInventoryFailedEvent: " + error);
         
            }
            else
            {
                Debug.Log("[AndroidStore] queryInventoryFailedEvent: Null");
            }
        }
        private void purchaseSucceededEvent(Purchase purchase)
        {
            Debug.Log("[AndroidStore] purchaseSucceededEvent: " + purchase);
         
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
            Debug.Log("[AndroidStore] purchaseFailedEvent: " + errorCode + " : " + errorMessage);
          
            DisposeSavedPurchaseItem();
            onError(errorMessage);
        }
        private void consumePurchaseSucceededEvent(Purchase purchase)
        {
            Debug.Log("[AndroidStore] consumePurchaseSucceededEvent: " + purchase);
         
            onPurchaseDone(true, purchase);
            DisposeSavedPurchaseItem();
            retry = false;
        }
        private void consumePurchaseFailedEvent(string error)
        {
            Debug.Log("[AndroidStore] consumePurchaseFailedEvent: " + error);

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
                //DeleteAppData();
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
