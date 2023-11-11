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
        Player1Summary.SetActive(false);
        Player2Summary.SetActive(false);
        OnClickDisplayQuestion();
    }
    void SpawnPlayer()
    {
        int r = UnityEngine.Random.Range(-8, 8);
        Vector3 pos;
        //if (Slot1.transform.childCount == 0)
        if (PhotonNetwork.IsMasterClient)
            {
            //pos = new Vector3(114.5f, 76, 0);
            pos = new Vector3(0, 0, 0);
            GameObject g = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
            g.GetComponent<MyPlayer>().SetParentAcrossNetwork(Slot1.GetComponent<PhotonView>().ViewID);
            g.GetComponent<MyPlayer>().AssignPosition(-200, 76, 0);
            Debug.Log("MMM: Slot Instantiated");

            /*g.transform.SetParent(Slot1.transform);
            photonView.RPC("SyncParent", RpcTarget.OthersBuffered, Slot1.GetComponent<PhotonView>().ViewID);
            Debug.Log("MMM: Slot1: InstantiatedObjName:" + g.gameObject.name + "\n" + "PhotonViewID: " + Slot1.GetComponent<PhotonView>().ViewID);
            Debug.Log("MMM: SlotCount - " + Slot1.transform.childCount);*/
        }
        else
        {
            //pos = new Vector3(100, 76, 0);
            pos = new Vector3(0, 0, 0);
            GameObject g = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
            g.GetComponent<MyPlayer>().SetParentAcrossNetwork(Slot2.GetComponent<PhotonView>().ViewID);
            g.GetComponent<MyPlayer>().AssignPosition(200, 76, 0);

            /*g.transform.SetParent(Slot2.transform);
            photonView.RPC("SyncParent", RpcTarget.OthersBuffered, Slot2.GetComponent<PhotonView>().ViewID);
            Debug.Log("MMM: Slot2: InstantiatedObjName:" + g.gameObject.name + "\n" + "PhotonViewID: " + Slot2.GetComponent<PhotonView>().ViewID);*/
        }


        /*if (Slot1.transform.childCount > 0)
        {
            g.transform.parent = Slot1.transform;
            Slot1.GetComponent<SyncChild>().SyncParent();
        }
        else
        {
            g.transform.parent = Slot2.transform;
            Slot2.GetComponent<SyncChild>().SyncParent();
        }*/
    }
    /*[PunRPC]
    void SyncParent(int parentViewID)
    {
        // Find the parent object using the ViewID received from the network
        PhotonView parentView = PhotonView.Find(parentViewID);
        Transform parentTransform = parentView.transform;

        // Set the parent on all networked instances
        transform.SetParent(parentTransform);
    }*/
    /*[PunRPC]
    void SetParent(int viewId, int parentViewId)
    {
        // Find the instantiated object and its intended parent
        PhotonView childView = PhotonView.Find(viewId);
        PhotonView parentView = PhotonView.Find(parentViewId);

        // Check if both views are valid
        if (childView != null && parentView != null)
        {
            // Set the parent of the child
            childView.transform.SetParent(parentView.transform);
        }
    }*/
    public void OnClickDisplayQuestion()
    {
        ShowQuestionButton.SetActive(false);
        //CheckForWinnerButton.SetActive(true);

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
    public void GoToHomePage()
    {
        PhotonNetwork.LoadLevel(0);
    }
}