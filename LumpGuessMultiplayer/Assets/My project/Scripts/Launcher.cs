using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject ConnectedScreen;
    [SerializeField] GameObject DisconnectedScreen;

    public void OnClickConnectButton()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        ConnectedScreen.SetActive(true);
    }
}
