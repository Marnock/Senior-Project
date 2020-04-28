using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;

namespace ImmixKit
{
    namespace UI
    {
        public class Kit_LobbyManager : MonoBehaviourPunCallbacks, IPunObservable
        {
          
            public Kit_GameInformation game;
                 public Kit_MainMenu mainMenu;

       
            [Header("Settings")]
            public float timeUntilGameStartsOnceEnoughPlayersAreFound = 15f;
         
            public double voteTimeGiven = 30f;

      
            [Header("Sounds")]
            public AudioClip playerJoinedSound;
    
            public AudioClip playerLeftSound;
    
            public AudioClip countdownSound;
              private int lastCountdownInt = 10;

    
            [Header("UI")]
            public GameObject disableOnLobbyEnter;
      
            public GameObject enableOnLobbyEnter;
     
            public Text currentLobbyState;
        
            public float currentLobbyStateUpdateTime;
      
            private float currentLobbyStateLastUpdate;

    
            [Header("Game Mode Selection")]
            public GameObject gameModeSelectionPrefab;
          
            public RectTransform gameModeSelectionGo;
    
            public List<Kit_LobbyButton> gameModeSelectionEntries = new List<Kit_LobbyButton>();

            [Header("Player Entries")]
            public GameObject playerEntryPrefab;
    
            public RectTransform playerEntryGo;
       
            public List<Kit_LobbyPlayerEntry> playerEntries = new List<Kit_LobbyPlayerEntry>();

         
            [Header("Map Preview/Vote")]
            public GameObject mapVotePrefab;
       
            public RectTransform mapVoteGo;
        
            public List<Kit_LobbyButton> mapVoteEntries = new List<Kit_LobbyButton>();

            #region Runtime
            private Dictionary<string, RoomInfo> cachedRoomList;
         
            private bool countdownUntilGameIsEnteredHasBegun;
        
            private float countdownUntilGameIsEnteredTimeLeft;
            private double lobbyCreatedAtNetworkTime;
       
            private double voteOverAtTime;
     
            private bool countdownIsBotOverride;
     
            public int[] mapVotes;
            #endregion

            #region Unity Calls
            void Awake()
            {
                cachedRoomList = new Dictionary<string, RoomInfo>();
                //Redraw Once
                RedrawGameModeSelection();
            }

            void Start()
            {
                if (PhotonNetwork.InRoom)
                {
                    //Create new custom properties for ourselves
                    Hashtable table = new Hashtable();
                    table.Add("mapVote", -1);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);

                    LobbyEntered();

                    //Set up map votes
                    if (PhotonNetwork.IsMasterClient)
                    {
                        voteOverAtTime = PhotonNetwork.Time + voteTimeGiven;

                        List<int> mapsToVoteFor = new List<int>();
                        int gm = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                        int iterations = 0; //Just to be sure that we dont hang up
                        while (mapsToVoteFor.Count < Mathf.Clamp(game.allGameModes[gm].lobbyAmountOfMapsToVoteFor, 0, game.allGameModes[gm].lobbyMaps.Length) && iterations < 100)
                        {
                            iterations++;
                            int mapToAdd = Random.Range(0, game.allGameModes[gm].lobbyMaps.Length);
                            if (!mapsToVoteFor.Contains(mapToAdd))
                            {
                                mapsToVoteFor.Add(mapToAdd);
                            }
                        }

                        Debug.Log(mapsToVoteFor.Count);

                        mapVotes = mapsToVoteFor.ToArray();
                    }
                }
                else
                {
                    LobbyLeft();
                }
            }

