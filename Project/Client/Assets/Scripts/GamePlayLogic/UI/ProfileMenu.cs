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
using TMPro;
using GameFramework.ASCIISerializer;
using Assets.Scripts.GamePlayLogic.UI.UIItems;
using UnityEngine.UI;
using ClientUtilities.ResourceManager;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class MatchResultItemPool : ObjectPool<MatchResultItem>
    {

    }

    public class AvatarItemPool : ObjectPool<AvatarItem>
    {

    }

    public class ProfileMenu : UIBase
    {
        private UserInfo userInfo;
        private Action OnClose = null;
        private UIButton backButton;
        private UIButton editButton;
        private UIButton applyButton;
        private UIButton totalDataButton;
        private UIButton matchHistoryButton;
        private RTLTextMeshPro Uname;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro userCode;
        private RTLTextMeshPro gtext;
        private RTLTextMeshPro wTtext;
        private RTLTextMeshPro Wtext;
        private RTLTextMeshPro ltext;
        private RTLTextMeshPro wbtext;
        private RTLTextMeshPro lbtext;
        private GameObject setProfileDataPanel;
        private GameObject totalDataPanel;
        private GameObject matchHistoryPanel;
        private TMP_InputField inputFiled;
        private RTLTextMeshPro placeHolderText;
        private RTLTextMeshPro inputFiledTextComponent;
        private Image uAvatar;
        private string tempString;
        private int tempAvatarIndex = 0;
        private RectTransform matchResultsContentPanel;
        private RectTransform avatarsContentPanel;
        private MatchResultItemPool matchesPool = new MatchResultItemPool();
        private AvatarItemPool avatarsPool = new AvatarItemPool();
        private List<MatchResultItem> resultItems = new List<MatchResultItem>();
        private List<AvatarItem> avatarItems = new List<AvatarItem>();

        private bool isReadyToReplay = false;
        private int ReplayGameID = -1;

        protected override void Awake()
        {
            base.Awake();

        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            matchesPool.InitiliazePool("UI/UIItems/MatchResultItem", 10);
            avatarsPool.InitiliazePool("UI/UIItems/AvatarItem", 10);

            backButton = transform.FindDeep("BackButton").GetComponent<UIButton>();
            editButton = transform.FindDeep("EditButton").GetComponent<UIButton>();
            totalDataButton = transform.FindDeep("TotalDataButton").GetComponent<UIButton>();
            matchHistoryButton = transform.FindDeep("MatchHistoryButton").GetComponent<UIButton>();
            Uname = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();

            gtext = transform.FindDeep("GCountText").GetComponent<RTLTextMeshPro>();
            wTtext = transform.FindDeep("WCountText").GetComponent<RTLTextMeshPro>();
            Wtext = transform.FindDeep("WGCountText").GetComponent<RTLTextMeshPro>();
            ltext = transform.FindDeep("LCountText").GetComponent<RTLTextMeshPro>();
            wbtext = transform.FindDeep("WBGCountText").GetComponent<RTLTextMeshPro>();
            lbtext = transform.FindDeep("LBCountText").GetComponent<RTLTextMeshPro>();
            userCode = transform.FindDeep("UserCode").GetComponent<RTLTextMeshPro>();
            uAvatar = transform.FindDeep("Avatar").GetComponent<Image>();
            setProfileDataPanel = transform.FindDeep("SetProfilePanel").gameObject;
            matchHistoryPanel = transform.FindDeep("MatchHistoryPanel").gameObject;
            totalDataPanel = transform.FindDeep("DataPanel").gameObject;
            inputFiled = transform.FindDeep("InputField - RTLTMP", true).GetComponent<TMP_InputField>();
            placeHolderText = inputFiled.placeholder.GetComponent<RTLTextMeshPro>();
            applyButton = transform.FindDeep("ApplyButton", true).GetComponent<UIButton>();
            matchResultsContentPanel = transform.FindDeep("MatchHistoryContent").GetComponent<RectTransform>();
            avatarsContentPanel = transform.FindDeep("AvatarsContentPanel").GetComponent<RectTransform>();

            backButton.onClick.AddListener(HideUI);
            editButton.onClick.AddListener(ShowProfileData);
            applyButton.onClick.AddListener(SubmitData);
            totalDataButton.onClick.AddListener(ShowTotalDataPanel);
            matchHistoryButton.onClick.AddListener(ShowMatchHistoryPanel);

            inputFiled.onEndEdit.AddListener(OnEdit);
            inputFiled.onValueChanged.AddListener(OnEdit);
            inputFiledTextComponent = inputFiled.transform.FindDeep("TextHolder").GetComponent<RTLTextMeshPro>();
            base.SetUIRefrences();

            ShowTotalDataPanel();
        }

        private void SubmitData()
        {
            applyButton.enabled = false;
            if (tempString == string.Empty)
                Uname.text = inputFiledTextComponent.text = tempString = UserInfoManager.Instance.User.UserName;
            if (tempString != UserInfoManager.Instance.User.UserName || tempAvatarIndex != UserInfoManager.Instance.User.AvatarID)
            {
                tempString = tempString.Replace("ی", "ي");
                RequestManager.Instance.Network.SetUserInfo(tempString, tempAvatarIndex);
                UserInfoManager.Instance.UpdateUserInfo(OnUserInfoUpdated);
            }

            setProfileDataPanel.gameObject.SetActive(false);
        }

        private void OnUserInfoUpdated(UserInfo User)
        {
            Uname.text = inputFiledTextComponent.text = User.UserName;
            uAvatar.sprite = GameResourceManager.Instance.LoadAvatarSprite(User.AvatarID.ToString());
        }

        private void OnEdit(string arg0)
        {
            //inputFiled.text = "";

            //inputFiledTextComponent.Farsi = false;
            //inputFiledTextComponent.Farsi = true;
            tempString = arg0.Replace("ی", "ي"); ;
            inputFiledTextComponent.text = tempString;
            //inputFiledTextComponent.Farsi = false;
            //inputFiledTextComponent.Farsi = true;


        }

        private void ShowProfileData()
        {
            applyButton.enabled = true;
            inputFiled.characterLimit = 20;

            inputFiled.text = inputFiledTextComponent.text = tempString;
            setProfileDataPanel.gameObject.SetActive(true);

            SetupAvatars();
        }

        private void SetupAvatars()
        {
            ClearAvatarsPool();
            Sprite[] avatars = GameResourceManager.Instance.LoadAllAvatars();

            for (int i = 0; i < avatars.Length; ++i)
            {
                AvatarItem item = avatarsPool.GetFromPool();
                int index = i;
                item.SetData(index, () => OnAvatarClick(index));
                item.transform.SetParent(avatarsContentPanel, false);
                item.transform.SetAsLastSibling();
                item.gameObject.SetActive(true);
                avatarItems.Add(item);
            }
        }

        private void OnAvatarClick(int index)
        {
            tempAvatarIndex = index;
        }

        private void ClearAvatarsPool()
        {
            for (int i = 0; i < avatarItems.Count; ++i)
            {
                avatarsPool.SendToPool(avatarItems[i]);
            }

            avatarItems.Clear();
        }

        private void ShowTotalDataPanel()
        {
            matchHistoryButton.interactable = true;
            totalDataButton.interactable = false;
            matchHistoryPanel.gameObject.SetActive(false);
            totalDataPanel.gameObject.SetActive(true);
        }

        private void ShowMatchHistoryPanel()
        {
            matchHistoryButton.interactable = false;
            totalDataButton.interactable = true;
            matchHistoryPanel.gameObject.SetActive(true);
            totalDataPanel.gameObject.SetActive(false);
        }

        public override void ShowUI(params object[] Args)
        {

            if (Args != null && Args.Length != 0)
            {
                userInfo = (UserInfo)Args[0];
                if (Args.Length > 1)
                    OnClose = (Action)Args[1];
            }

            base.ShowUI(Args);
            isReadyToReplay = false;
            editButton.gameObject.SetActive(userInfo.ID == UserInfoManager.Instance.User.ID);
            inputFiled.text = placeHolderText.text = Uname.text = userInfo.UserName;
            uLevel.text = string.Format(GameDataManager.GetString("Level"), UserInfoManager.Instance.User.Level);
            uAvatar.sprite = GameResourceManager.Instance.LoadAvatarSprite(userInfo.AvatarID.ToString());
            tempAvatarIndex = userInfo.AvatarID;
            gtext.text = userInfo.GameCount.ToString();
            wTtext.text = userInfo.WinCount.ToString();
            Wtext.text = userInfo.WinGammonCount.ToString();
            ltext.text = userInfo.LoseGammonCount.ToString();
            wbtext.text = userInfo.WinBackGammonCount.ToString();
            lbtext.text = userInfo.LoseBackGammonCount.ToString();
            placeHolderText.text = GameDataManager.GetString("EnterYourName");
            if (userInfo.ID == UserInfoManager.Instance.User.ID)
                userCode.text = string.Format(GameDataManager.GetString("UserCode"), UserInfoManager.Instance.User.ID);
            else
                userCode.text = string.Empty;

            ClearMatchHistoryItems();
            RequestManager.Instance.Network.OnGamesLogDataReady += OnGamesLogDataReady;
            RequestManager.Instance.Network.GetGamesLog(userInfo.ID);
        }

        private void OnGamesLogDataReady(string Data)
        {
            ISerializeArray array = Creator.Create<ISerializeArray>(Data);
            List<MatchResult> matchResults = new List<MatchResult>();

            for (uint i = 0; i < array.Count; ++i) //making match result ready
            {
                MatchResult result = new MatchResult();
                result.DeserialzeData(array.Get<ISerializeObject>(i));
                matchResults.Add(result);
            }


            for (int i = 0; i < matchResults.Count; ++i) //generate match result ui items
            {
                MatchResultItem item = matchesPool.GetFromPool();
                MatchResult matchData = matchResults[i];
                item.SetData(matchData, () => OnMatchReplay(matchData));
                item.transform.SetParent(matchResultsContentPanel, false);
                item.transform.SetAsLastSibling();
                resultItems.Add(item);
            }

        }

        private void ClearMatchHistoryItems()
        {
            for (int i = 0; i < resultItems.Count; ++i)//sending old items too pool
                matchesPool.SendToPool(resultItems[i]);

            resultItems.Clear();
        }

        private void OnMatchReplay(MatchResult matchData)
        {
            if (!matchData.IsReplayAvailable)
            {
                Debug.LogError("Replay Data Is Not Available");
                return;
            }

            RequestManager.Instance.Network.OnGameReplayDataReady += OnReplayDataIsReady;
            ReplayGameID = matchData.ID;
            RequestManager.Instance.Network.GetGameReplayData(matchData.ID);

            Debug.Log($"Showing replay of match with id:{matchData.ID}");
        }

        private void OnReplayDataIsReady(bool IsAvailable, string OtherPlayerInfo, byte[] ReplayData)
        {
            if (!IsAvailable)
            {
                Debug.LogError("Replay Data Is Not Available");
                return;
            }

            //RqeuestUserInfo Opponent = new RqeuestUserInfo();
            //ISerializeObject opponentData = Creator.Create<ISerializeObject>(OtherPlayerInfo);
            //Opponent.Deserialize(opponentData);
            UserInfoManager.Instance.UpdateOpponnentInfo(OtherPlayerInfo);
            UserInfoManager.Instance.UpdateCurrentPlayerInfo(userInfo);

            isReadyToReplay = true;
            HideUI();
            SimulationManager.Instance.ReplayGame(ReplayData, ReplayGameID);
        }

        public override void HideUI()
        {
            base.HideUI();
            ClearMatchHistoryItems();
            RequestManager.Instance.Network.OnGamesLogDataReady -= OnGamesLogDataReady;
            RequestManager.Instance.Network.OnGameReplayDataReady -= OnReplayDataIsReady;
            if (!isReadyToReplay)
                OnClose?.Invoke();
        }



    }
}