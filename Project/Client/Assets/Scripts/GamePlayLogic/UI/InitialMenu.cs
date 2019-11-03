using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic.UI.UIItems;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class TablePool : ObjectPool<TableItem>
    { }

    public class InitialMenu : UIBase
    {
        private TablePool tableList = new TablePool();
        private List<TableItem> activeTableItem = new List<TableItem>();
        private RectTransform viewPortTransform;
        private MagneticScrollRect scrollView;
        private bool isDataSet;
        private UIButton profileButton;
        private UIButton shopButton;
        private UIButton userCoinPanel;
        private UIButton dailyRewardButton;
        private object Close = null;

        protected override void Awake()
        {
            base.Awake();
            RegisterUI("InitialMenu", this);
            Close = (Action)(() => { ShowUI(); });
        }

        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();
            tableList.InitiliazePool("UI/UIItems/TableItem", 3);
            RegisterUI("InitialMenu", this);
            viewPortTransform = transform.FindDeep("Viewport").GetComponent<RectTransform>();
            scrollView = transform.FindDeep("Magnetic Scroll View").GetComponent<MagneticScrollRect>();
            profileButton = transform.FindDeep("Profile").GetComponent<UIButton>();
            profileButton.onClick.AddListener(OnProfileButtonClick);
            shopButton = transform.FindDeep("ShopButton").GetComponent<UIButton>();
            shopButton.onClick.AddListener(OnShopButtonClick);
            userCoinPanel = transform.FindDeep("CurrencyPanel").GetComponent<UIButton>();
            userCoinPanel.onClick.AddListener(OnShopButtonClick);
            dailyRewardButton = transform.FindDeep("DailyRewardButton").GetComponent<UIButton>();
            dailyRewardButton.onClick.AddListener(OnDailyRewardButtonClick);
        }



        public override void ShowUI(object[] Args)
        {
            base.ShowUI(Args);

            if (!isDataSet)
            {
                isDataSet = true;
                float width = (viewPortTransform.rect.width - (scrollView.ElementPadding * 2)) / 1.5F;
                float height = viewPortTransform.rect.height;
                scrollView.ElementsSize = new Vector2(width, height);
                for (int i = 0; i < TablesDataManager.Instance.Tables.Length; ++i)
                {
                    TableItem it = null;
                    activeTableItem.Add(it = tableList.GetFromPull());
                    TablesDataManager.Table table = TablesDataManager.Instance.Tables[i];
                    it.transform.SetParent(viewPortTransform, false);
                    it.transform.SetAsLastSibling();
                    it.SetData(() => JoinTable(table.Enterance), table.Name, table.Enterance.ToString(), "");
                }
            }
        }

        private void JoinTable(uint Enterance)
        {
            object entranceValue = (ushort)Enterance;
            UIManager.Instance.ShowUI("MatchMakingMenu", entranceValue, Close);
            HideUI();
        }

        private void OnProfileButtonClick()
        {

           // UserInfoManager.Instance.UpdateUserInfo();
            object userInfo = (UserInfo)UserInfoManager.Instance.User;
            if (userInfo == null)
                return;

            HideUI();
            UIManager.Instance.ShowUI("ProfileMenu", userInfo, Close);
        }


        private void OnShopButtonClick()
        {
            object state = (ShopMenu.ShopState)ShopMenu.ShopState.Coin;
            HideUI();
            UIManager.Instance.ShowUI("ShopMenu", state, Close);
           
        }

        private void OnDailyRewardButtonClick()
        {
            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                return;

            HideUI();
            UIManager.Instance.ShowUI("DailyRewardMenu", Close);

        }

     

    }
}