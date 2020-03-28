
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClientUtilities.UI
{

    public class CanvasScalerExtension : CanvasScaler
    {
        private Vector2 DefaultResulationPortrait = new Vector2(1080, 1920);
        private Vector2 DefaultResulationLandscape = new Vector2(1920, 1080);
        private Vector2 DefaultNeutralAscpect = new Vector2(1920, 1920);

        protected override void Awake()
        {
            base.Awake();
            SetData();
        }


//#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            SetData();
        }
//#endif

        private void SetData()
        {
           
            if (Screen.width > Screen.height)
            {
                referenceResolution = DefaultResulationLandscape;
                matchWidthOrHeight = 0F;
            }
            else if(Screen.width < Screen.height)
            {
                referenceResolution = DefaultResulationPortrait;
                matchWidthOrHeight = 1;
            }else
            {
                referenceResolution = DefaultNeutralAscpect;
                matchWidthOrHeight = 0.48F;
            }
        }

    }
}