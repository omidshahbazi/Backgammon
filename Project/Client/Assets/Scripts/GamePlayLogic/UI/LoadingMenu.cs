using Assets.Scripts.GamePlayLogic.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ClientUtilities.Extensions;
using RTLTMPro;
using Assets.Scripts.GamePlayLogic;
using System;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.UI;
using ClientUtilities.ResourceManager;
using Assets.Scripts.ClientUtilities.ScheduleSystem;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class LoadingMenu : UIBase
    {
        public static LoadingMenu Instance
        {
            get;
            private set;
        }

        private string WaitText = string.Empty;
        private RTLTextMeshPro text;
        private int dotIndex;
        private ScheduleObj obj;
        private RectTransform rect;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            text = transform.FindDeep("Text").GetComponent<RTLTextMeshPro>();
            rect = GetComponent<RectTransform>();
            Instance = this;
            base.SetUIRefrences();
        }


        public void ShowLoading(Transform Trans)
        {
            if (obj != null)
                obj.CancelSchedule();


            this.transform.SetParent(Trans);

            WaitText = GameDataManager.GetString("PleaseWait");
            this.gameObject.SetActive(true);
            this.transform.localScale = Vector3.one;
            rect.offsetMax = rect.offsetMin = Vector2.zero;
            //AnimateDots();
        }

        public void HideLoading()
        {
            this.gameObject.SetActive(false);
            //this.transform.SetParent(null);
            if (obj != null)
                obj.CancelSchedule();

        }

        private void AnimateDots()
        {

            switch (dotIndex)
            {
                case 1:
                    text.text = WaitText + ".";
                    break;
                case 2:
                    text.text = WaitText + "..";
                    break;
                case 3:
                    text.text = WaitText + "...";
                    break;
                default:
                    break;
            }

            if (++dotIndex > 3)
                dotIndex = 1;

            obj = ScheduleManager.Instance.AddSchedule(AnimateDots, 0.2F);
        }
    }
}