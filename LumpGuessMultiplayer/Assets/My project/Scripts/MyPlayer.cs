using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MyPlayer : MonoBehaviourPun
{
    [SerializeField] Text PlayerName;
    [SerializeField] PhotonView pv;
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
     }
}
