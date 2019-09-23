using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum DiceState
{
    NotUsed,
    HalfUsed,
    FullyUsed
}

public class DiceController : MonoBehaviour
{
    public delegate void RolledDelegate(DiceController InDice, int InDots);
    public event RolledDelegate OnRolled;

    public delegate void DiceUsedDelegate(DiceController InDice, DiceState InState);
    public event DiceUsedDelegate OnUsed;

    public SpriteRenderer MySpriteRenderer;    
    [SerializeField]
    public Sprite[] Sprites =  new Sprite[6];
    /// <summary>
    /// Reference to GameManager component that exists on the scene.
    /// </summary>
    public GameManager GameManager;

    public int MinFlips = 5;
    public int MaxFlips = 15;
    public bool bConstantFlipTime = true;
    public float MinFlipTime = 0.1f;
    public float MaxFlipTime = 0.4f;
    public Sprite FadeHalf;
    public Sprite FadeFull;
    public SpriteRenderer FadeRenderer;
    protected int SpriteIndex = 0;
    protected bool bCanRoll = false;
    protected bool bFinishedRolling = true;
    protected DiceState State = DiceState.NotUsed;


    void Start()
    {
      
        if (GameManager == null)
        {
            GameManager = GameManager.Find();
        }

        if (GameManager != null)
        {
            GameManager.OnStateChanged += GameManager_OnStateChanged;
        }
  
        gameObject.SetActive(false);
    }

    private void GameManager_OnStateChanged(GameState InOldState, GameState InNewState)
    {        
        if (InNewState == GameState.RedPlayerRolls || InNewState == GameState.WhitePlayerRolls)
        {
            Reset();
        }

        if(InNewState != GameState.Init)
        {
            gameObject.SetActive(true);
        }
    }

    public void Roll()
    {
        if(bCanRoll)
        {
            State = DiceState.NotUsed;
            if (FadeRenderer != null)
            {
                FadeRenderer.gameObject.SetActive(false);
            }

            bCanRoll = false;
            bFinishedRolling = false;
            StartCoroutine(RollCoroutine());
        }
        else
        {
            
        }
    }

    public DiceState GetUsageState()
    {
        return State;
    }

    public void Use(bool bInWhole = true)
    {
        if(State == DiceState.NotUsed)
        {
            SetState(bInWhole ? DiceState.FullyUsed : DiceState.HalfUsed);
        }
        else
        {
            SetState(DiceState.FullyUsed);
        }

        if (OnUsed != null)
        {
            OnUsed(this, State);
        }
    }

    public IEnumerator RollCoroutine()
    {
        // initial delay, so dices won't start rolling at the same time
        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));

        // generating amount of flips this single dice will make
        int flips = Random.Range(MinFlips, MaxFlips + 1);

        // 
        int tempIndex = 0;
        for (; flips >= 0; --flips)
        {
            tempIndex = Random.Range(0, 6);
            MySpriteRenderer.sprite = Sprites[tempIndex];

            yield return new WaitForSeconds(bConstantFlipTime ? MinFlipTime : Random.Range(MinFlipTime, MaxFlipTime));
        }

        SpriteIndex = tempIndex;



        bFinishedRolling = true;

        if (OnRolled != null)
        {
            OnRolled(this, GetDots());
        }
    }

    public int GetDots()
    {
        return SpriteIndex + 1;
    }

    public void Reset()
    {
        bCanRoll = true;
        SetState(DiceState.NotUsed);
    }

    public bool HasFinishedRolling()
    {
        return bFinishedRolling;
    }

    protected void SetState(DiceState InState)
    {
        State = InState;

        if (FadeRenderer != null)
        {
            if (State == DiceState.NotUsed)
            {
                FadeRenderer.gameObject.SetActive(false);
            }
            else
            {
                FadeRenderer.sprite = State == DiceState.FullyUsed ? FadeFull : FadeHalf;
                FadeRenderer.gameObject.SetActive(true);
            }
        }
    }
}