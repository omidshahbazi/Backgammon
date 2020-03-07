using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using RTLTMPro;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.ClientUtilities.Extensions;
using UnityEngine.UI;
using Assets.Scripts.GamePlayLogic.UI.ItemPool;
using Assets.Scripts.GamePlayLogic.UI.UIItems;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Shop;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class ShopItemPool : ObjectPool<ShopItem>
    {

    }

    public class ShopMenu : UIBase
    {
        public enum ShopState
        {
            none = 0,
            Coin,
        }

        private RTLTextMeshPro totalCoin;
        private UIButton backButton;
        private MagneticScrollRect tabScroll;
        private RectTransform tabViewPort;
        private GridLayoutGroup mainPanelGridLayOut;
        private RectTransform mainPanelRectTransform;
        private ScrollRect mainPanelScrollRect;
        private TabPool tabItemList = new TabPool();
        private ShopItemPool shopItemList = new ShopItemPool();
        private ShopState state;
        public Action OnClose;
        private bool isDataSet = false;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            tabItemList.InitiliazePool("UI/UIItems/TabButtonItem", 3);
            shopItemList.InitiliazePool("UI/UIItems/ShopItem", 4);
            totalCoin = transform.FindDeep("UserCoin").GetComponent<RTLTextMeshPro>();
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            tabScroll = transform.FindDeep("Magnetic Scroll View").GetComponent<MagneticScrollRect>();
            tabViewPort = tabScroll.transform.FindDeep("Viewport").GetComponent<RectTransform>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideUI);
            mainPanelGridLayOut = transform.FindDeep("Content").GetComponent<GridLayoutGroup>();
            mainPanelRectTransform = transform.FindDeep("ShopViewport").GetComponent<RectTransform>();
            mainPanelScrollRect = transform.FindDeep("Scroll View").GetComponent<ScrollRect>();
            base.SetUIRefrences();
        }


        public override void ShowUI(params object[] Args)
        {
            base.ShowUI(Args);

            if (Args != null && Args.Length != 0)
            {
                state = (ShopState)Args[0];
                OnClose = (Action)Args[1];
            }


            totalCoin.text = UserInfoManager.Instance.User.Coin.ToString();
            mainPanelScrollRect.gameObject.SetActive(true);

            if (isDataSet)
                return;
            Vector2 tabSize = new Vector2(tabViewPort.rect.width / 3, tabViewPort.rect.height);
            tabScroll.ElementsSize = tabSize;


            if (ShopManager.Instance.CoinPacks.Count != 0)
            {
                TabButtonItem item = tabItemList.GetFromPool();
                item.SetData(() => { }, GameDataManager.GetString("Coin"));
                item.SetEnableState(true);
                item.transform.SetParent(tabViewPort, false);
                item.transform.SetAsLastSibling();
            }


            mainPanelGridLayOut.cellSize = new Vector2(mainPanelRectTransform.rect.width / 2, mainPanelRectTransform.rect.height / 2.2F);

            for (int i = 0; i < ShopManager.Instance.CoinPacks.Count; ++i)
            {
                ShopPack pack = ShopManager.Instance.CoinPacks[i];
                ShopItem item = shopItemList.GetFromPool();
                item.SetData(pack, (i + 1), totalCoin);
                item.transform.SetParent(mainPanelGridLayOut.transform, false);
                item.transform.SetAsLastSibling();
            }

            isDataSet = true;
        }

        public override void HideUI()
        {
            base.HideUI();

            mainPanelScrollRect.gameObject.SetActive(false);
            OnClose?.Invoke();
        }
    }
}