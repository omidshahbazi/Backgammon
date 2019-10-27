using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
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
        private ShopPack pack = null;

        private void Awake()
        {
            button = GetComponent<UIButton>();
            icon = transform.FindDeep("Icon").GetComponent<Image>();
            count = transform.FindDeep("CoinCount").GetComponent<RTLTextMeshPro>();
            price = transform.FindDeep("PriceCount").GetComponent<RTLTextMeshPro>();
            PackageName = transform.FindDeep("PackageName").GetComponent<RTLTextMeshPro>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            pack = null;
        }

        public void SetData(ShopPack Pack)
        {
            pack = Pack;

            icon.sprite = Pack.SpriteName == string.Empty ? GameResourceManager.Instance.LoadSprite(Pack.SpriteName) : GameResourceManager.Instance.LoadSprite(Pack.Name);
            count.text = Pack.Coin.ToString();
            PackageName.text = pack.Name.ToString();
            price.text =  pack.Price + "تومان";
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

            //Logic.CostRewardSystem.RewardInfo reward = new Logic.CostRewardSystem.RewardInfo(SoftCurrency.Zero, new HardCurrency(Bcpi.Amount), 0);
            //Logic.CostRewardSystem.CostRewardManager.Instance.ClaimReward(reward);//, "GetMoreGemDev"
            ////ResourcesManager.Instance.Income(new HardCurrency(Bcpi.Amount));
            //WindowCollectNotificationManager.Instance.SetNotification(Bcpi.Amount, "", GameResources.HardCurrencyIcon, this.transform.position, mainBaseGemPanel, UINotifications.SpaceType.TwoD, UINotifications.SpaceType.TwoD);

#else
            Debug.Log(string.Format("OnBuyPackClick , Gem Pack {0}", pack.ID));
            PurchaseManager.Instance.PurchaseItem(pack, OnPurchaseDone, OnPurchaseError);
#endif
        }



        protected void OnPurchaseDone(bool state, Purchase Item)
        {
            if (state)
            {
                Debug.Assert(this.pack != null, "Pack Is Null");

                if (pack != null)
                {
                   // RequestManager.Instance.Network.PurchaseFinished( , pack.ID, Item.Token);
                    //UINotifications.Instance.AddTextNotification("Soft Currency Pack request Send");
                    Debug.Log("Buying Coin have been success");
                }

            }
            else
            {
                //UINotifications.Instance.AddTextNotification(LocalizationManager.Instance.Get(LocalizationHelperUI.Instance.PurchaseSuccessfulWithError.ID).Value);
                Debug.Log("Payload Not valid ,Purchase Failed.");
            }
        }


        protected void OnBillingError(BillingState state)
        {
            pack = null;
            if (state == BillingState.NotSupported)
            {
                //UINotifications.Instance.AddTextNotification(LocalizationManager.Instance.Get(LocalizationHelperUI.Instance.StoreNotSupported.ID).Value);
                Debug.LogError("Store billing not supported in this device!");

            }
            else if (state == BillingState.NoAnswer)
            {
                // UINotifications.Instance.AddTextNotification(LocalizationManager.Instance.Get(LocalizationHelperUI.Instance.StoreNoAnswer.ID).Value);
                Debug.LogError("Store billing not Initialized completly!");

            }
        }

        protected void OnPurchaseError(string Error)
        {

            // UINotifications.Instance.AddTextNotification(LocalizationManager.Instance.Get(LocalizationHelperUI.Instance.PurchaseError.ID).Value);
            Debug.Log("Purchase Error : " + Error);
        }
    }
}