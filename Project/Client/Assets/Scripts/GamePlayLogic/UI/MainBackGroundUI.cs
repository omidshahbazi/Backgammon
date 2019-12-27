using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;
using ClientUtilities.UI;
using RTLTMPro;
using System;
using Assets.Scripts.GamePlayLogic.UserData;
using UnityEngine.UI;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class MainBackGroundUI : UIBase
    {
        private Image image;
        private _2dxFX_Heat heatEffect;
        private _2dxFX_Smoke smokeEffect;

        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;

            if (RequestManager.Instance != null)
            {
                RequestManager.Instance.OnMatchFound += Instance_OnMatchFound;
            }

            heatEffect = this.GetComponent<_2dxFX_Heat>();
            smokeEffect = this.GetComponent<_2dxFX_Smoke>();
            image = GetComponent<Image>();
            base.SetUIRefrences();
        }

  

        protected override void Awake()
        {
            base.Awake();
        }


        private void Instance_OnGameFinished(Simulation.Data.Game.PlayerColors WinnerColor, GameFinishReasons Reason, int Score)
        {
            if (SimulationManager.Instance != null)
                SimulationManager.Instance.OnGameFinished -= Instance_OnGameFinished;

            image.enabled = true;
            //Use this for test
            PopupTextMenu.Instance.ShowPopUpText(Reason.ToString());
            LeanTween.value(this.gameObject, smokeEffect._Value2, 0, GameManager.Instance.StartGameDelay - 0.5F).setOnUpdate(OnUpdate).setOnComplete(() =>
            {

                heatEffect.enabled = true;
                smokeEffect.enabled = false;
                object obj1 = (Simulation.Data.Game.PlayerColors)WinnerColor;
                object obj2 = (ushort)TableManager.Instance.SelectedTable.Prize;
                UIManager.Instance.ShowUI("EndGameResultMenu", obj1, obj2);

            });
        }

        private void Instance_OnMatchFound()
        {
            if (SimulationManager.Instance != null)
                SimulationManager.Instance.OnGameFinished+= Instance_OnGameFinished;

            heatEffect.enabled = false;
            smokeEffect.enabled = true;
            LeanTween.value(this.gameObject, smokeEffect._Value2, 1, GameManager.Instance.StartGameDelay - 0.5F).setOnUpdate(OnUpdate).setOnComplete(() =>
            {
                image.enabled = false;
            });

        }

        private void OnUpdate(float Value)
        {
            smokeEffect._Value2 = Value;
        }


    }
}