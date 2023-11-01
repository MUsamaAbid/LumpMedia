using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class UIHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] InputField CreateRoomTF;
    [SerializeField] InputField JoinRoomTF;

    public void OnClickCreateRoom()
    {
        PhotonNetwork.CreateRoom(CreateRoomTF.text, new RoomOptions { MaxPlayers = 6 }, null);
    }
    public void OnClickJoinRoom()
    {
        PhotonNetwork.JoinRoom(JoinRoomTF.text, null); 
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Room joinedddd");
        PhotonNetwork.LoadLevel(1);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room joining failed: " + message);
    }
}
