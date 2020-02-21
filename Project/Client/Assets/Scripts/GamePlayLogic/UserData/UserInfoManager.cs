using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Networking.Common;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UserData
{
    public class RqeuestUserInfo
    {
        private const string ID = "id";
        private const string USERNAME = "username";
        private const string LANGUAGE = "language";
        private const string SPLIT_TEST_ID = "split_test_group_id";
        private const string COIN = "coin";
        private const string XP = "xp";
        private const string LEVEL = "level";
        private const string GAME_COUNT = "game_count";
        private const string WIN_COUNT = "win_count";
        private const string WIN_GAMMON_COUNT = "win_gammon_count";
        private const string LOSE_GAMOON_COUNT = "lose_gammon_count";
        private const string WIN_BACKGAMMON_COUNT = "win_backgammon_count";
        private const string LOSE_BACKGAMMON_COUNT = "lose_backgammon_count";
        private const string SPLIT_TEST_GROUP_NAME = "split_test_group_name";
        private const string AVATAR_ID = "avatar";
        private const string CHAT_PACK = "chat_packs";

        public UserInfo UserInfo
        {
            get;
            private set;
        }


        private int id, xp, coin, level, gamecount, wincount, winGammonCount, loseGammonCount, winBackGammonCount, loseBackGammonCount, avatarID;
        private string splitGroupName, userName = string.Empty;
        private Languages language;
        private Action<int, UserInfo> OnComplete = null;


        public void GetUserInfo(int UserID, Action<int, UserInfo> OnComplete)
        {
            RequestManagers.RequestManager.Instance.Network.OnUserInfoReady += Network_OnUserInfoReady;
            RequestManagers.RequestManager.Instance.Network.GetUserInfo(UserID);
            this.OnComplete = OnComplete;
        }


        public void Deserialize(ISerializeObject Object)
        {
            GameAnalyticsManager.Instance.SendEvent("User Data Deserialize Begin");

            Debug.Assert(Object != null, "Object is null");
            if (Object == null)
            {
                GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Critical, "User Object is null");
                return;
            }

            int[] chatPack = null;
            if (Object.IsContains(ID))
                id = Object.Get<int>(ID);
            if (Object.IsContains(COIN))
                coin = Object.Get<int>(COIN);
            if (Object.IsContains(USERNAME))
                userName = Object.Get<string>(USERNAME);
            if (Object.IsContains(LANGUAGE))
                language = (Languages)Object.Get<int>(LANGUAGE);
            if (Object.IsContains(XP))
                xp = Object.Get<int>(XP);
            if (Object.IsContains(LEVEL))
                level = Object.Get<int>(LEVEL);
            if (Object.IsContains(GAME_COUNT))
                gamecount = Object.Get<int>(GAME_COUNT);
            if (Object.IsContains(WIN_COUNT))
                wincount = Object.Get<int>(WIN_COUNT);
            if (Object.IsContains(WIN_GAMMON_COUNT))
                winGammonCount = Object.Get<int>(WIN_GAMMON_COUNT);
            if (Object.IsContains(LOSE_GAMOON_COUNT))
                loseGammonCount = Object.Get<int>(LOSE_GAMOON_COUNT);
            if (Object.IsContains(WIN_BACKGAMMON_COUNT))
                winBackGammonCount = Object.Get<int>(WIN_BACKGAMMON_COUNT);
            if (Object.IsContains(LOSE_BACKGAMMON_COUNT))
                loseBackGammonCount = Object.Get<int>(LOSE_BACKGAMMON_COUNT);
            if (Object.IsContains(SPLIT_TEST_GROUP_NAME))
                splitGroupName = Object.Get<string>(SPLIT_TEST_GROUP_NAME);
            if (Object.IsContains(AVATAR_ID))
                avatarID = Object.Get<int>(AVATAR_ID);

            if (Object.IsContains(CHAT_PACK))
            {
                ISerializeArray array = Object.Get<ISerializeArray>(CHAT_PACK);
                chatPack = new int[array.Count];
                for (uint i = 0; i < array.Count; ++i)
                {
                    chatPack[i] = array.Get<int>(i);
                }
            }

            UserInfo = new UserInfo(id, userName, splitGroupName, language, coin, xp, level, gamecount,
                wincount, winGammonCount, loseGammonCount, winBackGammonCount, loseBackGammonCount, chatPack, avatarID);
            GameAnalyticsManager.Instance.SendEvent("User Data Deserialize end");

        }

        private void Network_OnUserInfoReady(int UserID, string Info)
        {
            RequestManagers.RequestManager.Instance.Network.OnUserInfoReady -= Network_OnUserInfoReady;
            Deserialize(Creator.Create<ISerializeObject>(Info));
            OnComplete?.Invoke(UserID, UserInfo);
            OnComplete = null;
        }
    }

    public class UserInfo
    {
        public int ID
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public int AvatarID
        {
            get;
            private set;
        }

        public string SplitGroupName
        {
            get;
            private set;
        }

        public Languages Language
        {
            get;
            private set;
        }

        public int Coin
        {
            get;
            private set;
        }

        public int XP
        {
            get;
            private set;
        }

        public int Level
        {
            get;
            private set;
        }

        public int GameCount
        {
            get;
            private set;
        }

        public int WinCount
        {
            get;
            private set;
        }

        public int WinGammonCount
        {
            get;
            private set;
        }

        public int LoseGammonCount
        {
            get;
            private set;
        }

        public int WinBackGammonCount
        {
            get;
            private set;
        }

        public int LoseBackGammonCount
        {
            get;
            private set;
        }

        public int[] ChatPack
        {
            get;
            private set;
        }

        public UserInfo(int iD, string userName, string splitGroupName, Languages language, int coin, int xP, int level, int gameCount, int winCount, int winGammonCount, int loseGammonCount, int winBackGammonCount, int loseBackGammonCount, int[] chatPack, int avatarID = 1)
        {
            ID = iD;
            UserName = userName;
            SplitGroupName = splitGroupName;
            Language = language;
            Coin = coin;
            XP = xP;
            Level = level;
            GameCount = gameCount;
            WinCount = winCount;
            WinGammonCount = winGammonCount;
            LoseGammonCount = loseGammonCount;
            WinBackGammonCount = winBackGammonCount;
            LoseBackGammonCount = loseBackGammonCount;
            AvatarID = avatarID;
            ChatPack = chatPack;
        }
    }


    public class UserInfoManager : MonoBehaviorSingleton<UserInfoManager>
    {
        public UserInfo User
        {
            get;
            private set;
        }

        public UserInfo Opponnent
        {
            get;
            private set;
        }

        public UserInfo CurrentPlayer
        {
            get;
            private set;
        }

        private void Awake()
        {
            SimulationManager.Instance.OnGameFinished += Instance_OnGameFinished;
        }

        private void Instance_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, int Score)
        {
            UpdateUserInfo();
        }


        public void UpdateUserInfo(Action<UserInfo> OnComplete = null)
        {
            UpdateUserInfo(User.ID, OnComplete);
        }

        public void UpdateUserInfo(int ID, Action<UserInfo> OnComplete = null)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.GetUserInfo(ID, (Id, Info) =>
            {
                User = Info;
                OnComplete?.Invoke(User);
            });
        }

        public void UpdateOpponnentInfo(string Info)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.Deserialize(Creator.Create<ISerializeObject>(Info));
            if (fillUser.UserInfo != null)
                Opponnent = fillUser.UserInfo;
        }

        public void UpdateCurrentPlayerInfo(string Info)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.Deserialize(Creator.Create<ISerializeObject>(Info));
            if (fillUser.UserInfo != null)
                CurrentPlayer = fillUser.UserInfo;
        }

        public void UpdateCurrentPlayerInfo(UserInfo Info)
        {
            if (Info != null)
                CurrentPlayer = Info;
        }

        public void GetUserInfo(int ID, Action<UserInfo> OnComplete)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.GetUserInfo(ID, (Id, Info) =>
            {
                OnComplete?.Invoke(Info);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}