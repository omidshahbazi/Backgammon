using ClientUtilities.UI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simulation.Data.Event;
using System;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public delegate void UISendChangeTurnEvent();
    public delegate void UISendUndoActionEvent();
    public class InGameUI : MonoBehaviour
    {
        public static event UISendChangeTurnEvent OnChangeTurnEventClick = null;
        public static event UISendUndoActionEvent OnUndoEventClick = null;
        public UIButton EndTurn;
        public UIButton UndoAction;
        public Text PlayerTurn;

        private void Start()
        {
            EndTurn.onClick.AddListener(OnChangeTurnClick);
            UndoAction.onClick.AddListener(OnUndoActionClick);
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            PlayerTurn.text = SimulationManager.Instance.tempSimulator.Frame.Board.TurnColor.ToString();
        }

        private void OnDiceChanged()
        {
            PlayerTurn.text = SimulationManager.Instance.tempSimulator.Frame.Board.TurnColor.ToString();
        }

        private void OnChangeTurnClick()
        {
            OnChangeTurnEventClick?.Invoke();
        }

        private void OnUndoActionClick()
        {
            OnUndoEventClick?.Invoke();
            //SimulationManager.Instance.UndoActions()
        }
    }
}