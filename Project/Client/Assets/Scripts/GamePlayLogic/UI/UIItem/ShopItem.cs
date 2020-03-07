using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Shop;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.IAP;
using ClientUtilities.ResourceManager;
using ClientUtilities.UI;
using OnePF;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{

    public class ShopItem : MonoBehaviour
    {
        private UIButton button;
        private Image icon;
        private RTLTextMeshPro count;
        private RTLTextMeshPro price;
        private RTLTextMeshPro PackageName;
        private RTLTextMeshPro textCoin;
        private RTLTextMeshPro discountText;
        private ShopPack pack = null;
        private GameObject discountTag;
        private Purchase item;


        private void Awake()
        {
            button = GetComponent<UIButton>();
            icon = transform.FindDeep("Icon").GetComponent<Image>();
            count = transform.FindDeep("CoinCount").GetComponent<RTLTextMeshPro>();
            price = transform.FindDeep("PriceCount").GetComponent<RTLTextMeshPro>();
            PackageName = transform.FindDeep("PackageName").GetComponent<RTLTextMeshPro>();
            discountTag = transform.FindDeep("DiscountTag").gameObject;
            discountText = discountTag.transform.FindDeep("DiscountPrice").GetComponent<RTLTextMeshPro>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }


        public void SetData(ShopPack Pack, int Index, RTLTextMeshPro TextCoin)
        {
            pack = null;
            pack = Pack;
            textCoin = TextCoin;
            icon.sprite = Pack.SpriteName == string.Empty ? GameResourceManager.Instance.LoadSprite("Fantasy UI/Fantasy UI Sliced/CoinPacks/" + "DefaultPackCoins" + Index) : GameResourceManager.Instance.LoadSprite("Fantasy UI/Fantasy UI Sliced/CoinPacks/" + Pack.Name);
            count.text = Pack.PackReward.Coin.ToString();
            PackageName.text = GameDataManager.GetString(pack.Name);
            price.text = string.Format(GameDataManager.GetString("CurrenyUnit"), pack.Price.ToString());
            item = null;

            if (pack.DiscountPercent != 0)
            {
                discountTag.gameObject.SetActive(true);
                discountText.text = pack.DiscountPercent + "%";
            }else
            {
                discountTag.gameObject.SetActive(false);
            }
        }


        private void OnClick()
        {
            if (pack == null)
                return;


            //ToDo Some work
            if (!RequestManager.Instance.Network.IsConnected)
                return;

            if (PurchaseManager.Instance.Store.StoreState != BillingState.Supported)
            {
                OnBillingError(PurchaseManager.Instance.Store.StoreState);

                return;
            }

#if UNITY_EDITOR || UNITY_IOS



#else
            item = null;
            Debug.Log(string.Format("OnBuyPackClick , Gem Pack {0}", pack.ID));
            PurchaseManager.Instance.PurchaseItem(pack, OnPurchaseDone, OnPurchaseError);
#endif
        }



        protected void OnPurchaseDone(bool state, Purchase Item)
        {

            Debug.Assert(this.pack != null, "Pack Is Null");
            Debug.Assert(Item != null, "Item is filled null by OpenIAB");
            if (pack != null)
            {
                item = Item;
                GameAnalyticsManager.Instance.SendCoinSourceEvent(pack.PackReward.Coin, "Shop Purchased", "PackCoin :" + pack.PackReward.Coin);
                GameAnalyticsManager.Instance.SendBussinesEvent(ProjectConfigs.Instance.CurrencyType.ToString(), pack.Price, pack.Name, pack.ID.ToString(), "PackCoin :" + pack.PackReward.Coin);
                GameAnalyticsManager.Instance.SendEvent("Purchase Time" + Item.PurchaseTime);
                GameAnalyticsManager.Instance.SendEvent("SKU" + Item.Sku);
                GameAnalyticsManager.Instance.SendEvent("Price" + pack.OriginalPrice);
                GameAnalyticsManager.Instance.SendEvent("Discount Percent" + pack.DiscountPercent);
                RequestManager.Instance.Network.OnPurchaseFinished += Network_OnPurchaseFinished;
                RequestManager.Instance.Network.PurchaseFinished(ProjectConfigs.Instance.market, pack.ID, Item.Token);
                Debug.Log("Buying Coin have been success");
            }
            else
            {
                GameAnalyticsManager.Instance.SendEvent("Pack Is Null");
            }

        }

        private void Network_OnPurchaseFinished(bool IsValid)
        {
            RequestManager.Instance.Network.OnPurchaseFinished -= Network_OnPurchaseFinished;
            if (IsValid)
            {
                if (item != null)
                {
                    AdTracker.Server.Markets market = AdTracker.Server.Markets.GooglePlay;
                    switch (ProjectConfigs.Instance.market)
                    {
                        case Networking.Common.Markets.Windows:
                            break;
                        case Networking.Common.Markets.Cafebazaar:
                            market = AdTracker.Server.Markets.CafeBazaar;
                            break;
                        case Networking.Common.Markets.Myket:
                            market = AdTracker.Server.Markets.Myket;
                            break;
                        default:
                            break;
                    }
                    RequestManager.Instance.AdTracker.SendPurchaseRequest(market, pack.Price, item.Token);
                  
                }
                UserInfoManager.Instance.UpdateUserInfo(OnUserUpdated);
            }
            else
            {
                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("PurchaseFailed"));
            }
            item = null;
        }

        private void OnUserUpdated(UserInfo Arg)
        {
            UIEffect.Instance.AddNotification(true, 0, string.Empty, UIEffect.Instance.CoinAudioPath, UIEffect.Instance.CoinSprite, this.transform.position, textCoin.transform, UIEffect.SpaceType.TwoD, UIEffect.SpaceType.TwoD);
            PopupTextMenu.Instance.ShowPopUpText("+" + pack.PackReward.Coin);
        }

        protected void OnBillingError(BillingState state)
        {

            if (state == BillingState.NotSupported)
            {
                GameAnalyticsManager.Instance.SendEvent("Store billing not supported in this device!");

                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("MarketNoSupport"));

                Debug.LogError("Store billing not supported in this device!");

            }
            else if (state == BillingState.NoAnswer)
            {
                GameAnalyticsManager.Instance.SendEvent("Store billing no answer");

                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("NoAnswerFromMarket"));

                Debug.LogError("Store billing not Initialized completly!");

            }
        }

        protected void OnPurchaseError(string Error)
        {
            GameAnalyticsManager.Instance.SendEvent("On Purchase Error " + Error);
            PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("PurchaseFailed"));
            Debug.Log("Purchase Error : " + Error);
        }
    }
}