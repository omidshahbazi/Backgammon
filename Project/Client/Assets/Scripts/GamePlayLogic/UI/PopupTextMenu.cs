

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
    public class PopupTextMenu : UIBase
    {
        public static PopupTextMenu Instance
        {
            get;
            private set;
        }

        private GameObject mainRtlText;
        private GameObject coinGameObject;
        private List<RTLTextMeshPro> popuptexts = new List<RTLTextMeshPro>();
        private List<GameObject> coinEffects = new List<GameObject>();
        private Vector3 destPos;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void SetUIRefrences()
        {
            base.SetUIRefrences();
            Instance = this;
            mainRtlText = transform.FindDeep("Text - RTLTMP",true).gameObject;
            for (int i = 0; i < 8; ++i)
            {
                GameObject go = Instantiate(mainRtlText, Vector3.zero, Quaternion.identity);
                popuptexts.Add(go.GetComponent<RTLTextMeshPro>());
                go.gameObject.SetActive(false);
                go.transform.SetParent(this.gameObject.transform);
            }

            //for (int i = 0; i < 10; ++i)
            //{
            //    GameObject go = Instantiate(coinGameObject, Vector3.zero, Quaternion.identity);
            //    coinEffects.Add(go);
            //    go.gameObject.SetActive(false);
            //    go.transform.SetParent(this.gameObject.transform);
            //}
            destPos = new Vector3(mainRtlText.transform.transform.position.x, mainRtlText.transform.transform.position.y + 200, mainRtlText.transform.transform.position.z);
           
        }


        //protected override void Update()
        //{
        //    base.Update();

        //    if (Input.GetKeyDown(KeyCode.W))
        //        ShowPopUpText("Fuck");
        //}
        //public void ShowCoinEffect(Vector3 orgin, Vector3 destination,string Text)
        //{
        //    for(int i =0;i<10;++i)
        //    {
        //        if (coinEffects[i].gameObject.activeSelf)
        //        {

        //        }
        //        LeanTween.value( orgin,destination,0.5F)
        //    }
        //    ShowPopUpText(Text);
        //}

        public void ShowPopUpText(string Text)
        {
            this.gameObject.transform.SetAsLastSibling();
            int resrveIndex = -1;
            for (int i = 0; i < popuptexts.Count; ++i)
            {
                if (popuptexts[i].gameObject.activeSelf)
                    continue;
                resrveIndex = i;
                break;
            }

            RTLTextMeshPro goRTL = null;
            if (resrveIndex == -1)
            {
                GameObject go = Instantiate(mainRtlText, Vector3.zero, Quaternion.identity);
                popuptexts.Add(goRTL = go.GetComponent<RTLTextMeshPro>());
                go.transform.SetParent(this.gameObject.transform);

                go.gameObject.SetActive(false);
            }
            else
                goRTL = popuptexts[resrveIndex];

            goRTL.gameObject.transform.position = mainRtlText.transform.position;
            goRTL.gameObject.SetActive(true);
            goRTL.text = Text;
            LeanTween.scale(goRTL.gameObject, goRTL.transform.localScale*2F, 1F).setLoopOnce().setEase(LeanTweenType.punch);
            LeanTween.move(goRTL.gameObject, destPos, 3F);
            LeanTween.value(goRTL.gameObject, 1.0F, 0.0F, 3F).setOnUpdate(
                (value) =>
                {
                    goRTL.color = new Color(goRTL.color.r, goRTL.color.g, goRTL.color.b, value);
                }).setOnComplete(() =>
                {
                    goRTL.gameObject.SetActive(false);
                });
        }

     
    }
}