using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UserData
{
    public class RqeuestUserInfo
    {
        private const string ID = "id";
        private const string USER_NAME = "username";
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

        public UserInfo UserInfo
        {
            get;
            private set;
        }


        private int id, xp, coin, level, gamecount, wincount, winGammonCount, loseGammonCount, winBackGammonCount, loseBackGammonCount;
        private string userName = string.Empty;
        private Action<int, UserInfo> OnComplete = null;
      

        public void GetUserInfo(int UserID, Action<int, UserInfo> OnComplete)
        {
            RequestManagers.RequestManager.Instance.Network.OnUserInfoReady += Network_OnUserInfoReady;
            RequestManagers.RequestManager.Instance.Network.GetUserInfo(UserID);
            this.OnComplete = OnComplete;
        }


        public void Deserialize(ISerializeObject Object)
        {

            if (Object == null)
                return;

            if (Object.Contains(ID))
                id = Object.Get<int>(ID);
            if (Object.Contains(COIN))
                coin = Object.Get<int>(COIN);
            if (Object.Contains(USER_NAME))
                userName = Object.Get<string>(USER_NAME);
            if (Object.Contains(XP))
                xp = Object.Get<int>(XP);
            if (Object.Contains(LEVEL))
                level = Object.Get<int>(LEVEL);
            if (Object.Contains(GAME_COUNT))
                gamecount = Object.Get<int>(GAME_COUNT);
            if (Object.Contains(WIN_COUNT))
                wincount = Object.Get<int>(WIN_COUNT);
            if (Object.Contains(WIN_GAMMON_COUNT))
                winGammonCount = Object.Get<int>(WIN_GAMMON_COUNT);
            if (Object.Contains(LOSE_GAMOON_COUNT))
                loseGammonCount = Object.Get<int>(LOSE_GAMOON_COUNT);
            if (Object.Contains(WIN_BACKGAMMON_COUNT))
                winBackGammonCount = Object.Get<int>(WIN_BACKGAMMON_COUNT);
            if (Object.Contains(LOSE_BACKGAMMON_COUNT))
                loseBackGammonCount = Object.Get<int>(LOSE_BACKGAMMON_COUNT);

            UserInfo = new UserInfo(id, userName, coin, xp, level, gamecount,
                wincount, winGammonCount, loseGammonCount, winBackGammonCount, loseBackGammonCount);
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

        public UserInfo(int iD, string userName, int coin, int xP, int level, int gameCount, int winCount, int winGammonCount, int loseGammonCount, int winBackGammonCount, int loseBackGammonCount)
        {
            ID = iD;
            UserName = userName;
            Coin = coin;
            XP = xP;
            Level = level;
            GameCount = gameCount;
            WinCount = winCount;
            WinGammonCount = winGammonCount;
            LoseGammonCount = loseGammonCount;
            WinBackGammonCount = winBackGammonCount;
            LoseBackGammonCount = loseBackGammonCount;
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


        public void UpdateUserInfo()
        {
            if (User == null)
                return;

            UpdateUserInfo(User.ID);
        }

        public void UpdateUserInfo(int ID)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.GetUserInfo(ID, (Id, Info) =>
            {
                User = Info;
            });
        }

        public void UpdateOpponnentInfo(string Info)
        {
            RqeuestUserInfo fillUser = new RqeuestUserInfo();
            fillUser.Deserialize(Creator.Create<ISerializeObject>(Info));
            if (fillUser.UserInfo != null)
                Opponnent = fillUser.UserInfo;
        }

    }
}