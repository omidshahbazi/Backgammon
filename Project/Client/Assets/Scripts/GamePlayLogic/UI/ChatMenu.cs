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
        private TabPool tabItemList = new TabPool();
        protected SimpleChatItemPool simpleChatItemList = new SimpleChatItemPool();
        private MagneticScrollRect tabScroll;
        private RectTransform tabViewPort;
        private GridLayoutGroup mainPanelGridLayOut;
        private RectTransform mainPanelRectTransform;
        private bool isDataSet;

        protected override void Awake()
        {

            base.Awake();
        }

        public override void SetUIRefrences()
        {
            base.SetUIRefrences();

            tabItemList.InitiliazePool("UI/UIItems/TabButtonItem", 3);
            simpleChatItemList.InitiliazePool("UI/UIItems/SimpleChatItem", 5);
            tabScroll = transform.FindDeep("Magnetic Scroll View").GetComponent<MagneticScrollRect>();
            tabViewPort = tabScroll.transform.FindDeep("Viewport").GetComponent<RectTransform>();
            mainPanelGridLayOut = transform.FindDeep("Content").GetComponent<GridLayoutGroup>();
            mainPanelRectTransform = transform.FindDeep("ChatViewport").GetComponent<RectTransform>();
            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideUI);
        }

        public override void ShowUI(params object[] Args)
        {
            base.ShowUI(Args);


            if (isDataSet)
                return;
            Vector2 tabSize = new Vector2(tabViewPort.rect.width / 3, tabViewPort.rect.height);
            tabScroll.ElementsSize = tabSize;

            for (int i = 0; i < 1; ++i)
            {
                TabButtonItem item = tabItemList.GetFromPull();
                item.SetData(() => { }, "چت");
                item.SetEnableState(true);
                item.transform.SetParent(tabViewPort, false);
                item.transform.SetAsLastSibling();
            }

            mainPanelGridLayOut.cellSize = new Vector2(mainPanelRectTransform.rect.width / 2, mainPanelRectTransform.rect.height / 7.1F);

            for (int i = 0; i < ChatManager.Instance.SimpleChatList.Length; ++i)
            {
                SimpleChat pack = ChatManager.Instance.SimpleChatList[i];
                SimpleChatItem item = simpleChatItemList.GetFromPull();
                item.SetData(pack.Content, i);
                item.transform.SetParent(mainPanelGridLayOut.transform, false);
                item.transform.SetAsLastSibling();
            }

            isDataSet = true;
        }

        public override void HideUI()
        {
            base.HideUI();
        }
    }

}