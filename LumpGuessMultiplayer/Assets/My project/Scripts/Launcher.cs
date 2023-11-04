using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        ConnectedScreen.SetActive(false);
        DisconnectedScreen.SetActive(true);
    }

    public override void OnJoinedLobby()
    {
        DisconnectedScreen.SetActive(false);
        ConnectedScreen.SetActive(true);
    }
    public void OnClickJoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode + " - Room Not Joined: " + message);
    }
}
