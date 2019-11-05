using Assets.Scripts.ClientUtilities.Extensions;
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
        }

        public void SetData(UnityAction OnClick, string SpriteName,string TableName,string Price,string Reward ,string Timer)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            tableNametxt.text = TableName;
            pricetxt.text = " ورودي " + Price;
            rewardTxt.text =   " جايزه " + Reward;
            timerText.text = " ثانيه " + Timer;
            backGround.sprite = GameResourceManager.Instance.LoadSprite("Fantasy UI/TablesBackGround"+ SpriteName);
        }
    }
}