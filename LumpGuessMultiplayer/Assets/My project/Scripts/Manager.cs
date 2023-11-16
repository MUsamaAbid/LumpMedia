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
    public int reward = 0;
    public bool answered = false;
    public int totalBet = 0;
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
    [SerializeField] Text crownTotalPrize;

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
    [SerializeField] Text Player1RewardSummaryScreen;

    [SerializeField] GameObject Player2Summary;
    [SerializeField] Text Player2NameSummaryScreen;
    [SerializeField] Text Player2BetSummaryScreen;
    [SerializeField] Text Player2RewardSummaryScreen;

    [SerializeField] GameObject WaitingForOtherPlayersScreen;

    [SerializeField] Text RoundText;

    [SerializeField] Timer timer;

    int round;

    bool gameEnded;

    bool timerEnded;

    private void Start()
    {
        gameEnded = false;
        timerEnded = false;
        round = 0;
        RoundText.text = "ROUND " + round.ToString();
        //questionIndex = 2;
        betAmount = 0;

        players = new List<Player>();
        winner = new List<Player>();

        SpawnPlayer();

        Player1Summary.SetActive(false);
        Player2Summary.SetActive(false);

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
        round ++;
        if(round >= 3)
        {
            gameEnded = true;
        }
        RoundText.text = "ROUND " + round.ToString();
        Next();
        
        ShowQuestionButton.SetActive(false);

        //questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
        //questionIndex = 0;
        betAmount = 0;
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player p in players)
            {
                p.answered = false;
                //p.betAmount = 0;
                p.answer = 0;
                p.difference = 0;
            }
            //questionIndex = UnityEngine.Random.Range(0, questions.Length - 1);
            questionIndex = round;
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
        EmptyEverything();
        QuestionBox.SetActive(true);
        Debug.Log("Question Box activated");
    }
    public void TimerEnded()
    {
        timerEnded = true;
        OnSubmitAnswerButton();
    }
    public void OnSubmitAnswerButton()
    {
        WaitingForOtherPlayerImage.SetActive(true);
        if (AnswerTF.text == null || AnswerTF.text == "")
        {
            //Soemtihng else
            string str = int.MaxValue.ToString();
            photonView.RPC("SendAnswerToAll", RpcTarget.All, str, PhotonNetwork.LocalPlayer.ActorNumber, betAmount, PhotonNetwork.NickName);
        }
        else
        {
            photonView.RPC("SendAnswerToAll", RpcTarget.All, AnswerTF.text, PhotonNetwork.LocalPlayer.ActorNumber, betAmount, PhotonNetwork.NickName);
        }
    }
    [PunRPC]
    public void SendAnswerToAll(string answer, int actorNumber, int bet, string name)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player1NameSummaryScreen.text = name;
                Player1RewardSummaryScreen.text = answer;
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
            else
            {
                Player2NameSummaryScreen.text = name;
                Player2RewardSummaryScreen.text = answer;
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
        }
        else
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player2NameSummaryScreen.text = name;
                Player2RewardSummaryScreen.text = answer;
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
            else
            {
                Player1NameSummaryScreen.text = name;
                Player1RewardSummaryScreen.text = answer;
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
                //Here you have to check if the answer isn't empty or not
                pl.difference = CheckForDifference(pl.answer, questions[questionIndex].Answer);
                pl.betAmount = bet;
                pl.answered = true;
                pl.totalBet += bet;
                Debug.Log("TTT: " + pl.totalBet);
                ListOfPlayer.text = ListOfPlayer.text + "\n" + "name: " + pl.name + "Answer" + pl.answer;
                //photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
                //FillPlayerStats(answer, actorNumber, bet, name, pl);
                Debug.Log("DDebug:OnSubmitAnswer - " + pl.name + "-Answer(" + pl.answer + ")BetAmount(" + pl.betAmount + ")TotalBet(" + pl.totalBet + ")");
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
            pl.totalBet += bet;
            Debug.Log("TTT: " + pl.totalBet);
            pl.answered = true;
            Debug.Log("DDebug:OnSubmitAnswer - " + pl.name + "-Answer(" + pl.answer + ")BetAmount(" + pl.betAmount + ")TotalBet(" + pl.totalBet + ")");
            players.Add(pl);

            //photonView.RPC("RecieveFromMasterClient", RpcTarget.All, pl.answer, pl.actorNumber, questions[questionIndex].Answer, pl.difference);
        }
        
        #region Check if everybody Answered
        if (PhotonNetwork.IsMasterClient)
        {
            bool allAnswered = true;
            foreach (Player p in players)
            {
                if (!p.answered)
                {
                    allAnswered = false;
                }
            }
            if (allAnswered || timerEnded)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    ShortListWinner(); //Now test Also recheck the result screen
                }
                if (gameEnded)
                {
                    WaitingForOtherPlayerImage.SetActive(false);
                    //CheckForWinnerButton.SetActive(true);
                    CheckForTheWinner();
                    pv.RPC("ShowResults", RpcTarget.All);
                }
                else
                {
                    CheckForTheWinner();
                    //StartNextRound();
                    StartCoroutine(N());
                    timerEnded = false;
                }
            }
            else
            {
                CheckForWinnerButton.SetActive(false);
                //WaitingForOtherPlayerImage.SetActive(true);
            }
        }
        #endregion
    }
    IEnumerator N()
    {
        yield return new WaitForSeconds(5);
        StartNextRound();
    }
    /*private void FillPlayerStats(string answer, int actorNumber, int bet, string name, Player pl)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player1NameSummaryScreen.text = pl.name;
                Player1RewardSummaryScreen.text = pl.answer.ToString();
                Player1BetSummaryScreen.text = pl.totalBet.ToString();
                Player1Summary.SetActive(true);
            }
            else
            {
                Player2NameSummaryScreen.text = pl.name;
                Player2RewardSummaryScreen.text = pl.answer.ToString();
                Player2BetSummaryScreen.text = pl.totalBet.ToString();
                Player2Summary.SetActive(true);
            }

        }
        else
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player2NameSummaryScreen.text = name;
                Player2RewardSummaryScreen.text = answer;
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
            else
            {
                Player1NameSummaryScreen.text = name;
                Player1RewardSummaryScreen.text = answer;
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
        }
    }*/

    /*[PunRPC]
    public void RecieveFromMasterClient(int answer, int actorNumber, int correctAnswer, int difference)
    {
        if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            QuestionText.text = actorNumber + " Master client recived your answer: " + answer + "- Correct: " + correctAnswer + "- Difference: " + difference;
        }
    }*/
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
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("CalculateResults", RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    public void CalculateResults() //To be called on a button
    {
        //photonView.RPC("ShortListWinner", RpcTarget.MasterClient, null);
        //ShortListWinner();
        ResultsBaseOnPrize();

    }
    //[PunRPC]
    public void ShortListWinner()
    {
        //No need to add bet amount here
        Debug.Log("ShortLIST WINNER CALLED");

        int totalBetAmount = 0;
        int diff = Int32.MaxValue;
        int actorNumber = 0;

        winner = null;
        winner = new List<Player>();

        foreach (Player p in players)
        {
            if (p.difference < diff)
            {
                actorNumber = p.actorNumber;
                diff = p.difference;
            }
        }

        foreach (Player p in players)
        {
            Debug.Log("PPP: " + p.actorNumber);
            if (p.actorNumber == actorNumber) //To Add the one that was found the smallest in the previous loop
            {
                winner.Add(p);
                Debug.Log("WWW: First winner: " + p.actorNumber);
            }
            if (p.difference == diff && p.actorNumber != actorNumber) //To find any other with same differences
            {
                winner.Add(p);
                Debug.Log("WWW: Second winner: " + p.actorNumber);
            }
            totalBetAmount += p.betAmount;
           // Debug.Log("Debug:OnSubmitAnswer - " + pl.name + "-Answer(" + pl.answer + ")BetAmount(" + pl.betAmount + ")TotalBet(" + pl.totalBet + ")");
            Debug.Log("DDebug:OnSubmitAnswer - TotalBetAmount(" + totalBetAmount + ")");
        }
        int prize = totalBetAmount / winner.Count;
        Debug.Log("DDebug:OnSubmitAnswer - Prize(" + prize + ")");
        Debug.Log("Prize: " + prize + "Total: " + totalBetAmount + "winner.count: " + winner.Count);
        bool win = false;
        foreach (Player pl in players)
        {
            win = false;
            foreach (Player p in winner)
            {
                if(p.actorNumber == pl.actorNumber)
                {
                    p.reward += prize; //prize adding
                    Debug.Log("Reward added: Bet - " + p.totalBet);
                    Debug.Log("Reward added: Reward - " + p.reward);
                    Debug.Log("Reward added: Prize - " + prize);
                    Debug.Log("Reward added: name - " + p.name);
                    win = true;
                    //photonView.RPC("AnnounceWinner", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, prize, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
                    Debug.Log("MMaster: Winner is: " + p.name);
                    Debug.Log("TTT-P: " + p.reward); //Check this
                    Debug.Log("DDebug:OnSubmitAnswer - PrizeGivenTo(" + p.name + ")" + "PrizeAmount(" + prize + ")TotalPrize(" + p.reward + ")" + "TotalBet(" + p.totalBet +")");
                    pv.RPC("DisplayOnSummaryScreen", RpcTarget.All, p.actorNumber, p.betAmount, prize, p.name);
                }
            }
            if (!win)
            {
                //photonView.RPC("AnnounceLoser", RpcTarget.All, pl.actorNumber, pl.answer, questions[questionIndex].Answer, CheckForDifference(pl.answer, questions[questionIndex].Answer), pl.name);
                Debug.Log("MMaster: Loose is: " + pl.name);
                Debug.Log("DDebug:OnSubmitAnswer - Lost(" + pl.name + ")" + "PrizeAmount(" + prize + ")TotalPrize(" + pl.reward + ")" + "TotalBet(" + pl.totalBet + ")");
                pv.RPC("DisplayOnSummaryScreen", RpcTarget.All, pl.actorNumber, pl.betAmount, 0, pl.name);
            }
        }
    }
    [PunRPC]
    void DisplayOnSummaryScreen(int actorNumber,int bet, int prize, string n)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player1NameSummaryScreen.text = n;
                Player1RewardSummaryScreen.text = prize.ToString();
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
            else
            {
                Player2NameSummaryScreen.text = n;
                Player2RewardSummaryScreen.text = prize.ToString();
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
        }
        else
        {
            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Player2NameSummaryScreen.text = n;
                Player2RewardSummaryScreen.text = prize.ToString();
                Player2BetSummaryScreen.text = bet.ToString();
                Player2Summary.SetActive(true);
            }
            else
            {
                Player1NameSummaryScreen.text = n;
                Player1RewardSummaryScreen.text = prize.ToString();
                Player1BetSummaryScreen.text = bet.ToString();
                Player1Summary.SetActive(true);
            }
        }
    }
    void ResultsBaseOnPrize()
    {
        List<Player> wins = new List<Player>();
        int actorNumber = 0;
        int reward = 0;
        foreach (Player p in players)
        {
            if(p.reward > reward)
            {
                actorNumber = p.actorNumber;
                reward = p.reward;
            }
        }
        foreach(Player p in players)
        {
            if(p.actorNumber == actorNumber)
            {
                wins.Add(p);
            }
            else if(p.reward == reward && p.actorNumber != actorNumber)
            {
                wins.Add(p);
            }
        }
        bool win;
        foreach(Player p in players)
        {
            win = false;
            foreach(Player pl in wins)
            {
                if(pl.actorNumber == p.actorNumber)
                {
                    win = true;
                    if(p.reward == 0)
                    {
                        photonView.RPC("AnnounceLoser", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, p.reward, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
                    }
                    else
                    {
                        photonView.RPC("AnnounceWinner", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, p.reward, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
                    }
                }
            }
            if (!win)
            {
                photonView.RPC("AnnounceLoser", RpcTarget.All, p.actorNumber, p.answer, questions[questionIndex].Answer, p.reward, CheckForDifference(p.answer, questions[questionIndex].Answer), p.name);
            }
            Debug.Log("PPPlayer: Name: " + p.name);
            Debug.Log("PPPlayer: totalBet: " + p.totalBet);
            Debug.Log("PPPlayer: reward: " + p.reward);
        }
        //pv.RPC("ShowResults", RpcTarget.All);
    }
    [PunRPC]
    public void AnnounceWinner(int actorNumber, int answer, int correctAnswer, int reward, int difference, string name) 
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            WhoWonText.text = "YOU WIN!!!";
            crownTotalPrize.text = reward.ToString();
            foreach(Player p in players)
            {
                if(actorNumber == p.actorNumber)
                {
                    Debug.Log("DDebug: Win(" + p.name + ")Prize(" + p.reward + ")");
                }
            }
        }
    }
    [PunRPC]
    public void AnnounceLoser(int actorNumber, int answer, int correctAnswer, int reward, int difference, string name)
    {
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            WhoWonText.text = "YOU LOST!!!";
            crownTotalPrize.text = reward.ToString();
            foreach (Player p in players)
            {
                if (actorNumber == p.actorNumber)
                {
                    Debug.Log("DDebug: Lost(" + p.name + ")Prize(" + p.reward + ")");
                }
            }
        }
    }
    [PunRPC]
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
        bool exist = false;
        foreach(Player pl in players)
        {
            if(pl.actorNumber == actorNumber)
            {
                exist = true;
            }
        }
        if (!exist)
        {
            Player p = new Player();
            p.actorNumber = actorNumber;
            p.name = name;
            players.Add(p);
            ListOfPlayer.text = "";
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Name1.text = pv.Owner.NickName;
        }
        else
        {
            Name2.text = PhotonNetwork.NickName;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (players.Count > 1)
            {
                WaitingForOtherPlayersScreen.SetActive(false);
                pv.RPC("StartGame", RpcTarget.All);
                //StartGame();
            }
            else
            {
                WaitingForOtherPlayersScreen.SetActive(true);
            }
        }
    }
    [PunRPC]
    void StartGame()
    {
        timer.RestartTimer();
    }
    public void OnClickSetBet()
    {
        photonView.RPC("SetBet", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, int.Parse(BetAmount.text));
    }
    [PunRPC]
    public void EndGame()
    {
        gameEnded = true;
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
        NextRound();
        
        pv.RPC("NextRound", RpcTarget.Others);
    }
    [PunRPC]
    public void NextRound()
    {
        Next();

        OnClickDisplayQuestion();
        timer.RestartTimer();
    }
    void Next()
    {
        EmptyEverything();
        //QuestionBox.SetActive(true);
        Debug.Log("Here");

        AnswerTF.text = null;
        Debug.Log("Here");

        //round++;
        RoundText.text = "ROUND " + round.ToString();

        Debug.Log("Here");

    }

    private void EmptyEverything()
    {
        Debug.Log("Here");
        Bet10.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        Bet25.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        Bet50.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        Debug.Log("Here");

        SummaryScreen.SetActive(false);
        ResultScreen.SetActive(false);
        CheckForWinnerButton.SetActive(false);
        WaitingForOtherPlayerImage.SetActive(false);
        Player1Summary.SetActive(false);
        Player2Summary.SetActive(false);
        PvPScreen.SetActive(true);
    }

    public void GoToHomePage()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }
}