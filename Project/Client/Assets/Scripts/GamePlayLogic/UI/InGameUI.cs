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

            EndTurn.onClick.AddListener(() => SimulationManager.Instance.Simulator.SendEvent(new FinishTurnEvent(SimulationManager.Instance.Shot.BoardData.TurnColor)));
            UndoAction.onClick.AddListener(() => SimulationManager.Instance.UndoActions());
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            PlayerTurn.text = SimulationManager.Instance.Shot.BoardData.TurnColor.ToString();
        }

        private void OnDiceChanged()
        {
            PlayerTurn.text = SimulationManager.Instance.Shot.BoardData.TurnColor.ToString();
        }
    }
}