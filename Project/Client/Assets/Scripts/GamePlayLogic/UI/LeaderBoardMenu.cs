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
using Assets.Scripts.GamePlayLogic.UI.ItemPool;
using Assets.Scripts.GamePlayLogic.LeaderBoard;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class LeaderBoarItemPool : ObjectPool<LeaderBoardItem>
    {

    }

    public class LeaderBoardMenu : UIBase
    {
        private GameObject userPanel;
        private RTLTextMeshPro uRankText;
        private RTLTextMeshPro UserNameText;
        private RTLTextMeshPro coinText;
        private UITweenMover tween;
        private RTLTextMeshPro titleText;
        private RTLTextMeshPro descriptionText;
        private GridLayoutGroup leaderBoardPanel;

        private RectTransform leaderBoardTabViewPort;
        private TabPool tabItemList = new TabPool();
        private Action OnClose = null;

        private UIButton backButton;
        private string leaderdBoard;
        private string UPlace;
        private string LAText;
        private string LWText;
        private string URRankText;
        private List<TabButtonItem> tabList = new List<TabButtonItem>();

        private List<LeaderBoardItem> itemList = new List<LeaderBoardItem>();
        private GridLayoutGroup mainPanelGridLayOut;
        private RectTransform mainPanelRectTransform;
        private bool isDataSet = false;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;
           
            tween = GetComponent<UITweenMover>();

            tabItemList.InitiliazePool("UI/UIItems/TabButtonItem", 3);
            mainPanelGridLayOut = transform.FindDeep("TabContnet").GetComponent<GridLayoutGroup>();
            mainPanelRectTransform = transform.FindDeep("TabViewPort").GetComponent<RectTransform>();

            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            titleText = transform.FindDeep("TitleText").GetComponent<RTLTextMeshPro>();
            descriptionText = transform.FindDeep("DescriptionText").GetComponent<RTLTextMeshPro>();
            leaderBoardPanel = transform.FindDeep("LeaderBoardContnet").GetComponent<GridLayoutGroup>();
            //leaderBoardPanel1 = transform.FindDeep("LeaderBoardContnet1").GetComponent<GridLayoutGroup>();
            leaderBoardTabViewPort = transform.FindDeep("LeaderBoardViewPort").GetComponent<RectTransform>();
       
            backButton.onClick.AddListener(HideUI);
            userPanel = transform.FindDeep("UserPanel").gameObject;
            uRankText = userPanel.transform.FindDeep("URankText").GetComponent<RTLTextMeshPro>();
            UserNameText = userPanel.transform.FindDeep("UserNameText").GetComponent<RTLTextMeshPro>();
            coinText = userPanel.transform.FindDeep("CoinText").GetComponent<RTLTextMeshPro>();
            base.SetUIRefrences();
        }


        //protected override void Update()
        //{
        //    base.Update();

        //    if (Input.GetKeyDown(KeyCode.B))
        //    {
        //        for (int i = 0; i < tweens.Length; ++i)
        //            tweens[i].OnAnimateInsideIn();
        //    }

        //    if (Input.GetKeyDown(KeyCode.C))
        //    {
        //        for (int i = 0; i < tweens.Length; ++i)
        //            tweens[i].OnAnimateInsideOut();

        //    }
        //}



        public override void ShowUI(params object[] Args)
        {

            leaderdBoard = GameDataManager.GetString("LeaderBoard");
            UPlace = GameDataManager.GetString("UPlace");
            LAText = GameDataManager.GetString("LATText");
            LWText = GameDataManager.GetString("LWText");
            URRankText = GameDataManager.GetString("URankText");
            LeaderBoardManager.Instance.GetAllLeaderBoardData();
            base.ShowUI(Args);
            userPanel.gameObject.SetActive(false);
            tween.OnAnimateInsideIn(() =>
            {

                titleText.text = leaderdBoard;

                if (Args != null && Args.Length != 0)
                    OnClose = (Action)Args[0];

                if (!isDataSet)
                {
                    int length = Enum.GetNames(typeof(LeaderboardTypes)).Length;
                    mainPanelGridLayOut.cellSize = new Vector2(mainPanelRectTransform.rect.width / 4.1F, mainPanelRectTransform.rect.height);

                    leaderBoardPanel.cellSize = new Vector2(leaderBoardTabViewPort.rect.width, leaderBoardTabViewPort.rect.height / 8.5F);

                    for (int i = 0; i < length; ++i)
                    {
                        TabButtonItem item = null;
                        LeaderboardTypes Type = (LeaderboardTypes)i;
                        tabList.Add(item = tabItemList.GetFromPool());
                        item.SetData(() => { ShowTab(Type, item); }, GameDataManager.GetString(Type.ToString()));
                        item.SetEnableState(false);
                        item.transform.SetParent(mainPanelGridLayOut.transform, false);
                        item.transform.SetAsLastSibling();
                    }
                    isDataSet = true;
                }

                ShowLeaderBoard();
            });

        }


        private void ShowLeaderBoard()
        {
            if (LeaderBoardManager.Instance.IsDataFilled)
                ShowTab(LeaderboardTypes.Weekly, tabList[2]);
            else
                ScheduleManager.Instance.AddSchedule(ShowLeaderBoard, 1);
        }
        public override void HideUI()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                LeaderBoardItem it = itemList[i];

                UIManager.Instance.leaderBoolItemPool.SendToPool(it);
                it.ResetInitialValues();
                it.transform.SetAsLastSibling();


            }
            itemList.Clear();

            tween.OnAnimateInsideOut(() =>
            {
                base.HideUI();
                OnClose?.Invoke();
            }
            );

        }

        private void ShowTab(LeaderboardTypes Type, TabButtonItem Item)
        {
            if (!LeaderBoardManager.Instance.IsDataFilled)
                return;

            for (int i = 0; i < tabList.Count; ++i)
                tabList[i].SetEnableState(false);

            Item.SetEnableState(true);
            switch (Type)
            {
                case LeaderboardTypes.Hourly:
                    if (LeaderBoardManager.Instance.UserContainInsideHourly == null)
                        userPanel.gameObject.SetActive(false);
                    else
                    {
                        userPanel.gameObject.SetActive(true);
                        uRankText.text = URRankText;
                        coinText.text = LeaderBoardManager.Instance.UserContainInsideHourly.Coin.ToString();
                        UserNameText.text = UserInfoManager.Instance.User.UserName;
                    }
                    descriptionText.text = LAText;
                    FillTheList(LeaderBoardManager.Instance.HourlyUsers);
                    break;
                case LeaderboardTypes.Daily:
                    descriptionText.text = LAText;

                    if (LeaderBoardManager.Instance.UserContainInsideDaily == null)
                        userPanel.gameObject.SetActive(false);
                    else
                    {
                        userPanel.gameObject.SetActive(true);
                        uRankText.text = URRankText;
                        coinText.text = LeaderBoardManager.Instance.UserContainInsideDaily.Coin.ToString();
                        UserNameText.text = UserInfoManager.Instance.User.UserName;
                    }
                    FillTheList(LeaderBoardManager.Instance.DailyUsers);
                    break;
                case LeaderboardTypes.Weekly:
                    descriptionText.text = LWText;
                    if (LeaderBoardManager.Instance.UserContainInsideWeakly == null)
                        userPanel.gameObject.SetActive(false);
                    else
                    {
                        userPanel.gameObject.SetActive(true);
                        uRankText.text = URRankText;
                        coinText.text = LeaderBoardManager.Instance.UserContainInsideWeakly.Coin.ToString();
                        UserNameText.text = UserInfoManager.Instance.User.UserName;
                    }
                    FillTheList(LeaderBoardManager.Instance.WeakelyUsers);
                    break;
                case LeaderboardTypes.AllTime:
                    descriptionText.text = LAText;
                    if (LeaderBoardManager.Instance.UserContainInsideAllTime == null)
                        userPanel.gameObject.SetActive(false);
                    else
                    {
                        userPanel.gameObject.SetActive(true);
                        uRankText.text = URRankText;
                        coinText.text = LeaderBoardManager.Instance.UserContainInsideAllTime.Coin.ToString();
                        UserNameText.text = UserInfoManager.Instance.User.UserName;
                    }

                    FillTheList(LeaderBoardManager.Instance.AllTime);
                    break;
                default:
                    break;
            }
        }

        private void FillTheList(User[] Array)
        {

            for (int i = 0; i < itemList.Count; i++)
            {
                LeaderBoardItem it = itemList[i];

                UIManager.Instance.leaderBoolItemPool.SendToPool(it);
                it.ResetInitialValues();
                it.transform.SetAsLastSibling();


            }
            itemList.Clear();

            for (int i = 0; i < Array.Length; i++)
            {
                LeaderBoardItem item = UIManager.Instance.leaderBoolItemPool.GetFromPool();
                User us = Array[i];
                itemList.Add(item);
                item.SetData(us, this.gameObject);

                //if (item.transform.parent == null)
                item.transform.SetParent(leaderBoardPanel.transform, false);
                //else
                //{
                //    if (item.transform.parent == leaderBoardPanel)
                //        item.transform.SetParent(leaderBoardPanel.transform, false);
                //    else
                //        item.transform.SetParent(leaderBoardPanel.transform, false);
                //}
                item.transform.localScale = Vector3.one;
                item.transform.SetAsLastSibling();

            }
            for (int i = 0; i < itemList.Count; i++)
            {
                LeaderBoardItem it = itemList[i];
                it.SetTextSize();
            }
        }
    }
}