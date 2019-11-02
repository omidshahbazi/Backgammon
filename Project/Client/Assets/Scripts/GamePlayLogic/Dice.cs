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

        private SimulationManager simInstance;

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


        public SpriteRenderer FirstDiceSprite;
        public SpriteRenderer SecondDiceSprite;
        public Sprite[] DiceSprites;

        private int Dice1Value = 1;
        private int Dice2Value = 1;

        private int minRoll = 4;
        private int maxRoll = 15;
        private float minflipInterval = 0.1F;
        private float maxFlipInerval = 0.3F;

        private void Awake()
        {
            Instance = this;
            simInstance = SimulationManager.Instance;



        }

        private void OnEnable()
        {
            if (simInstance != null)
            {

                simInstance.OnDiceRolled += OnDiceChanged;
                simInstance.OnTableReady += Instance_OnTableReady;
            }

        }

        private void OnDisable()
        {
            if (simInstance != null)
            {
                simInstance.OnDiceRolled -= OnDiceChanged;
                simInstance.OnTableReady -= Instance_OnTableReady;
            }

        }


        public void RollTheDice()
        {
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

        private void Instance_OnTableReady()
        {
            OnDiceChanged();
        }

        private void OnDiceChanged()
        {
            if (simInstance.Board.TurnDice.Moves == null || simInstance.Board.TurnDice.Moves.Length == 0)
                return;

            this.Dice1Value = simInstance.Board.TurnDice.Moves[0];
            this.Dice2Value = simInstance.Board.TurnDice.Moves[1];
            IsDiceRolled = false;
        }
    }

}