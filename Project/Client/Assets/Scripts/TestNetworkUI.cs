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
        RequestManager.Instance.Network.OnJoinedToRoom += Network_OnJoinedToRoom;
        EndTurn.onClick.AddListener(() => RequestManager.Instance.Network.JoinToRoom(500, false));
       
    }

    private void Network_OnJoinedToRoom(int GameID, string OtherPlayerInfo)
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
