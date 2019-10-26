
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
           
            if (Screen.width >= Screen.height)
            {
                referenceResolution = DefaultResulationLandscape;
                matchWidthOrHeight = 0.48F;
            }
            else
            {
                referenceResolution = DefaultResulationPortrait;
                matchWidthOrHeight = 0;
            }
        }

    }
}