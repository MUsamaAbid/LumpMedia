using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

[Serializable]
public class Questions
{
    [SerializeField] string Question;
    [SerializeField] int Answer;
}
public class Answer
{
    int actorNumber;
    string name;
    int answer;
    int difference; //Set by master client
    int betAmount;
}
public class Manager : MonoBehaviourPun
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] GameObject ShowQuestionButton;

    [SerializeField] PhotonView pv;

    [SerializeField] GameObject QuestionBox;

    [SerializeField] Text QuestionText;

    [SerializeField] InputField Answer;
    [SerializeField] Button SubmitAnswerButton;

    [SerializeField] Questions[] questions;

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
        int r = UnityEngine.Random.Range(-8, 8);
        Vector3 pos = new Vector3(PlayerPrefab.transform.position.x + r, PlayerPrefab.transform.position.y - 3, PlayerPrefab.transform.position.z);
        PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
    }
    public void OnClickDisplayQuestion()
    {
        ShowQuestionButton.SetActive(false);
        QuestionText.text = "This is the question";
        QuestionBox.SetActive(true);
        pv.RPC("DisplayQuestion", RpcTarget.Others); //Not all because we are already flipping it locally
        pv.RPC("EnableQuestionBox", RpcTarget.Others); //Not all because we are already flipping it locally
    }
    [PunRPC]
    void DisplayQuestion()
    {
        QuestionText.text = "This is the question";
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
        QuestionText.text = "Number: " + number + " Send by: " + actorNumber;
        photonView.RPC("RecieveFromMasterClient", RpcTarget.Others, number, actorNumber);
    }

    [PunRPC]
    public void RecieveFromMasterClient(string answer, int actorNumber)
    {
        if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = QuestionText.text + " - " + actorNumber + " Master client recived your answer: " + answer;
        }
    }
}