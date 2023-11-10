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
    [SerializeField] GameObject ResultBox;

    [SerializeField] Text QuestionText;
    [SerializeField] Text SummaryQuestionText;
    [SerializeField] Text SummaryAnswer;

    [SerializeField] InputField AnswerTF;
    [SerializeField] Button SubmitAnswerButton;

    [SerializeField] GameObject CheckForWinnerButton;

    [SerializeField] GameObject WaitingForOtherPlayerImage;

    [SerializeField] Questions[] questions;

    [SerializeField] InputField BetAmount;

    [SerializeField] GameObject Bet10;
    [SerializeField] GameObject Bet25;
    [SerializeField] GameObject Bet50;
    int betAmount;

    List<Player> players;
    List<Player> winner;

    int questionIndex;

    [SerializeField] Text ListOfPlayer;

    [SerializeField] Text Name1;
    [SerializeField] Text Name2;

    [SerializeField] Text Player1NameSummaryScreen;
    [SerializeField] Text Player1BetSummaryScreen;
    [SerializeField] Text Player1AnswerSummaryScreen;

    [SerializeField] Text Player2NameSummaryScreen;
    [SerializeField] Text Player2BetSummaryScreen;
    [SerializeField] Text Player2AnswerSummaryScreen;

    private void Start()
    {
        questionIndex = 2;
        betAmount = 0;

        players = new List<Player>();
        winner = new List<Player>();

        SpawnPlayer();
        /*if (PhotonNetwork.IsMasterClient)
        {
            ShowQuestionButton.SetActive(true);
        }
        else
        {
            ShowQuestionButton.SetActive(false);
        }*/
        OnClickDisplayQuestion();
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
        SummaryQuestionText.text = questions[questionIndex].Question;
        SummaryAnswer.text = questions[questionIndex].Answer.ToString();

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
        SummaryQuestionText.text = questions[questionIndex].Question;
        SummaryAnswer.text = questions[questionIndex].Answer.ToString();
    }
    private void Update()
    {
        if(AnswerTF.text.Length > 0 && betAmount > 0)
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
        WaitingForOtherPlayerImage.SetActive(true);
        //pv.RPC("SendToMasterClient", RpcTarget.All, Answer.text, PhotonNetwork.LocalPlayer.ActorNumber);
        /*if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are the master client. Cannot send to yourself.");
            //SendToMasterClient(AnswerTF.text, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
            photonView.RPC("SendToMasterClient", RpcTarget.MasterClient, AnswerTF.text, betAmount, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
        }
        else*/
        {
            //PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SendToMasterClient", RpcTarget.MasterClient, AnswerTF.text, betAmount, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
        }
    }
    [PunRPC]
    public void SendToMasterClient(string answer, int actorNumber, int bet, string name)
    {
        //We are into master client
        /*Player p = new Player();
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
        #endregion*/
        bool exist = false;
        foreach(Player pl in players)
        {
            if(pl.actorNumber == actorNumber)
            {
                exist = true;
                pl.answer = int.Parse(answer);
                pl.difference = CheckForDifference(pl.answer, questions[questionIndex].Answer);
                pl.betAmount = bet;

                ListOfPlayer.text = ListOfPlayer.text + "\n" + "name: " + pl.name + "Answer" + pl.answer;
                photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
            }
        }
        if (!exist)
        {
            Player pl = new Player();
            pl.actorNumber = actorNumber;
            pl.name = name;
            pl.answer = int.Parse(answer);
            pl.difference = CheckForDifference(pl.answer, questions[questionIndex].Answer);
            pl.betAmount = bet;
            players.Add(pl);

            photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
        }

        /*foreach (Player p in players)
        {
            if(p.actorNumber == actorNumber)
            {
                p.answer = int.Parse(answer);
                p.difference = CheckForDifference(p.answer, questions[questionIndex].Answer);

                ListOfPlayer.text = ListOfPlayer.text + "\n" + "name: " + p.name + "Answer" + p.answer;
                photonView.RPC("RecieveFromMasterClient", RpcTarget.All, p.answer, p.actorNumber, questions[questionIndex].Answer, p.difference);
            }
        }*/
        //QuestionText.text = "Number: " + p.answer + " Send by: " + p.actorNumber + "-Bet: " + p.betAmount;
        //photonView.RPC("RecieveFromMasterClient", RpcTarget.All, p.answer, p.actorNumber, questions[questionIndex].Answer, CheckForDifference(p.answer, questions[questionIndex].Answer));
        //CheckForTheWinner();
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
        WaitingForOtherPlayerImage.SetActive(false);
        photonView.RPC("ShortListWinner", RpcTarget.MasterClient, null);
    }
    [PunRPC] 
    public void ShortListWinner()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        QuestionText.text = "";
        int diff = Int32.MaxValue;
        int actorNumber = 0;

        ListOfPlayer.text = "We are here";

        foreach (Player p in players)
        {
            if (p.difference < diff)
            {
                actorNumber = p.actorNumber;
                diff = p.difference;
                //winner.Add(p);
            }
            ListOfPlayer.text = ListOfPlayer + "\n" + "Name: " + p.name + "- Difference: " + p.difference;
        }

        foreach (Player p in players)
        {
            if (p.actorNumber == actorNumber) //To Add the one that was found the smallest in the previous loop
            {
                winner.Add(p);
            }
            if (p.difference == diff) //To find any other with same differences
            {
                winner.Add(p);
            }
        }
        bool win = false;
        foreach (Player pl in players)
        {
            win = false;
            foreach (Player p in winner)
            {
                if(p.actorNumber == pl.actorNumber)
                {
                    win = true;
                    photonView.RPC("AnnounceWinner", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
                    Debug.Log("MMaster: Winner is: " + p.name);
                }
            }
            if (!win)
            {
                photonView.RPC("AnnounceLoser", RpcTarget.All, pl.actorNumber, pl.answer, questions[questionIndex].Answer, CheckForDifference(pl.answer, questions[questionIndex].Answer), pl.name);
                Debug.Log("MMaster: Loose is: " + pl.name);
            }
        }
    }

    [PunRPC]
    public void AnnounceWinner(int actorNumber, int answer, int correctAnswer, int difference, string name)
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = "You  Win" + name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + " WINNER!!!";
            QuestionBox.SetActive(false);
            ResultBox.SetActive(true);
        }

        /*else
        {
            QuestionText.text = name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + "Wins";
        }*/
    }
    [PunRPC]
    public void AnnounceLoser(int actorNumber, int answer, int correctAnswer, int difference, string name)
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = "You  Loose" + name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + " WINNER!!!";
            QuestionBox.SetActive(false);
            ResultBox.SetActive(true);
        }
        /*else
        {
            QuestionText.text = name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + "Wins";
        }*/
    }

    public void AddToRoom()
    {
        photonView.RPC("AddToMasterClient", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
    }
    [PunRPC]
    public void AddToMasterClient(int actorNumber, string name)
    {
       /* bool exist = false;
        foreach (Player pl in players)
        {
            if(pl.actorNumber == actorNumber)
            {
                exist = true;
            }
        }
        if (!exist)*/
        {
            Player p = new Player();
            p.actorNumber = actorNumber;
            p.name = name;
            players.Add(p);
            ListOfPlayer.text = "";

            if (photonView.IsMine)
            {
                Name1.text = PhotonNetwork.NickName;
            }
            else
            {
                Name1.text = pv.Owner.NickName;
            }
        }
        
        foreach (Player pl in players)
        {
            ListOfPlayer.text = ListOfPlayer.text + "\n" + "Name: " + pl.name + " ActorNumber: " + pl.actorNumber;
            Debug.Log("MMaster: ------------");
            Debug.Log("MMaster: Player Name: " + pl.name);
        }
        //ListOfPlayer.text = ListOfPlayer.text + "/n" + "Name: " + name + " ActorNumber: " + actorNumber;
    }
    
    public void OnClickSetBet()
    {
        photonView.RPC("SetBet", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, int.Parse(BetAmount.text));
    }
    [PunRPC]
    public void SetBet(int actorNumber, int betAmount)
    {
        foreach(Player p in players)
        {
            if(p.actorNumber == actorNumber)
            {
                p.betAmount = betAmount;
                QuestionText.text = "\n" + p.actorNumber + " - Bet: " + betAmount;
            }
        }
    }

    public void OnBetSelect(int bet)
    {
        if(bet == 10)
        {
            Bet10.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = true;

            Bet25.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
            Bet50.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;

            betAmount = 10;
            Debug.Log("Bet 10");
        }
        else if (bet == 25)
        {
            Bet25.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = true;

            Bet10.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
            Bet50.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;

            betAmount = 25;
            Debug.Log("Bet 25");
        }
        else if (bet == 50)
        {
            Bet50.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = true;

            Bet10.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
            Bet25.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;

            betAmount = 50;
            Debug.Log("Bet 50");
        }
    }
}