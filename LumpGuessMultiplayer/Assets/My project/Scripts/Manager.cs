using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Manager : MonoBehaviourPun
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] GameObject ShowQuestionButton;

    [SerializeField] PhotonView pv;

    [SerializeField] Text Question;

    private void Start()
    {
        SpawnPlayer();
        if (PhotonNetwork.IsMasterClient)
        {
            ShowQuestionButton.SetActive(true);
        }
        else
        {
            ShowQuestionButton.SetActive(false);
        }
    }
    void SpawnPlayer()
    {
        int r = Random.Range(-10, 10);
        Vector3 pos = new Vector3(PlayerPrefab.transform.position.x + r, PlayerPrefab.transform.position.y, PlayerPrefab.transform.position.z);
        PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
    }
    public void OnClickDisplayQuestion()
    {
        Question.text = "This is the question";
        pv.RPC("DisplayQuestion", RpcTarget.Others); //Not all because we are already flipping it locally
    }
    [PunRPC]
    void DisplayQuestion()
    {
        Question.text = "This is the question";
    }
}
