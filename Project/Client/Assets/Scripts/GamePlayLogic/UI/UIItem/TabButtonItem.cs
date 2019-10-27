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
    public class TabButtonItem : MonoBehaviour
    {
 
        private UIButton button;
        private RTLTextMeshPro tabNameYext;
        private _2dxFX_GrayScale grayScaleEffect = null;


        private void Awake()
        {
            button = GetComponent<UIButton>();
            tabNameYext = transform.FindDeep("TabName").GetComponent<RTLTextMeshPro>();
            grayScaleEffect = GetComponent<_2dxFX_GrayScale>();
          
        }

        public void SetData(UnityAction OnClick,string TabName)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            tabNameYext.text = TabName;
        
        }

        public void SetEnableState(bool IsActive)
        {
            grayScaleEffect.enabled = !IsActive;
        }
    }
}