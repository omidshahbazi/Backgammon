using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientUtilities.UI;


namespace Assets.Scripts.GamePlayLogic
{
    public class Dice : MonoBehaviour
    {
        public static Dice Instance
        {
            get;
            private set;
        }

        public bool IsPair
        {
            get
            {
                return this.Dice1Value == this.Dice2Value;
            }

        }


        public bool IsDiceRolled
        {
            get;
            private set;
        }

        public UIButton OnRollDice;
        public SpriteRenderer FirstDiceSprite;
        public SpriteRenderer SecondDiceSprite;
        public Sprite[] DiceSprites;

        private int Dice1Value = 1;
        private int Dice2Value = 1;

        private int minRoll = 4;
        private int maxRoll = 20;
        private float minflipInterval = 0.1F;
        private float maxFlipInerval = 0.4F;

        private void Awake()
        {
            Instance = this;
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            OnRollDice.onClick.AddListener(RollTheDice);
            OnDiceChanged();
        }

        private void RollTheDice()
        {
            OnRollDice.enabled = false;
            StartCoroutine(Roll());
        }

        private IEnumerator Roll()
        {

            int rollCount = UnityEngine.Random.Range(minRoll, maxRoll);

            for (; rollCount >= 0; --rollCount)
            {
                FirstDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
                SecondDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
                yield return new WaitForSeconds(UnityEngine.Random.Range(minflipInterval, maxFlipInerval));
            }

            FirstDiceSprite.sprite = DiceSprites[Dice1Value - 1];
            SecondDiceSprite.sprite = DiceSprites[Dice2Value - 1];
            IsDiceRolled = true;


        }


        private void OnDiceChanged()
        {

            OnRollDice.enabled = true;
            this.Dice1Value = SimulationManager.Instance.Simulator.Frame.Board.TurnDice.Dice1;
            this.Dice2Value = SimulationManager.Instance.Simulator.Frame.Board.TurnDice.Dice2;
            IsDiceRolled = false;
        }
    }

}