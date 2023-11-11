using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using System.Drawing;

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
    public int betAmount = 0;
    public int actorNumber;
    public int difference; //Set by master client only
    public bool replied;
}
public class Manager : MonoBehaviourPun
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] GameObject ShowQuestionButton;

    [SerializeField] GameObject Slot1;
    [SerializeField] GameObject Slot2;

    [SerializeField] PhotonView pv;

    [SerializeField] GameObject PvPScreen;
    [SerializeField] GameObject QuestionBox;
    [SerializeField] GameObject SummaryScreen;
    [SerializeField] GameObject ResultScreen;

    [SerializeField] Text QuestionText;
    [SerializeField] Text SummaryQuestionText;
    [SerializeField] Text SummaryAnswer;
    [SerializeField] Text WhoWonText;
    [SerializeField] Text BetAmountWin;

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

    [SerializeField] GameObject Player1Summary;
    [SerializeField] Text Player1NameSummaryScreen;
    [SerializeField] Text Player1BetSummaryScreen;
    [SerializeField] Text Player1AnswerSummaryScreen;

    [SerializeField] GameObject Player2Summary;
    [SerializeField] Text Player2NameSummaryScreen;
    [SerializeField] Text Player2BetSummaryScreen;
    [SerializeField] Text Player2AnswerSummaryScreen;

    [SerializeField] GameObject FightAgainButton;

    [SerializeField] Text RoundText;
    int round;

    private void Start()
    {
        round = 1;
        RoundText.text = "ROUND " + round.ToString();
        //questionIndex = 2;
        betAmount = 0;

        players = new List<Player>();
        winner = new List<Player>();

        SpawnPlayer();

        Player1Summary.SetActive(false);
        Player2Summary.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            FightAgainButton.SetActive(true);
        }
        else
        {
            FightAgainButton.SetActive(false);
        }

        OnClickDisplayQuestion();
    }
    void SpawnPlayer()
    {
        Vector3 pos;
        if (PhotonNetwork.IsMasterClient)
        {
            pos = new Vector3(0, 0, 0);
            GameObject g = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
            g.GetComponent<MyPlayer>().SetParentAcrossNetwork(Slot1.GetComponent<PhotonView>().ViewID);
            g.GetComponent<MyPlayer>().AssignPosition(-200, 76, 0);
        }
        else
        {
            pos = new Vector3(0, 0, 0);
            GameObject g = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
            g.GetComponent<MyPlayer>().SetParentAcrossNetwork(Slot2.GetComponent<PhotonView>().ViewID);
            g.GetComponent<MyPlayer>().AssignPosition(200, 76, 0);
        }
    }
    
    public void OnClickDisplayQuestion()
    {
        ShowQuestionButton.SetActive(false);

        //questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
        //questionIndex = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
            //questionIndex = 6;
            pv.RPC("SetQuestionIndex", RpcTarget.AllBuffered, questionIndex);
            QuestionText.text = questions[questionIndex].Question;
            SummaryQuestionText.text = questions[questionIndex].Question;
            SummaryAnswer.text = questions[questionIndex].Answer.ToString();

            QuestionBox.SetActive(true);
        }
        else
        {
            pv.RPC("AskForQuestionIndex", RpcTarget.MasterClient);
        }

        
    }
    [PunRPC]
    void AskForQuestionIndex()
    {
        pv.RPC("SetQuestionIndex", RpcTarget.AllBuffered, questionIndex);
    }
    [PunRPC]
    public void SetQuestionIndex(int index)
    {
        questionIndex = index;
        pv.RPC("DisplayQuestion", RpcTarget.All);
        pv.RPC("EnableQuestionBox", RpcTarget.All);
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
            photonView.RPC("SendAnswerToAll", RpcTarget.All, AnswerTF.text, PhotonNetwork.LocalPlayer.ActorNumber, betAmount, PhotonNetwork.NickName);
        }
    }
    [PunRPC]
    public void SendAnswerToAll(string answer, int actorNumber, int bet, string name)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player1NameSummaryScreen.text = name;
                Player1AnswerSummaryScreen.text = answer;
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
            else
            {
                Player2NameSummaryScreen.text = name;
                Player2AnswerSummaryScreen.text = answer;
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }

        }
        else
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player2NameSummaryScreen.text = name;
                Player2AnswerSummaryScreen.text = answer;
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
            else
            {
                Player1NameSummaryScreen.text = name;
                Player1AnswerSummaryScreen.text = answer;
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
        }
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
                //photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
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

            //photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
        }

        #region Check if everybody Answered
        if (PhotonNetwork.IsMasterClient)
        {
            bool allAnswered = true;
            foreach (Player p in players)
            {
                if (p.betAmount == 0)
                {
                    allAnswered = false;
                    Debug.Log("Bet: " + p.betAmount + " Not by: " + p.name);
                }
            }
            if (allAnswered)
            {
                WaitingForOtherPlayerImage.SetActive(false);
                CheckForWinnerButton.SetActive(true);
            }
            else
            {
                CheckForWinnerButton.SetActive(false);
                //WaitingForOtherPlayerImage.SetActive(true);
            }
        }
        #endregion
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
        //WaitingForOtherPlayerImage.SetActive(false);
        photonView.RPC("DisplaySummaryScreen", RpcTarget.All);
    }
    [PunRPC]
    public void DisplaySummaryScreen()
    {
        QuestionBox.SetActive(false);
        WaitingForOtherPlayerImage.SetActive(false);
        CheckForWinnerButton.SetActive(false);
        SummaryScreen.SetActive(true);
        photonView.RPC("CalculateResults", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void CalculateResults() //To be called on a button
    {
        photonView.RPC("ShortListWinner", RpcTarget.AllBuffered, null);
    }
    [PunRPC]
    public void ShortListWinner()
    {
        int totalBetAmount = 0;
        //if (!PhotonNetwork.IsMasterClient) return;
        //QuestionText.text = "";
        int diff = Int32.MaxValue;
        int actorNumber = 0;

        //ListOfPlayer.text = "We are here";

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
            if (p.difference == diff && p.actorNumber != actorNumber) //To find any other with same differences
            {
                winner.Add(p);
            }
            totalBetAmount += p.betAmount;
        }
        int prize = totalBetAmount / winner.Count;
        Debug.Log("Prize: " + prize + "Total: " + totalBetAmount + "winner.count: " + winner.Count);
        bool win = false;
        foreach (Player pl in players)
        {
            win = false;
            foreach (Player p in winner)
            {
                if(p.actorNumber == pl.actorNumber)
                {
                    win = true;
                    photonView.RPC("AnnounceWinner", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, prize, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
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
    public void AnnounceWinner(int actorNumber, int answer, int correctAnswer, int prize, int difference, string name) 
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //QuestionText.text = "You  Win" + name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + " WINNER!!!";
            //QuestionBox.SetActive(false);

            //SummaryScreen.SetActive(true);

            WhoWonText.text = "YOU WIN!!!";
            BetAmountWin.text = prize.ToString();
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
            //QuestionText.text = "You  Loose" + name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + " WINNER!!!";
            //QuestionBox.SetActive(false);
            //SummaryScreen.SetActive(true);
            WhoWonText.text = "YOU LOST!!!";
            BetAmountWin.text = "0";
        }
        /*else
        {
            QuestionText.text = name + " with answer " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference + "Wins";
        }*/
    }
    public void ShowResults()
    {
        PvPScreen.SetActive(false);
        SummaryScreen.SetActive(false);
        ResultScreen.SetActive(true);
    }
    public void AddToRoom()
    {
        photonView.RPC("AddToMasterClient", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.NickName);
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

            if (PhotonNetwork.IsMasterClient)
            {
                Name1.text = pv.Owner.NickName;
            }
            else
            {
                Name2.text = PhotonNetwork.NickName;
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
    public void StartNextRound()
    {
        photonView.RPC("NextRound", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void NextRound()
    {
        foreach(Player p in players)
        {
            betAmount = 0;
            p.answer = 0;
            p.difference = 0;
        }

        Bet10.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        Bet25.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        Bet50.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;

        SummaryScreen.SetActive(false);
        Debug.Log("SummaryScreenDisabled: " + SummaryScreen.activeSelf);
        ResultScreen.SetActive(false);
        Debug.Log("ResultScreenDisabled: " + ResultScreen.activeSelf);
        CheckForWinnerButton.SetActive(false);
        WaitingForOtherPlayerImage.SetActive(false);
        Player1Summary.SetActive(false);
        Player2Summary.SetActive(false);
        PvPScreen.SetActive(true);
        QuestionBox.SetActive(true);

        AnswerTF.text = null;

        //round ++;
        //RoundText.text = "ROUND " + round.ToString();

        OnClickDisplayQuestion();
    }
    public void GoToHomePage()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }
}