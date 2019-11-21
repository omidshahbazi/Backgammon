using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.Tables;
using ClientUtilities.ResourceManager;
using ClientUtilities.UI;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{
    public class TableItem : MonoBehaviour
    {
        private Image backGround;
        private Image foreGround;
        private UIButton button;
        private RTLTextMeshPro tableNametxt;
        private RTLTextMeshPro pricetxt;
        private RTLTextMeshPro rewardTxt;
        private RTLTextMeshPro timerText;

        private void Awake()
        {
            button = GetComponent<UIButton>();
            tableNametxt = transform.FindDeep("TableName").GetComponent<RTLTextMeshPro>();
            pricetxt = transform.FindDeep("Price").GetComponent<RTLTextMeshPro>();
            rewardTxt = transform.FindDeep("Reward").GetComponent<RTLTextMeshPro>();
            timerText = transform.FindDeep("TimerText").GetComponent<RTLTextMeshPro>();
            backGround = GetComponent<Image>();
            foreGround = transform.FindDeep("ForeGround").GetComponent<Image>();
        }

        public void SetData(UnityAction OnClick, TablesDataManager.Table Table)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            tableNametxt.text = GameDataManager.GetString(Table.Name);
            pricetxt.text = string.Format(GameDataManager.GetString("Entrance"), Table.Enterance);
            rewardTxt.text = string.Format(GameDataManager.GetString("Reward"), Table.Prize);
            timerText.text = string.Format(GameDataManager.GetString("Seconds"), Table.TurnTime);
            foreGround.sprite = GameResourceManager.Instance.LoadSprite("Fantasy UI/TablesBackGround/" + Table.SpriteName);
            foreGround.color = Table.Color;
        }
    }
}