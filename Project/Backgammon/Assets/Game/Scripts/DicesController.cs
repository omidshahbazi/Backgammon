using UnityEngine;
using System.Collections.Generic;

public class DicesController : MonoBehaviour
{
    [SerializeField]
    public DiceController[] Dices;
   
    public GameManager GameManager;
    public SpriteRenderer MySpriteRenderer;

    void Start ()
    {
      
      

        if(GameManager)
            GameManager.OnStateChanged += GameManager_OnStateChanged;

    }

    private void GameManager_OnStateChanged(GameState InOldState, GameState InNewState)
    {
        if(MySpriteRenderer && (InNewState == GameState.RedPlayerRolls || InNewState == GameState.WhitePlayerRolls))
        {
            MySpriteRenderer.enabled = true;
        }
    }

    void OnMouseDown()
    {
        for(int i = 0; i< Dices.Length;++i)
        {
            Dices[i].Roll();
        }

        if(MySpriteRenderer)
        {
            MySpriteRenderer.enabled = false;
        }
    }
}