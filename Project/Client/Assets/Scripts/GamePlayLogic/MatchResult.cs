using Assets.Scripts.GamePlayLogic.UserData;
using GameFramework.ASCIISerializer;
using Networking.Common;


namespace Assets.Scripts.GamePlayLogic
{
    public class MatchResult
    {
        private const string Key_ID = "id";
        private const string Key_TableID = "table_id";
        private const string Key_OpponentUserID = "opponent_user_id";
        private const string Key_IsWinner = "is_winner";
        private const string Key_FinishReason = "finish_reason";
        private const string Key_OccursTime = "occurs_time";
        private const string Key_IsReplayAvailable = "is_replay_available";
        private const string Key_UserInfo = "user_info";
        private const string Key_BotUserInfo = "bot_user_info";

        public int ID { get; private set; }
        public int TableID { get; private set; }
        public int OpponentUserID { get; private set; }
        public bool IsWinner { get; private set; }
        public GameFinishReasons FinishReason { get; private set; }
        public double OccursTime { get; private set; }
        public bool IsReplayAvailable { get; private set; }
        public UserInfo OpponentInfo { get; private set; }

        public MatchResult()
        {

        }

        public MatchResult(int iD, int tableID, int opponentUserID, bool isWinner, GameFinishReasons finishReason, double occursTime, bool isReplayAvailable, UserInfo opponentInfo)
        {
            ID = iD;
            TableID = tableID;
            OpponentUserID = opponentUserID;
            IsWinner = isWinner;
            FinishReason = finishReason;
            OccursTime = occursTime;
            IsReplayAvailable = isReplayAvailable;
            OpponentInfo = opponentInfo;
        }

        public void DeserialzeData(ISerializeObject Object)
        {
            if (Object.Contains(Key_ID))
                ID = Object.Get<int>(Key_ID);
            if (Object.Contains(Key_TableID))
                TableID = Object.Get<int>(Key_TableID);
            if (Object.Contains(Key_IsWinner))
                IsWinner = Object.Get<bool>(Key_IsWinner);
            if (Object.Contains(Key_FinishReason))
                FinishReason = (GameFinishReasons)Object.Get<uint>(Key_FinishReason);
            if (Object.Contains(Key_OccursTime))
                OccursTime = Object.Get<double>(Key_OccursTime);
            if (Object.Contains(Key_IsReplayAvailable))
                IsReplayAvailable = Object.Get<bool>(Key_IsReplayAvailable);

            RqeuestUserInfo rqi = new RqeuestUserInfo();
            if (Object.Contains(Key_UserInfo) && Object.Get<ISerializeObject>(Key_UserInfo).Count != 0)
            {
                ISerializeObject opponentData = Object.Get<ISerializeObject>(Key_UserInfo);
                rqi.Deserialize(opponentData);
            }
            else if (Object.Contains(Key_BotUserInfo))
            {
                ISerializeObject opponentData = Object.Get<ISerializeObject>(Key_BotUserInfo);
                rqi.Deserialize(opponentData);
            }

            OpponentInfo = rqi.UserInfo;
        }
    }
}