using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using RTLTMPro;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic.UI.ItemPool;
using Assets.Scripts.GamePlayLogic.UI.UIItems;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class SimpleChatItemPool : ObjectPool<SimpleChatItem>
    {

    }

    public class ChatMenu : UIBase
    {
        private UIButton backButton;
        private UIButton sellButton;
        private TabPool tabItemList = new TabPool();
        protected SimpleChatItemPool simpleChatItemList = new SimpleChatItemPool();
        private GridLayoutGroup tabGridLayOut;
        private RectTransform tabViewPort;
        private GridLayoutGroup mainPanelGridLayOut;
        private RectTransform mainPanelRectTransform;
        private RTLTextMeshPro Price;
        private bool isDataSet;
        private GameObject SellPanel;
        private List<TabButtonItem> tabPoolHolder = new List<TabButtonItem>();
        private List<SimpleChatItem> simpleChatPoolHolder = new List<SimpleChatItem>();
        private bool isOpen = false;
        protected override void Awake()
        {

            base.Awake();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;


            tabItemList.InitiliazePool("UI/UIItems/TabButtonItem", 3);
            simpleChatItemList.InitiliazePool("UI/UIItems/SimpleChatItem", 5);
            tabGridLayOut = transform.FindDeep("TabContnet").GetComponent<GridLayoutGroup>();
            tabViewPort = transform.FindDeep("TabViewPort").GetComponent<RectTransform>();
            mainPanelGridLayOut = transform.FindDeep("Content").GetComponent<GridLayoutGroup>();
            mainPanelRectTransform = transform.FindDeep("ChatViewport").GetComponent<RectTransform>();
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            SellPanel = transform.FindDeep("SellPanel", true).gameObject;
            sellButton = SellPanel.transform.FindDeep("SellButton").GetComponent<UIButton>();
            Price = sellButton.transform.FindDeep("Price").GetComponent<RTLTextMeshPro>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideUI);
            base.SetUIRefrences();
        }

        public override void ShowUI(params object[] Args)
        {
            if (IsEnable)
                return;
            base.ShowUI(Args);


            if (!isDataSet)
            {
                tabGridLayOut.cellSize = new Vector2(tabViewPort.rect.width / 4F, tabViewPort.rect.height);


                mainPanelGridLayOut.cellSize = new Vector2(mainPanelRectTransform.rect.width / 2, mainPanelRectTransform.rect.height / 8.1F);
            }
            for (int i = 0; i < ChatManager.Instance.SimpleChatList.Length; ++i)
            {
                TabButtonItem item = null;
                tabPoolHolder.Add(item = tabItemList.GetFromPool());
                ChatPack ch = ChatManager.Instance.SimpleChatList[i];
                item.SetData(() =>
                {
                    ShowChatItems(ch,item);
                    item.SetEnableState(true);
                }, GameDataManager.GetString(ChatManager.Instance.SimpleChatList[i].Name));
                item.SetEnableState(false);
                item.transform.SetParent(tabGridLayOut.transform, false);
                item.transform.SetAsLastSibling();
                if (i == 0)
                {
                    ShowChatItems(ch,item);
                  
                }
            }



            if (ChatManager.Instance.SimpleChatList == null)
            {
                HideUI();
                return;
            }

            //for (int i = 0; i < ChatManager.Instance.SimpleChatList.Length; ++i)
            //{
            //    SimpleChat pack = ChatManager.Instance.SimpleChatList[i];
            //    SimpleChatItem item = simpleChatItemList.GetFromPool();
            //    item.SetData(GameDataManager.GetString(pack.Content), i);
            //    item.transform.SetParent(mainPanelGridLayOut.transform, false);
            //    item.transform.SetAsLastSibling();
            //}

            isDataSet = true;
         
        }

        private void ShowChatItems(ChatPack Item,TabButtonItem TabItem)
        {
            for (int i = 0; i < tabPoolHolder.Count; ++i)
            {
                tabPoolHolder[i].SetEnableState(false);
            }
            TabItem.SetEnableState(true);
            for (int i = 0; i < simpleChatPoolHolder.Count; ++i)
            {
                simpleChatItemList.SendToPool(simpleChatPoolHolder[i]);
            }

            sellButton.onClick.RemoveAllListeners();
            SellPanel.SetActive(false);
            if (!Item.IsSold)
            {
                SellPanel.SetActive(true);
                Price.text = Item.Cost.Coin.ToString();
                sellButton.onClick.AddListener(() => OnBuyChat(Item,TabItem));
            }
            simpleChatItemList.Clear();
            for (int i = 0; i < Item.Chat.Length; ++i)
            {
                SimpleChatItem item = null;
                SimpleChat pack = Item.Chat[i];
                simpleChatPoolHolder.Add(item = simpleChatItemList.GetFromPool());
                item.SetData(GameDataManager.GetString(pack.Content), Item.ID, i);
                item.transform.SetParent(mainPanelGridLayOut.transform, false);
                item.transform.SetAsLastSibling();
            }

        }

        private void OnBuyChat(ChatPack item,TabButtonItem TabItem)
        {
            if (UserInfoManager.Instance.User.Coin < item.Cost.Coin)
            {
                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("YouDontHaveEnoughCoin"));
                return;
            }
            ChatManager.Instance.BuyChat(item.ID, () => ShowChatItems(item, TabItem));
        }



        public override void HideUI()
        {
            base.HideUI();
            for (int i = 0; i < tabPoolHolder.Count; ++i)
            {
                tabItemList.SendToPool(tabPoolHolder[i]);
            }
            tabPoolHolder.Clear();
          
        }
    }

}