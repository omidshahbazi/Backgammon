using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using OnePF;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic;
using Assets.Scripts.GamePlayLogic.Shop;

namespace ClientUtilities.IAP
{
    public class PurchaseManager : ClientUtilities.Singleton.MonoBehaviorSingleton<PurchaseManager>
    {

        private const string BazaarKey = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDCmcCaLqNPJavSz92bhENrQaRl5d7RheWNCnchzgOy/XNrLompsatMNN98RB5p2ZHaVklHfygT/AoRxJ+W9hbmMJa7wvQCAlz+z9SFWtJGVNzEFTCqFKLZPW+H1v5qPj5Ye9yeQ5G270vP4aljuu4mKDd5HKqzcK6SvF3WEQ/6JEcfkES7cdU0j5pq5OmceMask5Y7AgOo2bnbf1Zy/5/7mKMORWpmJVMxgFkDCx8CAwEAAQ==";
        private const string charkhuneKey = "";
        private const string MyketKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCU5Fy8Xz1wKoT+yiOv2OpbbvRh16BmdyB7U9Q6sPCFFh0L++Z/+z9Jct3+xbwYv96KxuUgLu4L+vSliMkvyCjTVQrNmyhqxYpD0TPirjYXTI8Al8GtVaIV6XeGxKRik147qN9B0jlDGXIeaIxTc1GJ6k7j6c2LJCl9Rj0+/gRR9wIDAQAB";
        private const string IranAppsKey = "";
        private const string ZarrinPalKey = "";



        public bool Initilized
        {
            get;
            private set;
        }

        public ShopPack[] CoinPacks
        {
            //To Do 
            get { return ShopManager.Instance.Packs; }
        }


        public IStore Store
        {
            get;
            private set;
        }


        protected void Awake()
        {
            Debug.Log("[Purchase Manager] Constructor listen to Event");

        }



        public void Init()
        {
            Debug.Log("[Purchase Manager] Init Started");
            Initilized = true;

			//#if ZARINPAL_MARKET
			//        if (Store == null)
			//Store = new ZarrnPaIAP(ZarrinPalKey);
			//Debug.Log("[Purchase Manager] initialize Store : ZarinPal ");
			switch (ProjectConfigs.Instance.market)

			{
				case Networking.Common.Markets.Windows:
					break;
				case Networking.Common.Markets.Cafebazaar:
					MapSKU(CoinPacks, OpenIAB_Android.STORE_CAFEBAZAAR);
					Debug.Log("[Purchase Manager] CAFEBAZAAR_MARKET , mapSKU");
					Store = new AndroidStore(OpenIAB_Android.STORE_CAFEBAZAAR, BazaarKey);
					Debug.Log("[Purchase Manager] initialize Store : Bazaar " + Store.ToString());
					break;
				case Networking.Common.Markets.Myket:
					MapSKU(CoinPacks, OpenIAB_Android.STORE_MYKET);
					Debug.Log("[Purchase Manager]" + OpenIAB_Android.STORE_MYKET + "mapSKU");
					Store = new AndroidStore(OpenIAB_Android.STORE_MYKET, MyketKey);
					Debug.Log("[Purchase Manager] initialize Store : MYKET " + Store.ToString());
					break;
				default:
					break;
			}


			//#elif UNITY_ANDROID && CAFEBAZAAR_MARKET

			//#elif UNITY_ANDROID && CHAAHAARKHOONEH_MARKET
			//			MapSKU(CoinPacks, OpenIAB_Android.STORE_CHARKHUNEH);       
			//            Debug.Log("[Purchase Manager] CHAAHAARKHOONEH_MARKET , mapSKU");
			//            Store = new AndroidStore(OpenIAB_Android.STORE_CHARKHUNEH, charkhuneKey);
			//            Debug.Log("[Purchase Manager] CHAAHAARKHOONEH_MARKET Store : Charkhune " + Store.ToString());
			//#elif UNITY_ANDROID && MYKET_MARKET

			//#elif UNITY_ANDROID && IRANAPPS_MARKET
			//            MapSKU(CoinPacks, OpenIAB_Android.STORE_IRANAPPS);
			//            Debug.Log("[Purchase Manager]" + OpenIAB_Android.STORE_IRANAPPS + "mapSKU");
			//            Store = new AndroidStore(OpenIAB_Android.STORE_IRANAPPS, IranAppsKey);
			//            Debug.Log("[Purchase Manager] initialize Store : IRANAPPS " + Store.ToString());
			//#elif UNITY_IOS && APPSTORE_MARKET
			//			MapSKU(CoinPacks, OpenIAB_iOS.STORE);
			//            Debug.Log("[Purchase Manager] APPSTORE_MARKET , mapSKU");
			//            Store = new AppStore();
			//            Debug.Log("[Purchase Manager] initialize Store : AppStore "+ Store.ToString());
			//#endif
		}

        public void PurchaseItem(ShopPack item, Action<bool, Purchase> onPurchaseDone, Action<string> onError)
        {
            Debug.Log("[Purchase Manager] Start purchaseing an Item : ");

            Store.PurchaseItem(item, onPurchaseDone);
            Store.SetErrorHandler(onError);
        }


        public void PurchaseItem(ShopPack item, Action<string, string> onPurchaseDone, Action<string> onError)
        {
            Debug.Log("[Purchase Manager] Start purchaseing an Item : ");

            Store.SetErrorHandler(onError);
            Store.PurchaseItem(item, onPurchaseDone);

        }

        private void MapSKU(ShopPack[] ShopPack, string storeId)
        {
            Debug.Log("[Purchase Manager] MapSKU , data count:" + ShopPack.Length);
            for (int i = 0; i < ShopPack.Length; i++)
                OpenIAB.mapSku(ShopPack[i].SKU, storeId, ShopPack[i].SKU);

        }

        public void QueryInventory(Action<string> onError)
        {
            Debug.Log("[Purchase Manager] QueryInventory");
            Store.SetErrorHandler(onError);
            Store.QueryInventory();
        }


        protected override void OnDestroy()
        {
            if (Store != null)
                Store.Deinitialize();
            base.OnDestroy();
        }
    }
}
