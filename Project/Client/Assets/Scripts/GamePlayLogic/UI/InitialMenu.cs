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
using RTLTMPro;

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
        private UIButton LeaderBoardButton;
        private RTLTextMeshPro dailyRewardText;
        private _2dxFX_Shiny_Reflect shinyEffect;
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
            dailyRewardText = dailyRewardButton.transform.FindDeep("Text").GetComponent<RTLTextMeshPro>();
            LeaderBoardButton = transform.FindDeep("LeaderBoard").GetComponent<UIButton>();
            shinyEffect = dailyRewardButton.GetComponent<_2dxFX_Shiny_Reflect>();
            dailyRewardButton.onClick.AddListener(OnDailyRewardButtonClick);

            LeaderBoardButton.onClick.AddListener(ShowLeaderBoard);
        }

     
        public override void ShowUI(object[] Args)
        {
            base.ShowUI(Args);

            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                shinyEffect.enabled = false;

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


        protected override void Update()
        {
            base.Update();

            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                dailyRewardText.text = FormatTime(TimeSpan.FromSeconds( GameManager.Instance.DailyRewardInfo.NextClaimTime)- GameFramework.Common.Timing.Time.CurrentUTCDateTime.TimeOfDay);


        }

        public static string FormatTime(TimeSpan Time)
        {
            //if (Time <= 0)
            //    return "00:00:00";

            //TimeSpan timeSpan = TimeSpan.FromSeconds(Time);

            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                Time.Hours,
                Time.Minutes,
                Time.Seconds);
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


        private void ShowLeaderBoard()
        {
            HideUI();
            UIManager.Instance.ShowUI("LeaderBoardMenu", Close);
        }

    }
}