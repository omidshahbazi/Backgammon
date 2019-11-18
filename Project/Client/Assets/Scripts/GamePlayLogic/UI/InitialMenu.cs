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
        public static InitialMenu Instance
        {
            get;
            private set;
        }

        public UIButton userCoinPanel
        {
            get;
            private set;
        }
        private UITweenMover TweanEffect;
        private TablePool tableList = new TablePool();
        private List<TableItem> activeTableItem = new List<TableItem>();
        private RectTransform viewPortTransform;
        private MagneticScrollRect scrollView;
        private bool isDataSet;
        private GameObject coinPanel;

        private UIButton okButton;
        private UIButton noButton;
        private UIButton profileButton;
        private UIButton shopButton;
     
        private UIButton dailyRewardButton;
        private UIButton LeaderBoardButton;
        private RTLTextMeshPro dailyRewardText;
        private RTLTextMeshPro currecnyText;
        private RTLTextMeshPro userNameText;
        private _2dxFX_Shiny_Reflect shinyEffect;
        private object Close = null;

        protected override void Awake()
        {
            base.Awake();
            RegisterUI("InitialMenu", this);
            Close = (Action)(() => { ShowUI(); });
            Instance = this;
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            TweanEffect = GetComponent<UITweenMover>();
            coinPanel = transform.FindDeep("CoinNeeded", true).gameObject;

            tableList.InitiliazePool("UI/UIItems/TableItem", 3);
            RegisterUI("InitialMenu", this);
            viewPortTransform = transform.FindDeep("Viewport").GetComponent<RectTransform>();
            scrollView = transform.FindDeep("Magnetic Scroll View").GetComponent<MagneticScrollRect>();
            profileButton = transform.FindDeep("Profile").GetComponent<UIButton>();
            profileButton.onClick.AddListener(OnProfileButtonClick);
            shopButton = transform.FindDeep("ShopButton").GetComponent<UIButton>();

            userCoinPanel = transform.FindDeep("CurrencyPanel").GetComponent<UIButton>();
            userCoinPanel.onClick.AddListener(OnShopButtonClick);
            dailyRewardButton = transform.FindDeep("DailyRewardButton").GetComponent<UIButton>();
            dailyRewardText = dailyRewardButton.transform.FindDeep("Text").GetComponent<RTLTextMeshPro>();
            okButton = coinPanel.transform.FindDeep("OkButton", true).GetComponent<UIButton>();
            noButton = coinPanel.transform.FindDeep("NoButton", true).GetComponent<UIButton>();
            LeaderBoardButton = transform.FindDeep("LeaderBoard").GetComponent<UIButton>();
            userNameText = transform.FindDeep("UserNameText").GetComponent<RTLTextMeshPro>();
            currecnyText = transform.FindDeep("CurrencyText").GetComponent<RTLTextMeshPro>();
            shinyEffect = dailyRewardButton.GetComponent<_2dxFX_Shiny_Reflect>();




            dailyRewardButton.onClick.AddListener(OnDailyRewardButtonClick);
            shopButton.onClick.AddListener(OnShopButtonClick);
            okButton.onClick.AddListener(OnShopButtonClick);
            noButton.onClick.AddListener(CloseCoinPanel);
            LeaderBoardButton.onClick.AddListener(ShowLeaderBoard);
            base.SetUIRefrences();
            //UIManager.Instance.SetSpeceficUIRefrences("DailyRewardMenu");
        }


        public override void ShowUI(object[] Args)
        {

            base.ShowUI(Args);

            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                shinyEffect.enabled = false;

            userNameText.text = UserInfoManager.Instance.User.UserName;
            currecnyText.text = UserInfoManager.Instance.User.Coin.ToString();
            if (!isDataSet)
            {

                isDataSet = true;
                float width = (viewPortTransform.rect.width - (scrollView.ElementPadding * 2)) / 1.5F;
                float height = viewPortTransform.rect.height;
                scrollView.ElementsSize = new Vector2(width, height);
                for (int i = 0; i < TablesDataManager.Instance.Tables.Length; ++i)
                {
                    TableItem it = null;
                    activeTableItem.Add(it = tableList.GetFromPool());
                    TablesDataManager.Table table = TablesDataManager.Instance.Tables[i];
                    it.transform.SetParent(viewPortTransform, false);
                    it.transform.SetAsLastSibling();
                    it.SetData(() => JoinTable(table), table.SpriteName, table.Name, table.Enterance.ToString(), table.Prize.ToString(), table.TurnTime.ToString());
                }

            }
            TweanEffect.OnAnimateInsideIn();
        }


        protected override void Update()
        {
            base.Update();

            //if (Input.GetKeyDown(KeyCode.B))
            //    TweanEffect.OnAnimateInsideOut();

            //if (Input.GetKeyDown(KeyCode.C))
            //    TweanEffect.OnAnimateInsideIn();
            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                dailyRewardText.text = StringExtensions.FormatTime(GameManager.Instance.DailyRewardInfo.NextClaimTime);


        }

 


        public void HideUI(Action Action)
        {

            TweanEffect.OnAnimateInsideOut(() =>
            {
                base.HideUI();
                Action?.Invoke();

            });
        }

        private void JoinTable(TablesDataManager.Table Table)
        {
            if (Table.Enterance > UserInfoManager.Instance.User.Coin)
            {
                coinPanel.gameObject.SetActive(true);
                return;
            }

            if (UserInfoManager.Instance.User.Level < Table.UnlockLevel)
            {
                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("ULevelIsNotSufficient").Replace("%%", Table.UnlockLevel.ToString()));
                return;
            }

            HideUI(() =>
        {
            object selectedTable = (TablesDataManager.Table)Table;
            UIManager.Instance.ShowUI("MatchMakingMenu", selectedTable, Close);
        });
        }

        private void OnProfileButtonClick()
        {
            HideUI(() =>
            {
                object userInfo = (UserInfo)UserInfoManager.Instance.User;
                if (userInfo == null)
                    return;


                UIManager.Instance.ShowUI("ProfileMenu", userInfo, Close);
            });
            // UserInfoManager.Instance.UpdateUserInfo();

        }

        private void CloseCoinPanel()
        {
            coinPanel.gameObject.SetActive(false);
        }


        private void OnShopButtonClick()
        {
            CloseCoinPanel();
            HideUI(() =>
            {
                object state = (ShopMenu.ShopState)ShopMenu.ShopState.Coin;

                UIManager.Instance.ShowUI("ShopMenu", state, Close);
            });


        }

        private void OnDailyRewardButtonClick()
        {
            if (DailyRewardMenu.Instance != null && DailyRewardMenu.Instance.IsRewardShowed)
                return;

            HideUI(() =>
            {

                UIManager.Instance.ShowUI("DailyRewardMenu", Close);
            });

        }


        private void ShowLeaderBoard()
        {
            HideUI(() =>
            {

                UIManager.Instance.ShowUI("LeaderBoardMenu", Close);
            });
        }

    }
}