using ClientUtilities.UI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation.Data.Event;
using System;

namespace Assets.Scripts.GamePlayLogic.UI
{

    public class InGameUI : MonoBehaviour
    {
   
        public UIButton EndTurn;
        public UIButton UndoAction;
        public Text PlayerTurn;

        private void Start()
        {
            EndTurn.onClick.AddListener(OnChangeTurnClick);
            UndoAction.onClick.AddListener(OnUndoActionClick);
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            SimulationManager.Instance.OnTableReady += Instance_OnTableReady;
        }

        private void Instance_OnTableReady()
        {
            PlayerTurn.text = SimulationManager.Instance.CurrentSimulator.Frame.Board.TurnColor.ToString();
        }

        private void OnDiceChanged()
        {
            PlayerTurn.text = SimulationManager.Instance.CurrentSimulator.Frame.Board.TurnColor.ToString();
        }

        private void OnChangeTurnClick()
        {
            //OnChangeTurnEventClick?.Invoke();
        }

        private void OnUndoActionClick()
        {
           // OnUndoEventClick?.Invoke();
            //SimulationManager.Instance.UndoActions()
        }
    }
}