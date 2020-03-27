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
        //private Image foreGround;
        private GameObject backGroundPanel;
        private UIButton button;
        private RTLTextMeshPro tableNametxt;
        private RTLTextMeshPro pricetxt;
        private RTLTextMeshPro rewardTxt;
        private RTLTextMeshPro timerText;
        private Image tableIcon;

        public Sprite[] TableIcons = new Sprite[4];
        public Sprite[] TableBackGround = new Sprite[4];

        private void Awake()
        {
            button = GetComponent<UIButton>();
            tableNametxt = transform.FindDeep("TableName").GetComponent<RTLTextMeshPro>();
            pricetxt = transform.FindDeep("Price").GetComponent<RTLTextMeshPro>();
            rewardTxt = transform.FindDeep("Reward").GetComponent<RTLTextMeshPro>();
            timerText = transform.FindDeep("TimerText").GetComponent<RTLTextMeshPro>();
            backGround = GetComponent<Image>();
            //foreGround = transform.FindDeep("ForeGround").GetComponent<Image>();
            backGroundPanel = transform.FindDeep("BackGroundPanel").gameObject;
            tableIcon = transform.FindDeep("TableIcon").GetComponent<Image>();
        }

        public void SetData(UnityAction OnClick, TablesDataManager.Table Table, bool ShowPrices)
        {
            tableIcon.sprite = TableIcons[Table.ID - 1];
            backGround.sprite = TableBackGround[Table.ID - 1];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            tableNametxt.text = GameDataManager.GetString(Table.Name);
            pricetxt.text = Table.Enterance.ToString();
            rewardTxt.text = Table.Prize.Coin.ToString();
            timerText.text = Table.TurnTime.ToString();
            //foreGround.sprite = GameResourceManager.Instance.LoadSprite("Fantasy UI/TablesBackGround/" + Table.SpriteName);
            //foreGround.color = Table.Color;
            backGroundPanel.gameObject.SetActive(ShowPrices);
        }
    }
}