using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerName : MonoBehaviour
{
    [SerializeField] InputField PlayerNameTF;
    [SerializeField] Button SetPlayerNameButton;

    [SerializeField] Text playerNameText;
    public void OnPlayerNameTFChange()
    {
        if(PlayerNameTF.text.Length > 3)
        {
            SetPlayerNameButton.interactable = true;
        }
        else
        {
            SetPlayerNameButton.interactable = false;
        }
    }
    public void OnClickSetPlayerName()
    {
        PhotonNetwork.NickName = PlayerNameTF.text;
        playerNameText.text = PhotonNetwork.NickName;
    }
}
