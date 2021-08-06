using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// This Script is used for couting the kills, Deaths and other things of a player.
/// </summary>

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }

    // what kind of events we are sending
    public enum EventCode : byte
    {
        NewPlayer,
        ListPlayer,
        ChangeStats,
        TimerSync
    }

    public List<PlayerInfo> allPlayer = new List<PlayerInfo>();
    private int index;

    private List<LeaderBoardPlayer> lboardPlayer = new List<LeaderBoardPlayer>();

    //Match winning
    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 300;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding = 5f;

    public float matchLength = 180f;
    public float currentMatchTime;
    private float sendTimer;


    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            newPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetupTimer();
        }
    }

    private void Update()
    {
        // shows the leaderboard when the player presses TAB
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            showLeaderBoard();
        }

        // Timer Counter 
        if(PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;
                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;
                    state = GameState.Ending;
                    listPlayerSend();
                    StateCheck();
                }
                updateTimeDisplay();

                sendTimer -= Time.deltaTime;
                if(sendTimer <= 0)
                {
                    sendTimer += 1f;
                    timerSend();
                }
            }
            if (currentMatchTime <= 0f)
            {
                currentMatchTime = 0f;
                state = GameState.Ending;
                listPlayerSend();
                StateCheck();
            }
        }
        
    }

    //call OnEvent when enable and add the data to list
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    //call OnEvent when disable and add the data to list
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCode theEvent = (EventCode) photonEvent.Code;
            object[] data = (object[]) photonEvent.CustomData;

            Debug.Log("recieved : " + theEvent);
            switch (theEvent)
            {
                case EventCode.NewPlayer:
                    newPlayerReceive(data);
                    break;

                case EventCode.ListPlayer:
                    listPlayerReceive(data);
                    break;

                case EventCode.ChangeStats:
                    updateStatsReceive(data);
                    break;

                case EventCode.TimerSync:
                    timerReceive(data);
                    break;
            }
        }
    }

    public void newPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        //Call the OnEvent Function
        PhotonNetwork.RaiseEvent(
            (byte) EventCode.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }
    public void newPlayerReceive(object[] dataRecieved)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);
        allPlayer.Add(player);

        listPlayerSend();
    }

    public void listPlayerSend()
    {
        object[] package = new object[allPlayer.Count + 1];

        package[0] = state;
        
        for(int i = 0; i < allPlayer.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayer[i].name;
            piece[1] = allPlayer[i].actor;
            piece[2] = allPlayer[i].kills;
            piece[3] = allPlayer[i].deaths;

            package[i+1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCode.ListPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void listPlayerReceive(object[] dataRecieve)
    {
        allPlayer.Clear();

        state = (GameState)dataRecieve[0];

        for (int i = 1; i < dataRecieve.Length; i++)
        {
            object[] piece = (object[])dataRecieve[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]);

            allPlayer.Add(player);
            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }

        StateCheck();
    }

    // This functions is called whenever a player is killed
    public void updateStatsSend(int actorSending, int statsToUpdate, int amountTochange)
    {
        object[] package = new object[] { actorSending, statsToUpdate, amountTochange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCode.ChangeStats ,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void updateStatsReceive(object[] dataRecieve)
    {
        int actor = (int)dataRecieve[0];
        int statType = (int)dataRecieve[1];
        int amount = (int)dataRecieve[2];

        for(int i = 0; i < allPlayer.Count; i++)
        {
            if(allPlayer[i].actor == actor)
            {
                switch(statType)
                {
                    case 0: // Kills
                        allPlayer[i].kills += amount;
                        Debug.Log("Player " + allPlayer[i].name + " :  kills" + allPlayer[i].kills);
                        break;
                    case 1: // Deaths
                        allPlayer[i].deaths += amount;
                        Debug.Log("Player " + allPlayer[i].name + " : deaths" + allPlayer[i].deaths);
                        break;
                }

                if(i == index)
                {
                    updateStatsDisplay();
                }
                if(UIController.instance.LeaderBoard.activeInHierarchy)
                {
                    showLeaderBoard();
                }
                break;
            }
        }

        // Call this winner finder function when their is a change in the score stats;
        ScoreCheck();
    }

    void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in allPlayer)
        {
            if (player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                listPlayerSend();
            }
        }
    }

    //Show End screen after cehking the state
    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        
        state = GameState.Ending;
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIController.instance.endRound.SetActive(true);
        showLeaderBoard();
        StartCoroutine(EndCo());
    }

    IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    void updateStatsDisplay()
    {
        if(allPlayer.Count > index)
        {
            UIController.instance.kills.text = "Kills: " + allPlayer[index].kills;
            UIController.instance.deaths.text = "Deaths: " + allPlayer[index].deaths;
        }
        else
        {
            UIController.instance.kills.text = "Kills: 0";
            UIController.instance.deaths.text = "Deaths: 0";
        }
    }




    // ------------------------ LeaderBoard-----------------------
    void showLeaderBoard()
    {

        if (UIController.instance.LeaderBoard.activeInHierarchy)
        {
            StartCoroutine(disableLeaderboardAfter1Second());
        }
        else
        {
            DispleaderBoard();
        }

    }

    IEnumerator disableLeaderboardAfter1Second()
    {
        yield return new WaitForSeconds(1.0f);
        UIController.instance.LeaderBoard.SetActive(false);
    }

    void DispleaderBoard()
    {
        // First delete all the previous leadeboard
        UIController.instance.LeaderBoard.SetActive(true);
        foreach(LeaderBoardPlayer lp in lboardPlayer)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayer.Clear();

        UIController.instance.lBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayer(allPlayer);
        foreach(PlayerInfo player in sorted)
        {
            // Instantiate leaderBoard gameObject for each player
            LeaderBoardPlayer newPlayerDisplay = Instantiate(UIController.instance.lBoardPlayerDisplay, UIController.instance.lBoardPlayerDisplay.transform.parent);
            newPlayerDisplay.setInfo(player.name, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);
            lboardPlayer.Add(newPlayerDisplay);
        }
    }

    // List the players accordin to the kills for the leaderboard
    private List<PlayerInfo> SortPlayer(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while(sorted.Count < players.Count)
        {
            int highest = -1;

            PlayerInfo selectedPlayer = players[0];
            foreach(PlayerInfo player in players)
            {
                if(!sorted.Contains(player))
                {
                    if(player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }

            sorted.Add(selectedPlayer);
        }

        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }

    public void SetupTimer()
    {
        if(matchLength > 0)
        {
            currentMatchTime = matchLength;
            updateTimeDisplay();
        }
    }

    public void updateTimeDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);
        UIController.instance.remTimer.text = timeToDisplay.Minutes.ToString("00") + " : " + timeToDisplay.Seconds.ToString("00");
    }

    public void timerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };

        PhotonNetwork.RaiseEvent(
            (byte)EventCode.TimerSync ,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void timerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];

        updateTimeDisplay();
    }

}


[System.Serializable]
//  this class stores the player Info 
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
