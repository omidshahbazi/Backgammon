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
    public class TableItem : MonoBehaviour
    {
        private UIButton button;
        private RTLTextMeshPro tableNametxt;
        private RTLTextMeshPro pricetxt;
        private RTLTextMeshPro rewardTxt;

        private void Awake()
        {
            button = GetComponent<UIButton>();
            tableNametxt = transform.FindDeep("TableName").GetComponent<RTLTextMeshPro>();
            pricetxt = transform.FindDeep("Price").GetComponent<RTLTextMeshPro>();
            rewardTxt = transform.FindDeep("Reward").GetComponent<RTLTextMeshPro>();
        }

        public void SetData(UnityAction OnClick,string TableName,string Price,string Reward)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            tableNametxt.text = TableName;
            pricetxt.text = "ورودي" + Price;
            rewardTxt.text =   "جايزه" + Reward;

        }
    }
}