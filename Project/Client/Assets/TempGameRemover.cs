using Assets.Scripts.GamePlayLogic.RequestManagers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempGameRemover : MonoBehaviour
{
    Image image;
    // Start is called before the first frame update
    void Start()
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.OnMatchFound += Instance_OnMatchFound;
        image = GetComponent<Image>();
    }

    private void Instance_OnMatchFound()
    {
        image.enabled = false;
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
