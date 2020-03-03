using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.GamePlayLogic.RequestManagers;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.IAP;
using ClientUtilities.ResourceManager;
using ClientUtilities.UI;
using OnePF;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI.UIItems
{

    public class SimpleChatItem : MonoBehaviour
    {
        private int packID;
        private int index;
        private UIButton button;
        private RTLTextMeshPro rtlText;

        private void Awake()
        {
            button = GetComponent<UIButton>();
            rtlText = transform.FindDeep("Text").GetComponent<RTLTextMeshPro>();
            button.onClick.AddListener(SendChat);
        }


        public void SetData(string Text,int PackID ,int Index)
        {
            rtlText.text = Text;
            index = Index;
            packID = PackID;
        }

        private void SendChat()
        {
            ChatManager.Instance.SendSimpleChat(packID,index);
            UIManager.Instance.HideUI("ChatMenu");
        }

    }
}