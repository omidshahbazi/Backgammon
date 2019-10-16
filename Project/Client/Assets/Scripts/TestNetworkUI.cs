using Assets.Scripts.GamePlayLogic.RequestManager;
using ClientUtilities.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetworkUI : MonoBehaviour
{
    public UIButton EndTurn;
  
    void Start()
    {
        RequestManager.Instance.InitilizeNetwork();
        EndTurn.onClick.AddListener(() => RequestManager.Instance.Network.JoinToRoom(500, false));
        RequestManager.Instance.Network.OnGameDataReady += Network_OnGameDataReady;
    }

    private void Network_OnGameDataReady(Simulation.Data.Game.PlayerColors Color)
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
