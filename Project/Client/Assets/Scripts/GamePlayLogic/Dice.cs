using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientUtilities.UI;
using ClientUtilities.AudioMangaer;
using ClientUtilities.Tap;
using Assets.Scripts.GamePlayLogic.UI;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void DiceRolledFinished();
    public delegate void SelectedDiceChanged();
    public class Dice : MonoBehaviour
    {
        public event DiceRolledFinished OnDiceRolledFinished = null;
        public event SelectedDiceChanged OnSelectedDiceChanged = null;

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

        public int SelectedDice
        {
            get;
            private set;
        }

        public GameObject DiceRootObj;
        public GameObject FirstDiceFace;
        public GameObject SecondDiceFace;


        private Animator diceAnim;
        private SpriteRenderer firstDiceSprite;
        private SpriteRenderer secondDiceSprite;
        private SimulationManager simInstance;
        private Color selectedDiceColor;
        private Color unselectedDiceColor;
        //public Button FirstDiceButton;
        //public Button SecondDiceButton;
        public Sprite[] DiceSprites;

        private int Dice1Value = 1;
        private int Dice2Value = 1;

        //private int minRoll = 2;
        //private int maxRoll = 10;
        //private float minflipInterval = 0.05F;
        //private float maxFlipInerval = 0.2F;
        private Audio diceSound;
        private Vector3 myPos = new Vector3(3.48F, -0.83F, -2F);
        private Vector3 oPos = new Vector3(-1.7F, -0.83F, -2F);


        private void Awake()
        {
            Instance = this;
            simInstance = SimulationManager.Instance;
            firstDiceSprite = FirstDiceFace.GetComponent<SpriteRenderer>();
            secondDiceSprite = SecondDiceFace.GetComponent<SpriteRenderer>();
            selectedDiceColor = new Color(110/255F, 85/255F, 98/255F, firstDiceSprite.color.a );
            unselectedDiceColor = firstDiceSprite.color;
            diceAnim = DiceRootObj.GetComponent<Animator>();
            DiceRootObj.gameObject.SetActive(false);

        }

        private void OnEnable()
        {
            if (simInstance != null)
            {

                simInstance.OnDiceRolled += OnDiceChanged;
                simInstance.OnTableReady += Instance_OnTableReady;
              //  InGameMenu.OnChangeTurnEventClick += OnChangeTurn;
            }

            if (diceSound == null)
            {
                diceSound = AudioManager.Instance.Load("RollingDice", AudioManager.SoundTypes.Effect);
                diceSound.AutoUnload = false;
                diceSound.Volume = 100;
            }

            Tap.Instance.OnTapBegin += OnTap;

        }


        private void OnDisable()
        {
            if (simInstance != null)
            {
                simInstance.OnDiceRolled -= OnDiceChanged;
                simInstance.OnTableReady -= Instance_OnTableReady;
            }
            if (Tap.Instance != null)
                Tap.Instance.OnTapBegin -= OnTap;

        
         //   InGameMenu.OnChangeTurnEventClick -= OnChangeTurn;
        }

        //private void OnChangeTurn(bool IsRecivedFromNetwork)
        //{
          
        //}

        private void OnTap(Vector2 Position)
        {

            if (!IsDiceRolled || !TableManager.Instance.IsGameStarted || simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor )
                return;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Position), Vector2.zero);

            if (hit.collider == null)
            {
                return;
            }
            if (hit.collider.gameObject.tag == "Dice1")
                OnDiceOneClick();
            else if (hit.collider.gameObject.tag == "Dice2")
                OnDiceTwoClick();

        }



        public void RollTheDice(Action Action = null)
        {  
            StartCoroutine(Roll(Action));
            diceSound.Stop();
            diceSound.Play();
        }

        public void SetDiceState()
        {
            if (simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves == null || simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length == 0)
            {
                firstDiceSprite.color = secondDiceSprite.color = unselectedDiceColor;
                return;
            }
            if (simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length > 1)
            {
                SelectedDice = GameManager.Instance.GreaterDiceFirst == true ?
                        Mathf.Max(simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[0], simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[1]) :
                        Mathf.Min(simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[0], simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[1]);

            }
            else
            {
                SelectedDice = simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[0];
            }


            ChangeSelectedDiceColor();
            OnSelectedDiceChanged?.Invoke();
        }

        private IEnumerator Roll(Action Action = null)
        {
            DiceRootObj.gameObject.SetActive(true);
            firstDiceSprite.color = secondDiceSprite.color = unselectedDiceColor;

            diceAnim.SetBool("IsRolled", false);
            diceAnim.SetBool("IsRolled", true);
            IsDiceRolled = true;
            //int rollCount = UnityEngine.Random.Range(minRoll, maxRoll);

            //for (; rollCount >= 0; --rollCount)
            //{
            //    FirstDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
            //    SecondDiceSprite.sprite = DiceSprites[UnityEngine.Random.Range(0, DiceSprites.Length - 1)];
            //    yield return new WaitForSeconds(UnityEngine.Random.Range(minflipInterval, maxFlipInerval));
            //}


            yield return new WaitForSeconds(0.25F);


            // diceAnim.SetBool("IsRolled", false);
            //FirstDiceSprite.sprite = DiceSprites[Dice1Value - 1];
            //SecondDiceSprite.sprite = DiceSprites[Dice2Value - 1];

            OnDiceRolledFinished?.Invoke();
            firstDiceSprite.sprite = DiceSprites[Dice1Value - 1];
            secondDiceSprite.sprite = DiceSprites[Dice2Value - 1];
          

            ChangeSelectedDiceColor();
            Action?.Invoke();
        }

        private void ChangeSelectedDiceColor()
        {

            if (SelectedDice == Dice1Value)
            {
                firstDiceSprite.color = selectedDiceColor;
                secondDiceSprite.color = unselectedDiceColor;
            }
            else
            {
                firstDiceSprite.color = unselectedDiceColor;
                secondDiceSprite.color = selectedDiceColor;
            }
        }

        private void Instance_OnTableReady()
        {
            OnDiceChanged();
        }

        private void OnDiceChanged()
        {
            DiceRootObj.gameObject.SetActive(false);
            IsDiceRolled = false;
            if (simInstance.Board.TurnDice.Moves == null || simInstance.Board.TurnDice.Moves.Length == 0)
                return;

           
            this.Dice1Value = simInstance.Board.TurnDice.Moves[0];
            this.Dice2Value = simInstance.Board.TurnDice.Moves[1];
            SelectedDice = GameManager.Instance.GreaterDiceFirst == true ?
                            Mathf.Max(Dice1Value, Dice2Value) :
                            Mathf.Min(Dice1Value, Dice2Value);
      


            if (simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                DiceRootObj.transform.position = myPos;
            }
            else
            {
                DiceRootObj.transform.position = oPos;
            }
        }

        private void OnDiceOneClick()
        {
            if (!IsDiceRolled || simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor ||
               simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves == null || simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length == 0)
                return;
            SelectedDice = simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[0];
            ChangeSelectedDiceColor();
            OnSelectedDiceChanged?.Invoke();

        }

        private void OnDiceTwoClick()
        {
            if (!IsDiceRolled || simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor ||
            simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves == null || simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length == 0 ||
            simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length < 2)
                return;


            SelectedDice = simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves[1];
            ChangeSelectedDiceColor();
            OnSelectedDiceChanged?.Invoke();
        }


    }

}