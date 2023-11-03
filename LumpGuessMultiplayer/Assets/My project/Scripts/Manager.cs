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

    [SerializeField] GameObject QuestionBox;

    [SerializeField] Text Question;

    [SerializeField] InputField Answer;
    [SerializeField] Button SubmitAnswerButton;

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
        int r = Random.Range(-8, 8);
        Vector3 pos = new Vector3(PlayerPrefab.transform.position.x + r, PlayerPrefab.transform.position.y - 3, PlayerPrefab.transform.position.z);
        PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
    }
    public void OnClickDisplayQuestion()
    {
        ShowQuestionButton.SetActive(false);
        Question.text = "This is the question";
        QuestionBox.SetActive(true);
        pv.RPC("DisplayQuestion", RpcTarget.Others); //Not all because we are already flipping it locally
        pv.RPC("EnableQuestionBox", RpcTarget.Others); //Not all because we are already flipping it locally
    }
    [PunRPC]
    void DisplayQuestion()
    {
        Question.text = "This is the question";
    }
    private void Update()
    {
        if(Answer.text.Length > 0)
        {
            SubmitAnswerButton.interactable = true;
        }
        else
        {
            SubmitAnswerButton.interactable = false;
        }
    }

    [PunRPC]
    void EnableQuestionBox()
    {
        QuestionBox.SetActive(true);
    }

    public void OnSubmitAnswerButton()
    {
        //pv.RPC("SendToMasterClient", RpcTarget.All, Answer.text, PhotonNetwork.LocalPlayer.ActorNumber);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are the master client. Cannot send to yourself.");
        }
        else
        {
            //PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SendToMasterClient", RpcTarget.MasterClient, Answer.text, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
    [PunRPC]
    public void SendToMasterClient(string number, int actorNumber)
    {
        Question.text = "Number: " + number + " Send by: " + actorNumber;
    }

    [PunRPC]
    public void RecieveFromMasterClient()
    {

    }
}