            void Update()
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    if (PhotonNetwork.InRoom)
                    {
                        if (countdownUntilGameIsEnteredTimeLeft < 10f && countdownUntilGameIsEnteredHasBegun)
                        {
                            if (Mathf.FloorToInt(countdownUntilGameIsEnteredTimeLeft) != lastCountdownInt)
                            {
                                lastCountdownInt = Mathf.FloorToInt(countdownUntilGameIsEnteredTimeLeft);
                                if (countdownSound)
                                {
                                    AudioSource.PlayClipAtPoint(countdownSound, Camera.main.transform.position, 1f);
                                }
                            }
                            currentLobbyState.text = (Mathf.Clamp(countdownUntilGameIsEnteredTimeLeft, 0, 10f)).ToString("F1") + " seconds until game is entered";
                        }
                        else if (Time.time > currentLobbyStateLastUpdate)
                        {
                            if (PhotonNetwork.Time > voteOverAtTime)
                            {
                                if (PhotonNetwork.CurrentRoom.PlayerCount >= game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded)
                                {
                                    if (!countdownUntilGameIsEnteredHasBegun)
                                    {
                                        currentLobbyState.text = "Enough players found...";
                                        if (PhotonNetwork.IsMasterClient)
                                        {
                                            countdownUntilGameIsEnteredTimeLeft = timeUntilGameStartsOnceEnoughPlayersAreFound;
                                            countdownUntilGameIsEnteredHasBegun = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyStartWithBotsAfterSeconds > 0)
                                    {
                                        if (PhotonNetwork.Time > (lobbyCreatedAtNetworkTime + (double)game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyStartWithBotsAfterSeconds))
                                        {
                                            if (!countdownUntilGameIsEnteredHasBegun)
                                            {
                                                currentLobbyState.text = "Starting game with bots!";
                                                if (PhotonNetwork.IsMasterClient)
                                                {
                                                    countdownUntilGameIsEnteredTimeLeft = timeUntilGameStartsOnceEnoughPlayersAreFound;
                                                    countdownUntilGameIsEnteredHasBegun = true;
                                                    countdownIsBotOverride = true;
                                                    Hashtable roomTable = PhotonNetwork.CurrentRoom.CustomProperties;
                                                    roomTable["bots"] = true;
                                                    PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int rdn = Random.Range(0, 4);
                                            if (rdn < 2)
                                            {
                                                if (((game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded) - PhotonNetwork.CurrentRoom.PlayerCount) > 1)
                                                {
                                                    currentLobbyState.text = "We need " + ((game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded) - PhotonNetwork.CurrentRoom.PlayerCount).ToString() + " more players...";
                                                }
                                                else
                                                {
                                                    currentLobbyState.text = "We need one more player...";
                                                }
                                            }
                                            else
                                            {
                                                currentLobbyState.text = "Waiting for more players...";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int rdn = Random.Range(0, 4);
                                        if (rdn < 2)
                                        {
                                            if (((game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded) - PhotonNetwork.CurrentRoom.PlayerCount) > 1)
                                            {
                                                currentLobbyState.text = "We need " + ((game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded) - PhotonNetwork.CurrentRoom.PlayerCount).ToString() + " more players...";
                                            }
                                            else
                                            {
                                                currentLobbyState.text = "We need one more player...";
                                            }
                                        }
                                        else
                                        {
                                            currentLobbyState.text = "Waiting for more players...";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int rdn = Random.Range(0, 5);
                                if (rdn > 2)
                                {
                                    currentLobbyState.text = (voteOverAtTime - PhotonNetwork.Time).ToString("F0") + " more seconds...";
                                }
                                else
                                {
                                    currentLobbyState.text = "Vote in progress";
                                }
                            }
                            currentLobbyStateLastUpdate = Time.time + currentLobbyStateUpdateTime;
                        }

                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (countdownUntilGameIsEnteredHasBegun)
                            {
                                if (countdownUntilGameIsEnteredTimeLeft > 0)
                                {
                                    countdownUntilGameIsEnteredTimeLeft -= Time.deltaTime;

                                    //Check if countdown is over
                                    if (countdownUntilGameIsEnteredTimeLeft <= 0)
                                    {
                                        if (countdownIsBotOverride)
                                        {
                                            Hashtable roomTable = PhotonNetwork.CurrentRoom.CustomProperties;
                                            roomTable["bots"] = true;
                                            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                                        }
                                        else
                                        {
                                            Hashtable roomTable = PhotonNetwork.CurrentRoom.CustomProperties;
                                            int gm = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                                            roomTable["bots"] = game.allGameModes[gm].lobbyBotsEnabled;
                                            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                                        }

                                        Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                                        if (mapVotes.Length > 0)
                                        {
                                            int mapToLoad = mapVotes[GetMapWithMostVotes()];
                                            table["map"] = mapToLoad;
                                            int gameMode = (int)table["gameMode"];
                                            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                                            //Deactivate all input
                                            mainMenu.ui_EvntSystem.enabled = false;
                                            //Load the map
                                            Kit_SceneSyncer.instance.LoadScene(game.allGameModes[gameMode].lobbyMaps[mapToLoad].sceneName);
                                        }
                                        else
                                        {
                                            //Load Map
                                            //Get the correct map
                                            int mapToLoad = (int)table["map"];
                                            int gameMode = (int)table["gameMode"];
                                            //Deactivate all input
                                            mainMenu.ui_EvntSystem.enabled = false;
                                            //Load the map
                                            Kit_SceneSyncer.instance.LoadScene(game.allGameModes[gameMode].lobbyMaps[mapToLoad].sceneName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Custom Photon Calls
            public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(countdownUntilGameIsEnteredHasBegun);
                    stream.SendNext(countdownUntilGameIsEnteredTimeLeft);
                    stream.SendNext(lobbyCreatedAtNetworkTime);
                    stream.SendNext(countdownIsBotOverride);
                    stream.SendNext(voteOverAtTime);
                    stream.SendNext(mapVotes.Length);
                    for (int i = 0; i < mapVotes.Length; i++)
                    {
                        stream.SendNext(mapVotes[i]);
                    }
                }
                else
                {
                    countdownUntilGameIsEnteredHasBegun = (bool)stream.ReceiveNext();
                    countdownUntilGameIsEnteredTimeLeft = (float)stream.ReceiveNext();
                    lobbyCreatedAtNetworkTime = (double)stream.ReceiveNext();
                    countdownIsBotOverride = (bool)stream.ReceiveNext();
                    voteOverAtTime = (double)stream.ReceiveNext();
                    int length = (int)stream.ReceiveNext();
                    if (mapVotes.Length != length)
                    {
                        mapVotes = new int[length];
                    }
                    for (int i = 0; i < length; i++)
                    {
                        int id = (int)stream.ReceiveNext();
                        if (mapVotes[i] != id)
                        {
                            mapVotes[i] = id;
                            RedrawMaps();
                        }
                    }

                }
            }

            private void UpdateCachedRoomList(List<RoomInfo> roomList)
            {
                foreach (RoomInfo info in roomList)
                {
                    // Remove room from cached room list if it got closed, became invisible or was marked as removed
                    if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                    {
                        if (cachedRoomList.ContainsKey(info.Name))
                        {
                            cachedRoomList.Remove(info.Name);
                        }

                        continue;
                    }

                    // Update cached room info
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList[info.Name] = info;
                    }
                    // Add new room info to cache
                    else
                    {
                        cachedRoomList.Add(info.Name, info);
                    }
                }
            }

            public override void OnJoinedRoom()
            {
                Room room = PhotonNetwork.CurrentRoom;
                if (room.CustomProperties["lobby"] != null && (bool)room.CustomProperties["lobby"])
                {
                    //Create new custom properties for ourselves
                    Hashtable table = new Hashtable();
                    table.Add("mapVote", -1);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);

                    LobbyEntered();
                }
            }

            public override void OnCreatedRoom()
            {
                Room room = PhotonNetwork.CurrentRoom;
                if (room.CustomProperties["lobby"] != null && (bool)room.CustomProperties["lobby"])
                {
                    //Set time
                    lobbyCreatedAtNetworkTime = PhotonNetwork.Time;

                    //Create new custom properties for ourselves
                    Hashtable table = new Hashtable();
                    table.Add("mapVote", -1);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                    //Enter Lobby visually
                    LobbyEntered();
                    //Redraw vote
                    RedrawMaps();
                }
            }

            public override void OnLeftRoom()
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    LobbyLeft();
                }
            }
            #endregion

            #region Photon Calls
            public override void OnRoomListUpdate(List<RoomInfo> roomList)
            {
                UpdateCachedRoomList(roomList);
                RedrawGameModeSelection();
            }

            public override void OnPlayerEnteredRoom(Player newPlayer)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    RedrawLobbyMembers();
                    if (playerJoinedSound)
                    {
                        AudioSource.PlayClipAtPoint(playerJoinedSound, Camera.main.transform.position, 1f);
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        //Abort for another ten seconds
                        if (countdownIsBotOverride)
                        {
                            currentLobbyStateLastUpdate = Time.time + currentLobbyStateUpdateTime;
                            countdownUntilGameIsEnteredHasBegun = false;
                            currentLobbyState.text = "Waiting for another ten seconds";
                            countdownIsBotOverride = false;
                            lobbyCreatedAtNetworkTime += 10.0;
                            //Reset bots in Room table
                            Hashtable roomTable = PhotonNetwork.CurrentRoom.CustomProperties;
                            int gm = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                            roomTable["bots"] = game.allGameModes[gm].lobbyBotsEnabled;
                            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                        }
                    }

                    RedrawMaps();
                }
            }

            public override void OnPlayerLeftRoom(Player otherPlayer)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Redraw lobby member list
                    RedrawLobbyMembers();

                    if (playerLeftSound)
                    {
                        //Play leave sound
                        AudioSource.PlayClipAtPoint(playerLeftSound, Camera.main.transform.position, 1f);
                    }

                    if (PhotonNetwork.CurrentRoom.PlayerCount < game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMinimumPlayersNeeded)
                    {
                        if (!countdownIsBotOverride)
                        {
                            currentLobbyStateLastUpdate = Time.time + currentLobbyStateUpdateTime;
                            countdownUntilGameIsEnteredHasBegun = false;
                            currentLobbyState.text = "Not enough players...";
                        }
                    }

                    //He might have had voted, so redraw
                    RedrawMaps();
                }
            }

            public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
            {
                //Vote might have changed, redraw
                RedrawMaps();
            }
            #endregion

            #region Custom Calls
            void LobbyEntered()
            {
                //Set the mode so correct things will be used
                Kit_GameSettings.currentNetworkingMode = KitNetworkingMode.Lobby;

                //Do as the variables say
                disableOnLobbyEnter.SetActive(false);
                enableOnLobbyEnter.SetActive(true);
                //Redraw
                RedrawLobbyMembers();
            }

            void LobbyLeft()
            {
                //Set
                Kit_GameSettings.currentNetworkingMode = KitNetworkingMode.Traditional;

                countdownUntilGameIsEnteredHasBegun = false;
                countdownUntilGameIsEnteredTimeLeft = 0;
                lastCountdownInt = 10;

                disableOnLobbyEnter.SetActive(true);
                enableOnLobbyEnter.SetActive(false);
            }

            public void RedrawLobbyMembers()
            {
                if (PhotonNetwork.InRoom)
                {
                    Room room = PhotonNetwork.CurrentRoom;

                    while (playerEntries.Count < room.MaxPlayers)
                    {
                        //Add new
                        GameObject go = Instantiate(playerEntryPrefab, playerEntryGo, false);
                        //Get
                        Kit_LobbyPlayerEntry entry = go.GetComponent<Kit_LobbyPlayerEntry>();
                        //Add
                        playerEntries.Add(entry);
                    }
                    for (int i = 0; i < room.MaxPlayers; i++)
                    {
                        playerEntries[i].txt.text = "Searching...";
                    }

                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        //Update
                        playerEntries[i].txt.text = PhotonNetwork.PlayerList[i].NickName;
                    }
                }
            }

            void RedrawMaps()
            {
                for (int i = 0; i < mapVoteEntries.Count; i++)
                {
                    Destroy(mapVoteEntries[i].gameObject);
                }
                mapVoteEntries = new List<Kit_LobbyButton>();

                if (PhotonNetwork.InRoom)
                {
                    if (mapVotes.Length > 0)
                    {
                        for (int i = 0; i < mapVotes.Length; i++)
                        {
                            int id = i;
                            GameObject go = Instantiate(mapVotePrefab, mapVoteGo, false);
                            Kit_LobbyButton btn = go.GetComponent<Kit_LobbyButton>();
                            btn.txt.text = GetVotesForMap(i) + "/" + GetTotalVotes();
                            btn.img.sprite = game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMaps[id].mapPicture;
                            btn.btn.onClick.AddListener(delegate { VoteForMap(id); });
                            mapVoteEntries.Add(btn);
                        }
                    }
                    else
                    {
                        GameObject go = Instantiate(mapVotePrefab, mapVoteGo, false);
                        Kit_LobbyButton btn = go.GetComponent<Kit_LobbyButton>();
                        btn.txt.text = "";
                        btn.img.sprite = game.allGameModes[(int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]].lobbyMaps[(int)PhotonNetwork.CurrentRoom.CustomProperties["map"]].mapPicture;
                        mapVoteEntries.Add(btn);
                    }
                }
            }

            public void VoteForMap(int id)
            {
                if (PhotonNetwork.InRoom)
                {
                    Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
                    table["mapVote"] = id;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                    RedrawMaps();
                }
            }

            public void RedrawGameModeSelection()
            {
                int totalPlayersInAllRooms = 0;
                int[] playersInGameModeRoom = new int[game.allGameModes.Length];

                //Add up all players!
                foreach (RoomInfo info in cachedRoomList.Values)
                {
                    if (info.CustomProperties["lobby"] != null && (bool)info.CustomProperties["lobby"])
                    {
                        if (info.CustomProperties["gameMode"] != null)
                        {
                            int gameMode = (int)info.CustomProperties["gameMode"];
                            int players = info.PlayerCount;
                            totalPlayersInAllRooms += players;
                            if (gameMode >= 0 && gameMode < playersInGameModeRoom.Length)
                            {
                                playersInGameModeRoom[gameMode] += players;
                            }
                        }
                    }
                }

                if (gameModeSelectionEntries.Count == 0)
                {
                    for (int i = 0; i < game.allGameModes.Length; i++)
                    {
                        //Setup initially
                        GameObject go = Instantiate(gameModeSelectionPrefab, gameModeSelectionGo, false);
                        //Get
                        Kit_LobbyButton btn = go.GetComponent<Kit_LobbyButton>();
                        int id = i;
                        btn.btn.onClick.AddListener(delegate { SearchGame(id); });
                        //Add
                        gameModeSelectionEntries.Add(btn);
                    }
                }

                //Redraw
                for (int i = 0; i < game.allGameModes.Length; i++)
                {
                    gameModeSelectionEntries[i].txt.text = game.allGameModes[i].gameModeName + " [" + playersInGameModeRoom[i] + "/" + totalPlayersInAllRooms + "]";
                }
            }

            public void SearchGame(int gameMode)
            {
                if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby)
                {
                    foreach (RoomInfo info in cachedRoomList.Values)
                    {
                        if ((int)info.CustomProperties["gameMode"] == gameMode && (bool)info.CustomProperties["lobby"] && info.PlayerCount < info.MaxPlayers)
                        {
                            //Join
                            PhotonNetwork.JoinRoom(info.Name);
                            return;
                        }
                    }

                    //Reset
                    countdownIsBotOverride = false;
                    countdownUntilGameIsEnteredHasBegun = false;
                    countdownUntilGameIsEnteredTimeLeft = 0;
                    currentLobbyStateLastUpdate = 0;
                    voteOverAtTime = 0.0;

                    //None found, create room !
                    //Create room options
                    RoomOptions options = new RoomOptions();
                    //Assign settings
                    //Player Limit
                    options.MaxPlayers = game.allGameModes[gameMode].lobbyMaximumPlayers;
                    //Create a new hashtable
                    options.CustomRoomProperties = new Hashtable();
                    //Lobby or not
                    options.CustomRoomProperties.Add("lobby", true);
                    //Map
                    options.CustomRoomProperties.Add("map", Random.Range(0, game.allGameModes[gameMode].lobbyMaps.Length));
                    //Game Mode
                    options.CustomRoomProperties.Add("gameMode", gameMode);
                    //Duration
                    options.CustomRoomProperties.Add("duration", 0);
                    //Ping limit
                    options.CustomRoomProperties.Add("ping", 0);
                    //AFK limit
                    options.CustomRoomProperties.Add("afk", 0);
                    //Bots
                    options.CustomRoomProperties.Add("bots", game.allGameModes[gameMode].lobbyBotsEnabled);
                    //Password
                    options.CustomRoomProperties.Add("password", "");
                    //Player needed
                    options.CustomRoomProperties.Add("playerNeeded", game.allGameModes[gameMode].lobbyMinimumPlayersNeeded);
                    string[] customLobbyProperties = new string[6];
                    customLobbyProperties[0] = "lobby";
                    customLobbyProperties[1] = "map";
                    customLobbyProperties[2] = "gameMode";
                    customLobbyProperties[3] = "duration";
                    customLobbyProperties[4] = "bots";
                    customLobbyProperties[5] = "password";
                    options.CustomRoomPropertiesForLobby = customLobbyProperties;
                    PhotonNetwork.OfflineMode = false;
                    //Try to create a new room
                    if (PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName + "'s Lobby", options, null))
                    {
                        //It goes on automatically from here
                    }
                }
            }

            public void LeaveLobby()
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                    //Reset
                    countdownIsBotOverride = false;
                    countdownUntilGameIsEnteredHasBegun = false;
                    countdownUntilGameIsEnteredTimeLeft = 0;
                    currentLobbyStateLastUpdate = 0;
                    voteOverAtTime = 0.0;
                }
                #endregion
            }

            public int GetVotesForMap(int id)
            {
                int toReturn = 0;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if this player voted for "id" map
                    if (PhotonNetwork.PlayerList[i].CustomProperties["mapVote"] != null && (int)PhotonNetwork.PlayerList[i].CustomProperties["mapVote"] == id)
                    {
                        toReturn++;
                    }
                }
                return toReturn;
            }

            public int GetTotalVotes()
            {
                int toReturn = 0;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if player has voted
                    if (PhotonNetwork.PlayerList[i].CustomProperties["mapVote"] != null && (int)PhotonNetwork.PlayerList[i].CustomProperties["mapVote"] >= 0)
                    {
                        toReturn++;
                    }
                }
                return toReturn;
            }

            public int GetMapWithMostVotes()
            {
                int mapWithMost = -1;
                int mostVotes = -1;
                for (int i = 0; i < mapVotes.Length; i++)
                {
                    int id = i;
                    if (GetVotesForMap(id) > mostVotes)
                    {
                        mostVotes = GetVotesForMap(id);
                        mapWithMost = id;
                    }
                }

                return mapWithMost;
            }
        }
    }
}