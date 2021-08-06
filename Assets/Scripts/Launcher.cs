using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    private void Awake()
    {
        if(!PlayerPrefs.HasKey("PlayerName"))
        {
            SceneManager.LoadScene(2);
        }
        instance = this;
    }

    //Loading Screen
    public GameObject loadingScreen;
    public Text loaingText;

    // Menu Screen
    public GameObject menuScreen;

    // Create Room Screen
    public GameObject createRoomScreen;
    public InputField roomNameInput;

    //Inroom Screen
    public GameObject InRoomScreen;
    public Text roomName, playerNameTxt;
    private List<Text> allPlayerName = new List<Text>();

    //Error Screens
    public GameObject errorScreen;
    public Text errorTxt;

    //Room Browser Screens
    public GameObject roomBrowserScreen;
    public RoomButtonScript theRoomBtn;
    private List<RoomButtonScript> allRoomBtns = new List<RoomButtonScript>();

    // Extas 
    string levelToPlay = "Map1";
    public Button startBtn;
    public GameObject testBtn;

    

    private void Start()
    {
        closeScreens();
        loadingScreen.SetActive(true);
        loaingText.text = "Connecting you to the servers...";

        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

#if UNITY_EDITOR
        testBtn.SetActive(true);
#endif
    }

    // -------------------------------------------------------------- UI HANDLERS -----------------------------------------------------------

    void closeScreens()
    {
        loadingScreen.SetActive(false);
        menuScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        InRoomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
    }

    public void openCreateRoom()
    {
        closeScreens();
        createRoomScreen.SetActive(true);
    }
    public void exitErrorScreen()
    {
        closeScreens();
        menuScreen.SetActive(true);
    }
    public void openRoomBrowser()
    {
        closeScreens();
        roomBrowserScreen.SetActive(true);
    }
    public void closeRoomBrwoser()
    {
        closeScreens();
        menuScreen.SetActive(true);
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToPlay);
    }




    // ----------------------------------------------------------------- Master and Lobby ----------------------------------------------------------

    public override void OnConnectedToMaster()
    {
        loaingText.text = "Successfuly connected to servers";
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Do this once the player joins the lobby
    public override void OnJoinedLobby()
    {
        loaingText.text = "Lobby Joined";
        closeScreens();
        menuScreen.SetActive(true);

        string name = "Hanuman" + Random.Range(0, 1000).ToString();
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName", name);
    }

    // change the master when the real master leaves the game 
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Only the master can start the game
        if (PhotonNetwork.IsMasterClient)
        {
            startBtn.gameObject.SetActive(true);
        }
        else
        {
            startBtn.gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------------- Create and Join Room -------------------------------------------------------

    // This function is atached to the create room btn;
    public void createRoom()
    {
        if(!string.IsNullOrEmpty(roomNameInput.text))
        {
            // setting the max players that can join a room
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 5;

            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            closeScreens();
            loaingText.text = "Creating the Room...";
            loadingScreen.SetActive(true);
        }
        else if(string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.Log("Room name cannot be empty");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorTxt.text = "Failed to create Room : " + message;
        closeScreens();
        errorScreen.SetActive(true);
    }

    public void leaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        closeScreens();
        loaingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        closeScreens();
        menuScreen.SetActive(true);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        loaingText.text = "Joining Room : " + info.Name;
        closeScreens();
        loadingScreen.SetActive(true);
    }

    // do this once the player joins the room
    public override void OnJoinedRoom()
    {
        closeScreens();
        InRoomScreen.SetActive(true);
        roomName.text = "Joined " + PhotonNetwork.CurrentRoom.Name;

        listAllPlayers();

        // Only the master can start the game
        if(PhotonNetwork.IsMasterClient)
        {
            startBtn.gameObject.SetActive(true);
        }
        else
        {
            startBtn.gameObject.SetActive(false);
        }
    }


    // this functions updates the list the no. of rooms available
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // when a new room is detected first deactivate all the room present in the scroll bar
        foreach (RoomButtonScript rb in allRoomBtns)
        {
            Destroy(rb.gameObject);
        }
        allRoomBtns.Clear();
        theRoomBtn.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            // if the player count in that room is greater than or equal to max(5) and if it is not removed from the list than show the btn
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                // instantiate the butoon and then edit the text of that btn
                RoomButtonScript newBtn = Instantiate(theRoomBtn, theRoomBtn.transform.parent);
                newBtn.SetBtnDetails(roomList[i]);
                newBtn.gameObject.SetActive(true);

                // Add the btn to the list.
                allRoomBtns.Add(newBtn);
            }
        }
    }

    // -------------------------------------------------------- Players In Room ------------------------------------------------------------

    void listAllPlayers()
    {
        foreach(Text player in allPlayerName)
        {
            Destroy(player.gameObject) ;
        }
        allPlayerName.Clear();
        playerNameTxt.gameObject.SetActive(false);

        // Part of Photon Library
        Player[] players = PhotonNetwork.PlayerList;

        for(int i = 0; i < players.Length; i++)
        {
            Text newPlayerName = Instantiate(playerNameTxt, playerNameTxt.transform.parent);
            newPlayerName.text = players[i].NickName;
            newPlayerName.gameObject.SetActive(true);

            allPlayerName.Add(newPlayerName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Text newPlayerName = Instantiate(playerNameTxt, playerNameTxt.transform.parent);
        newPlayerName.text = newPlayer.NickName;
        newPlayerName.gameObject.SetActive(true);

        allPlayerName.Add(newPlayerName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        listAllPlayers();
    }



    // ------------------------------------------------------------------------ OTHERS ------------------------------------------------------------
    public void QuickJoin()
    {
        RoomOptions roomoptions = new RoomOptions();
        roomoptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom("Test", roomoptions);
        closeScreens();
        loaingText.text = "Creating Room ";
        loadingScreen.SetActive(true);
    }
}
