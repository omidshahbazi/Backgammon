using Assets.Scripts.GamePlayLogic.EventSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TableThemeController : MonoBehaviour
{
    public Sprite Table_1_Background;
    public Sprite Table_2_Background;
    public Sprite Table_3_Background;
    public Sprite Table_4_Background;

    private SpriteRenderer tableBackGround;
    private void Awake()
    {
        tableBackGround = GetComponent<SpriteRenderer>();
        EventManager.OnTableDataUpdate += OnTableDataUpdate;
    }

    private void OnTableDataUpdate(int TableID)
    {
        if (tableBackGround == null)
            tableBackGround = GetComponent<SpriteRenderer>();

        switch (TableID)
        {
            case 1:
                if (Table_1_Background != null)
                    tableBackGround.sprite = Table_1_Background;
                break;
            case 2:
                if (Table_2_Background != null)

                    tableBackGround.sprite = Table_2_Background;
                break;
            case 3:
                if (Table_3_Background != null)
                    tableBackGround.sprite = Table_3_Background;
                break;
            case 4:
                if (Table_4_Background != null)
                    tableBackGround.sprite = Table_4_Background;
                break;
            default:
                if (Table_1_Background != null)
                    tableBackGround.sprite = Table_1_Background;
                break;
        }
    }
}
