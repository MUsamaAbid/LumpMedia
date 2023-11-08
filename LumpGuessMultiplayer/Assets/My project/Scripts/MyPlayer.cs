using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MyPlayer : MonoBehaviourPun
{
    [SerializeField] Text PlayerName;
    [SerializeField] PhotonView pv;

    [SerializeField] Manager manager;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            PlayerName.text = PhotonNetwork.NickName;
        }
        else
        {
            PlayerName.text = pv.Owner.NickName;
        }
        AddInTheList();
     }

    void AddInTheList()
    {
        if (!manager) manager = FindObjectOfType<Manager>();
        manager.AddToRoom();
    }
}
