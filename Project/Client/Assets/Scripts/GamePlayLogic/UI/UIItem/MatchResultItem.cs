using Assets.Scripts.ClientUtilities.Extensions;
using ClientUtilities.UI;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{
    public class MatchResultItem : MonoBehaviour
    {
        private RTLTextMeshPro enemyNameText;
        private RTLTextMeshPro matchDateText;
        private RTLTextMeshPro winOrLoseText;
        private UIButton replayButton;

        private void Awake()
        {
            replayButton = transform.FindDeep("ReplayButton").GetComponent<UIButton>();
            enemyNameText = transform.FindDeep("EnemyNameText").GetComponent<RTLTextMeshPro>();
            matchDateText = transform.FindDeep("MatchDate").GetComponent<RTLTextMeshPro>();
            winOrLoseText = transform.FindDeep("WinOrLoseText").GetComponent<RTLTextMeshPro>();
        }

        public void SetData(MatchResult MatchResultData, UnityAction OnReplay)
        {
            enemyNameText.text = MatchResultData.OpponentInfo.UserName;
            DateTime matchTime = UnixTimeStampToDateTime(MatchResultData.OccursTime);
            matchDateText.text = matchTime.ToShortDateString() + " " + matchTime.ToShortTimeString();
            winOrLoseText.text = MatchResultData.IsWinner ? GameDataManager.GetString("Win") : GameDataManager.GetString("Lose");
            winOrLoseText.color = MatchResultData.IsWinner ? Color.green : Color.red;

            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(OnReplay);
            replayButton.interactable = MatchResultData.IsReplayAvailable;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}