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
        public Text PlayerTurn;

        private void Start()
        {

            EndTurn.onClick.AddListener(() => SimulationManager.Instance.Simulator.SendEvent(new FinishTurnEvent(SimulationManager.Instance.Simulator.Board.TurnColor)));
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            PlayerTurn.text = SimulationManager.Instance.Simulator.Board.TurnColor.ToString();
        }

        private void OnDiceChanged(int Dice1Value, int Dice2Value)
        {
            PlayerTurn.text = SimulationManager.Instance.Simulator.Board.TurnColor.ToString();
        }
    }
}