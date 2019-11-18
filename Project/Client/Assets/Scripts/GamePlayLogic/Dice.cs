using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientUtilities.UI;
using ClientUtilities.AudioMangaer;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolledFinished();
    public class Dice : MonoBehaviour
    {
        public event DiceRolledFinished OnDiceRolledFinished = null;
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


        public Image FirstDiceSprite;
        public Image SecondDiceSprite;
        public Sprite[] DiceSprites;

        private int Dice1Value = 1;
        private int Dice2Value = 1;

        private int minRoll = 2;
        private int maxRoll = 10;
        private float minflipInterval = 0.05F;
        private float maxFlipInerval = 0.2F;
        private Audio diceSound;
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

            if(diceSound == null)
            {
                diceSound = AudioManager.Instance.Load("RollingDice", AudioManager.SoundTypes.Effect);
                diceSound.AutoUnload = false;
                diceSound.Volume = 100;
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
            diceSound.Stop();
            diceSound.Play();
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
            OnDiceRolledFinished?.Invoke();

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