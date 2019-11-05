using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.Shop;
using Assets.Scripts.GamePlayLogic.Tables;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.Singleton;
using GameFramework.ASCIISerializer;
using GameFramework.BinarySerializer;
using GameFramework.Common.FileLayer;
using Networking.Common;
using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.LeaderBoard
{
    public struct User
    {
        public int Coin
        {
            get;
            private set;
        }

        public UserInfo UserInfo
        {
            get;
            private set;
        }

        public User(int coin, UserInfo userInfo) : this()
        {
            Coin = coin;
            UserInfo = userInfo;
        }
    }



    public class LeaderBoardManager : MonoBehaviorSingleton<LeaderBoardManager>
    {
        public User[] HourlyUsers
        {
            get;
            private set;
        }

        public User[] DailyUsers
        {
            get;
            private set;
        }

        public User[] WeakelyUsers
        {
            get;
            private set;
        }

        public User[] AllTime
        {
            get;
            private set;
        }

        public bool IsDataFilled
        {
            get { return index == LeaderBoardLength; }
        }

        private int LeaderBoardLength
        {
            get
            {
                return Enum.GetNames(typeof(LeaderboardTypes)).Length;
            }
        }

        private float period = 200;
        private float totalTime = 0;
        private int index = 0;

        private void Awake()
        {
            RequestManager.Instance.Network.OnLeaderboardDataReady += Network_OnLeaderboardDataReady;

        }


        private void Updata()
        {

            if (GameManager.Instance == null || !GameManager.Instance.IsGameDataReady || TableManager.Instance == null || TableManager.Instance.IsGameStarted)
                return;

            if (totalTime > Time.time)
                return;
            totalTime = Time.time + period;
            GetAllLeaderBoardData();
        }

        public void GetAllLeaderBoardData()
        {
            totalTime = Time.time + period;
            index = 0;
            //GetSpeceficLeaderBoard(LeaderboardTypes.Daily);
            for (int i = 0; i < LeaderBoardLength; ++i)
                GetSpeceficLeaderBoard((LeaderboardTypes)i);


        }


        private void GetSpeceficLeaderBoard(LeaderboardTypes Type)
        {
            RequestManager.Instance.Network.GetLeaderboard(Type);
        }

        private void Network_OnLeaderboardDataReady(LeaderboardTypes Type, long StartTime, string Data)
        {
            // index++;

            ScheduleManager.Instance.AddThreadedSchedule(() =>
            {
                ISerializeArray Object = Creator.Create<ISerializeArray>(Data);
                User[] tempUser = new User[Object.Count];

                for (uint i = 0; i < Object.Count; ++i)
                {
                    ISerializeObject obj = Object.Get<ISerializeObject>(i);
                    if (!obj.IsContains("user_info") || !obj.IsContains("coin"))
                        continue;

                    ISerializeObject userObj = obj.Get<ISerializeObject>("user_info");
                    RqeuestUserInfo fillUser = new RqeuestUserInfo();
                    fillUser.Deserialize(userObj);
                    tempUser[i] = new User(obj.Get<int>("coin"), fillUser.UserInfo);


                }

                switch (Type)
                {
                    case LeaderboardTypes.Hourly:
                        HourlyUsers = null;
                        HourlyUsers = tempUser;
                        break;
                    case LeaderboardTypes.Daily:
                        DailyUsers = null;
                        DailyUsers = tempUser;
                        break;
                    case LeaderboardTypes.Weekly:
                        WeakelyUsers = null;
                        WeakelyUsers = tempUser;
                        break;
                    case LeaderboardTypes.AllTime:
                        AllTime = null;
                        AllTime = tempUser;
                        break;
                    default:
                        break;
                }
            }, () => index++);
        }
    }
}