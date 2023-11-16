using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;

public class OnLeaveRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] Manager manager;
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        manager.EndGame();
    }
}
