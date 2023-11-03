using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

[Serializable]
public class Questions
{
    [SerializeField] public string Question;
    [SerializeField] public int Answer;
}
[Serializable]
public class Player
{
    public string name;
    public int answer;
    public int betAmount;
    public int actorNumber;
    public int difference; //Set by master client only
    public bool replied;
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

    [SerializeField] GameObject CheckForWinnerButton;

    [SerializeField] Questions[] questions;

    List<Player> players;

    int questionIndex;

    private void Start()
    {
        questionIndex = 2;
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
        CheckForWinnerButton.SetActive(true);

        //questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
        //questionIndex = 0;
        QuestionText.text = questions[questionIndex].Question;

        QuestionBox.SetActive(true);

        pv.RPC("DisplayQuestion", RpcTarget.Others); //Not all because we are already flipping it locally
        pv.RPC("EnableQuestionBox", RpcTarget.Others); //Not all because we are already flipping it locally
    }
    [PunRPC]
    void DisplayQuestion()
    {
        //QuestionText.text = "This is the question";
        //questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
        QuestionText.text = questions[questionIndex].Question;
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
            SendToMasterClient(AnswerTF.text, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
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
        p.difference = CheckForDifference(p.answer, questions[questionIndex].Answer);

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
                    CheckForDifference(pl.answer, questions[questionIndex].Answer);
                }
            }
        }
        #endregion

        //QuestionText.text = "Number: " + p.answer + " Send by: " + p.actorNumber + "-Bet: " + p.betAmount;
        photonView.RPC("RecieveFromMasterClient", RpcTarget.All, p.answer, p.actorNumber, questions[questionIndex].Answer, CheckForDifference(p.answer, questions[questionIndex].Answer));
    }

    [PunRPC]
    public void RecieveFromMasterClient(int answer, int actorNumber, int correctAnswer, int difference)
    {
        if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = actorNumber + " Master client recived your answer: " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference;
        }
    }
    public int CheckForDifference(int givenAnswer, int correctAnswer)
    {
        return Math.Abs(givenAnswer - correctAnswer);
    }

    public void CheckForTheWinner()
    {
        int diff = players[0].difference;
        int actorNumber = players[0].actorNumber;
        foreach(Player p in players)
        {
            if(p.difference < diff)
            {
                actorNumber = p.actorNumber;
            }
        }
        foreach (Player p in players)
        {
            if (actorNumber == p.actorNumber)
            {
                photonView.RPC("AnnounceWinner", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
            }
        }
    }

    [PunRPC]
    public void AnnounceWinner(int actorNumber, int answer, int correctAnswer, int difference, string name)
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = "You " + name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + " WINNER!!!";
        }
        else
        {
            QuestionText.text = name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + "Wins";
        }
    }
}