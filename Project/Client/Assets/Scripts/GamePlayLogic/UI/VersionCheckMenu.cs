using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using System;
using ClientUtilities.UI;
using Assets.Scripts.GamePlayLogic.UserData;
using Assets.Scripts.GamePlayLogic.UI.UIItems;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class VersionCheckMenu : UIBase
    {
        private VersionCheckResults Result;

        private UIButton UnderMaintancebutton;
        private UIButton GeNewVersiontbutton;
        private UIButton ForceGetNewVersionbutton;
        private UIButton Confirmbutton;
        private string URL;
        private Action OnConfirmButton;

        private GameObject underMaintenanceOBJ;
        private GameObject newVersionOBJ;
        private GameObject forceVersionOBJ;

        protected override void Awake()
        {
            base.Awake();
        }


       
        public override void ShowUI(params object[] Args)
        {
            if (Args != null && Args.Length != 0)
            {
                Result = (VersionCheckResults)Args[0];

                if (Args.Length > 1)
                    URL = (string)Args[1];

                if (Args.Length > 2)
                    OnConfirmButton = (Action)Args[2];
            }

            base.ShowUI(Args);


            switch (Result)
            {
                case VersionCheckResults.UnderMaintenance:
                    underMaintenanceOBJ.gameObject.SetActive(true);
                    break;

                case VersionCheckResults.NewerVersionAvailable:
                    newVersionOBJ.gameObject.SetActive(true);
                    break;
                case VersionCheckResults.UpdateNeeded:
                    forceVersionOBJ.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }

        }

        private void GetNewVersion()
        {
            Application.OpenURL(URL);
        }

        private void Quit()
        {
            Application.Quit();
        }

        private void OnConfirmButtonClick()
        {
            OnConfirmButton?.Invoke();
            HideUI();
        }

        public override void HideUI()
        {
            base.HideUI();
        }

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;
         

            UnderMaintancebutton = transform.FindDeep("QuitTheGame").GetComponent<UIButton>();
            GeNewVersiontbutton = transform.FindDeep("UpdateButton").GetComponent<UIButton>();
            ForceGetNewVersionbutton = transform.FindDeep("GoToTheGame").GetComponent<UIButton>();
            Confirmbutton = transform.FindDeep("ForceUpdateButton").GetComponent<UIButton>();


            underMaintenanceOBJ = transform.FindDeep("UnderMaintenance").gameObject;
            newVersionOBJ = transform.FindDeep("NewVersionAvalaible").gameObject;
            forceVersionOBJ = transform.FindDeep("ForceVersion").gameObject;
            UnderMaintancebutton.onClick.AddListener(Quit);
            GeNewVersiontbutton.onClick.AddListener(GetNewVersion);
            ForceGetNewVersionbutton.onClick.AddListener(GetNewVersion);
            Confirmbutton.onClick.AddListener(OnConfirmButtonClick);
            base.SetUIRefrences();
        }
    }
}