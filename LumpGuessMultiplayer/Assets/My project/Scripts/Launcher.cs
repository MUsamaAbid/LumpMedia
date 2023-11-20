using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject ConnectedScreen;
    [SerializeField] GameObject DisconnectedScreen;

    [SerializeField] GameObject BgNameScreen;
    [SerializeField] GameObject BgRoomSelectionScreen;

    [SerializeField] GameObject BanACategoryScreen;

    [SerializeField] Text playerNameText;

    private void Start()
    {
        Application.targetFrameRate = 20;
        if (PhotonNetwork.IsConnected)
        {
            DisconnectedScreen.SetActive(false);
            BgNameScreen.SetActive(false);
            ConnectedScreen.SetActive(true);
            BgRoomSelectionScreen.SetActive(true);
            PhotonNetwork.ConnectUsingSettings();
            playerNameText.text = PhotonNetwork.NickName;
            // You can perform additional actions here if needed
        }
    }
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
        string roomName = "Room" + Random.Range(1, 1000);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
    }
}