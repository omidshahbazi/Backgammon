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
        public static bool isDiceRolled = false;
        public UIButton OnRollDice;
        public SpriteRenderer FirstDiceSprite;
        public SpriteRenderer SecondDiceSprite;
        public Sprite[] DiceSprites;
        
        private int Dice1Value = 1;
        private int Dice2Value =1;

        private int minRoll =4;
        private int maxRoll =20;
        private float minflipInterval = 0.1F;
        private float maxFlipInerval = 0.4F;

        private void Awake()
        {
            SimulationManager.Instance.OnDiceRolled += OnDiceChanged;
            OnRollDice.onClick.AddListener(RollTheDice);
          
        }
       
        private void RollTheDice()
        {
            OnRollDice.enabled = false;
            StartCoroutine(Roll());
            isDiceRolled = true;
          
        }

        private IEnumerator Roll()
        {
         
            int rollCount = UnityEngine.Random.Range(minRoll, maxRoll); 

            for(;rollCount>=0;--rollCount)
            {
                FirstDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
                SecondDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
                yield return new WaitForSeconds(UnityEngine.Random.Range(minflipInterval,maxFlipInerval));
            }

            FirstDiceSprite.sprite = DiceSprites[Dice1Value - 1];
            SecondDiceSprite.sprite = DiceSprites[Dice2Value - 1];
          
        }
   

        private void OnDiceChanged(int Dice1Value, int Dice2Value)
        {
            OnRollDice.enabled = true;
            this.Dice1Value = Dice1Value;   
            this.Dice2Value = Dice2Value;
            isDiceRolled = false;
                                  
        }
    }

}