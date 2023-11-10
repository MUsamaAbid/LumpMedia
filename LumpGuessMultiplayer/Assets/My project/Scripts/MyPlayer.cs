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
    public void SetParentAcrossNetwork(int parentViewID)
    {
        photonView.RPC("SetParent", RpcTarget.AllBuffered, parentViewID);
    }
    [PunRPC]
    void SetParent(int parentViewID)
    {
        // Find the parent object using the PhotonView ID
        PhotonView parentView = PhotonView.Find(parentViewID);
        if (parentView != null)
        {
            // Set the parent relationship
            transform.parent = parentView.transform;
        }
    }
    public void AssignPosition(int x, int y, int z)
    {
        //transform.localPosition = new Vector3(x, y, z);
        photonView.RPC("SetPosition", RpcTarget.AllBuffered, x, y, z);
    }
    [PunRPC]
    void SetPosition(int x, int y, int z)
    {
        Debug.Log("MMM: Setting position");
        transform.localPosition = new Vector3(x, y, z);
    }
}
