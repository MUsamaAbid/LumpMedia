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
[Serializable]
public class Player
{
    public string name;
    public int answer;
    public int betAmount;
    public int actorNumber;
    public int difference; //Set by master client only
}
public class Manager : MonoBehaviourPun
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] GameObject ShowQuestionButton;

    [SerializeField] PhotonView pv;

    [SerializeField] GameObject QuestionBox;

    [SerializeField] Text QuestionText;

    [SerializeField] InputField AnswerTF;
    [SerializeField] Button SubmitAnswerButton;

    [SerializeField] Questions[] questions;

    List<Player> players;

    private void Start()
    {
        players = new List<Player>();

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
        if(AnswerTF.text.Length > 0)
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
            photonView.RPC("SendToMasterClient", RpcTarget.MasterClient, AnswerTF.text, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
        }
    }
    [PunRPC]
    public void SendToMasterClient(string answer, int actorNumber, string name)
    {
        //We are into master client
        Player p = new Player();
        p.actorNumber = actorNumber;
        p.answer = int.Parse(answer);
        p.name = name;
        p.betAmount = 10;

        #region If player already exists
        bool playerexist = false;
        foreach (Player pl in players)
        {
            if(pl.actorNumber == p.actorNumber)
            {
                playerexist = true;
            }
        }
        if (!playerexist)
        {
            players.Add(p);
        }
        else
        {
            foreach (Player pl in players)
            {
                if (pl.actorNumber == p.actorNumber)
                {
                    //pl.actorNumber = actorNumber;
                    pl.answer = int.Parse(answer);
                    pl.name = name;
                    pl.betAmount = 10;
                }
            }
        }
        #endregion

        QuestionText.text = "Number: " + p.answer + " Send by: " + p.actorNumber + "-Bet: " + p.betAmount;
        photonView.RPC("RecieveFromMasterClient", RpcTarget.Others, p.answer, p.actorNumber);
    }

    [PunRPC]
    public void RecieveFromMasterClient(int answer, int actorNumber)
    {
        if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = QuestionText.text + " - " + actorNumber + " Master client recived your answer: " + answer;
        }
    }
}