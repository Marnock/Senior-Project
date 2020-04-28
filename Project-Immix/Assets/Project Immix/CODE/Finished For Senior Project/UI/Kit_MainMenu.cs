using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace ImmixKit
{
    //Menu State enum
    public enum MenuState { closed = -1, main = 0, host = 2, browse = 3, quit = 4, selectRegion = 5, quickPlay = 6 }

    namespace UI
    {
        /// <summary>
        /// This script is responsible for controlling the Main Menu.It is a PunBehavior so it has all the callbacks
        /// </summary>
        public class Kit_MainMenu : MonoBehaviourPunCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
        {
            #region Menu States
            [Header("Menu State")]
            public MenuState currentMenuState = MenuState.closed;
            #endregion

            #region Game Information
            [Header("Internal Game Information")]
            public Kit_GameInformation gameInformation;
            #endregion

            #region References
            [Header("References")]
            public EventSystem ui_EvntSystem;
            public Canvas ui_Canvas;
            public Camera mainCamera;
            #endregion

            #region Menu Sections
            [Header("Menu Sections")]
            public GameObject navigationBar;
            public GameObject section_main; //Main menu
            public GameObject section_hostGame; //Hosting Menu
            public GameObject section_browseGames; //Server Browser
            public GameObject section_region; //To choose the region that we want to use
            public GameObject section_quit; //Question before quitting
            #endregion

 
            #region Login
            [Header("Login")]
            public Kit_MenuLogin login_System;

            [HideInInspector]
            public bool isLoggedIn;
            #endregion

            #region Host Menu
            [Header("Host Menu")]
       
            public InputField hm_nameField;
    
            public InputField hm_passwordField;
       
            public Text hm_curMapLabel;
        
            public Text hm_curGameModeLabel;
        
            public Text hm_curDurationLabel;
          
            public Text hm_curPlayerLimitLabel;
        
            public Text hm_curPlayersNeededLabel;
        
      
      
            public Text hm_curBotModeLabel;
       
            public Text hm_curOnlineModeLabel;
            private int hm_currentMap;
            private int hm_currentGameMode;
            private int hm_currentDuration;
            private int hm_currentPlayerLimit;
            private int hm_currentPlayerNeeded = 1;
            private int hm_currentBotMode;
            private int hm_currentOnlineMode;
            #endregion

            #region Region Menu
            public RectTransform rm_EntriesGo; 
            public GameObject rm_EntriesPrefab; 
            #endregion

    
            #region Server Browser
            [Header("Server Browser")]
            public RectTransform sb_EntriesGo;
            public GameObject sb_EntriesPrefab; 
            private List<GameObject> sb_ActiveEntries = new List<GameObject>(); //Currently active server browser entries - used for cleanup

            #region Password
         
            private bool sb_isPasswordActive;
        
            public RoomInfo sb_passwordRoom;
       
            public GameObject sb_password;
        
            public InputField sb_passwordInput;
            #endregion
            #endregion

            #region Player State 
            [Header("Player State")]
            public Kit_MenuPlayerStateBase playerState;
            #endregion

 
            #region Error Message
            [Header("Error Message")]
        
            public GameObject em_root;
        
            public Text em_text;
        
            public Button em_button;
            #endregion

           
            //This section includes Debug stuff
            #region Debug
            [Header("Debug")]
            public Text debug_PhotonState;
            #endregion

            //Internal variables
            #region Internal Variables
            private bool reconnectUponDisconnect; 
            private static bool wasLevelingInizialized;

            private Dictionary<string, RoomInfo> cachedRoomList;

            #endregion

            #region Unity Calls
            void Awake()
            {
                cachedRoomList = new Dictionary<string, RoomInfo>();
                //Set default region
                Kit_GameSettings.selectedRegion = PlayerPrefs.GetString("region", gameInformation.defaultRegion);
            }

            void Start()
            {
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].Reset(this);
                }

                PhotonNetwork.SendRate = gameInformation.sendRate;
                PhotonNetwork.SerializationRate = gameInformation.sendRate;

                //Firstly, reset our custom properties
                Hashtable myLocalTable = new Hashtable();
                //Set inital team
                //2 = No Team
                myLocalTable.Add("team", 2);
                //Set inital stats
                myLocalTable.Add("kills", 0);
                myLocalTable.Add("deaths", 0);
                myLocalTable.Add("assists", 0);
                myLocalTable.Add("ping", 0f);
                myLocalTable.Add("vote", -1); //For Map voting menu
                                              //Assign to GameSettings
                PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);

                //Start Login
                if (login_System)
                {
                    //Assign Delgate
                    login_System.OnLoggedIn = LoggedIn;
                    //Begin Login
                    login_System.BeginLogin();
                }
                else
                {
                    Debug.LogError("No Login System assigned");
                }

                //Set initial states
                em_root.SetActive(false);

                //Generate random name for the Host Menu
                hm_nameField.text = "Room(" + Random.Range(1, 1000) + ")";

                //Set Photon Settings
                //PhotonNetwork.AutoJoinLobby = true;

                //The kit uses a custom scene sync script, so you want to make sure that this is set to false
                PhotonNetwork.AutomaticallySyncScene = false;



                //Setup Region Menu
                for (int i = 0; i < gameInformation.allRegions.Length; i++)
                {
                    //Instantiate the rm_EntriesPrefab prefab
                    GameObject go = Instantiate(rm_EntriesPrefab, rm_EntriesGo) as GameObject;
                    //Set it up
                    go.GetComponent<Kit_RegionEntry>().Setup(this, i);
                }

                //Update Information (To make sure that everything is displayed correctly when the user first views the menus)
                UpdateAllDisplays();

                //Unlock cursor
                LockCursor.lockCursor = false;
            }

            void Update()
            {
                //If assigned
                if (debug_PhotonState)
                {
                    //Display Photon Connection  state
                    debug_PhotonState.text = PhotonNetwork.NetworkClientState.ToString();
                }
            }
            #endregion

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

            #region Photon Calls
            public override void OnRoomListUpdate(List<RoomInfo> roomList)
            {
                UpdateCachedRoomList(roomList);

                //Clean Up
                for (int i = 0; i < sb_ActiveEntries.Count; i++)
                {
                    //Destroy
                    Destroy(sb_ActiveEntries[i]);
                }
                //Reset list
                sb_ActiveEntries = new List<GameObject>();

                //Instantiate new List
                foreach (RoomInfo info in cachedRoomList.Values)
                {
                    if (!(bool)info.CustomProperties["lobby"])
                    {
                        //Instantiate entry
                        GameObject go = Instantiate(sb_EntriesPrefab, sb_EntriesGo) as GameObject;
                        //Set it up
                        go.GetComponent<Kit_ServerBrowserEntry>().Setup(this, info);
                        //Add it to our active list so it will get cleaned up next time
                        sb_ActiveEntries.Add(go);
                    }
                }
            }

            public override void OnConnectedToMaster()
            {
                PhotonNetwork.JoinLobby();
            }

            //We just created a room
            public override void OnCreatedRoom()
            {
                //Our room is created and ready
                //Lets load the appropriate map
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                if (!(bool)table["lobby"])
                {
                    //Get the correct map
                    int mapToLoad = (int)table["map"];
                    //Deactivate all input
                    ui_EvntSystem.enabled = false;
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(gameInformation.allGameModes[hm_currentGameMode].traditionalMaps[mapToLoad].sceneName);
                }
            }

            public override void OnCreateRoomFailed(short returnCode, string message)
            {
                //We could not create a room
                DisplayErrorMessage("Could not create room");
            }

            public override void OnJoinRoomFailed(short returnCode, string message)
            {
                DisplayErrorMessage("Could not join room");
            }

            public override void OnJoinedRoom()
            {
                Debug.Log("Joined room!");
            }

            public override void OnDisconnected(DisconnectCause cause)
            {
                //Check if we should try to reconnect
                if (reconnectUponDisconnect)
                {
                    if (PhotonNetwork.PhotonServerSettings.AppSettings.Server == "" && PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion == "")
                    {
                        //Connect
                        PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                        PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
                        PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion);
                    }
                    else
                    {
                        PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                        PhotonNetwork.ConnectUsingSettings();
                    }

                    //Set boolean
                    reconnectUponDisconnect = false;
                }
            }
            #endregion

            #region Main Menu Management
            /// <summary>
            /// Changes the menu state
            /// </summary>
            public void ChangeMenuState(MenuState newState)
            {
                //Disable Everything
                section_main.SetActive(false);
                section_hostGame.SetActive(false);
                section_browseGames.SetActive(false);
                section_region.SetActive(false);
                section_quit.SetActive(false);

                //Activate correct objects
                if (newState == MenuState.main)
                {
                    section_main.SetActive(true);
                }
                else if (newState == MenuState.host)
                {
                    section_hostGame.SetActive(true);
                }
                else if (newState == MenuState.browse)
                {
                    section_browseGames.SetActive(true);
                }
                else if (newState == MenuState.quit)
                {
                    section_quit.SetActive(true);
                }
                else if (newState == MenuState.selectRegion)
                {
                    section_region.SetActive(true);
                }

                //Copy state
                currentMenuState = newState;
            }

            public void ChangeMenuState(int newState)
            {
                ChangeMenuState((MenuState)newState);
            }

            void UpdateAllDisplays()
            {
                #region Host Menu
                //Map
                hm_curMapLabel.text = gameInformation.allGameModes[hm_currentGameMode].traditionalMaps[hm_currentMap].mapName;

                //Game Mode
                hm_curGameModeLabel.text = gameInformation.allGameModes[hm_currentGameMode].gameModeName;

                //Duration
                if (gameInformation.allGameModes[hm_currentGameMode].traditionalDurations[hm_currentDuration] != 60)
                    hm_curDurationLabel.text = (gameInformation.allGameModes[hm_currentGameMode].traditionalDurations[hm_currentDuration] / 60).ToString() + " minutes";
                else
                    hm_curDurationLabel.text = (gameInformation.allGameModes[hm_currentGameMode].traditionalDurations[hm_currentDuration] / 60).ToString() + " minute";

                //Player Limit
                if (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit] != 1)
                    hm_curPlayerLimitLabel.text = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit].ToString() + " players";
                else
                    hm_curPlayerLimitLabel.text = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit].ToString() + " player";

                //Player Limit
                if (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded] != 1)
                    hm_curPlayersNeededLabel.text = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded].ToString() + " players";
                else
                    hm_curPlayersNeededLabel.text = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded].ToString() + " player";
                    
                if (hm_currentOnlineMode == 0)
                {
                    hm_curOnlineModeLabel.text = "Online";
                }
                else
                {
                    hm_curOnlineModeLabel.text = "Offline";
                }

                if (hm_currentBotMode == 0)
                {
                    hm_curBotModeLabel.text = "Disabled";
                }
                else
                {
                    hm_curBotModeLabel.text = "Enabled";
                }
                #endregion
            }

            public void Quit()
            {
                Application.Quit();
            }
            #endregion

            #region Login
            void LoggedIn(string userName)
            {
                //Set Logged in to true
                isLoggedIn = true;
                //Activate Menu
                ChangeMenuState(MenuState.main);
                //Enable Navigation Bar
                navigationBar.SetActive(true);
                //Store username
                Kit_GameSettings.userName = userName;
                //Assign username to Photon
                PhotonNetwork.LocalPlayer.NickName = userName;
             
              
                if (!PhotonNetwork.InRoom)
                {
                    if (PhotonNetwork.PhotonServerSettings.AppSettings.Server == "" && PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion == "")
                    {
                        Debug.Log(Kit_GameSettings.selectedRegion);
                        //Connect to the default region
                        PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                        PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
                        if (PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion))
                        {
                        }
                        else
                        {
                            Debug.LogError("Could not connect to the default region");
                        }
                    }
                    else
                    {
                        //Connect to the default region
                        if (PhotonNetwork.ConnectUsingSettings())
                        {
                            PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                        }
                        else
                        {
                            Debug.LogError("Could not connect to the default region");
                        }
                    }
                }
                if (playerState)
                {
                    playerState.Initialize(this);
                }
            }
            #endregion

            //This section contains all functions for the hosting menu
            #region HostMenu
            /// <summary>
            /// Starts a new Photon Session (Room)
            /// </summary>
            public void StartSession()
            {
                StartCoroutine(StartSessionRoutine());
            }

            public IEnumerator StartSessionRoutine()
            {
                Kit_GameSettings.currentNetworkingMode = KitNetworkingMode.Traditional;
                if (hm_currentOnlineMode == 0)
                {
                    //Check if we are connected to the Photon Server
                    if (PhotonNetwork.IsConnected)
                    {
                        //Check if the user entered a name
                        if (!hm_nameField.text.IsNullOrWhiteSpace())
                        {
                            //Create room options
                            RoomOptions options = new RoomOptions();
                            //Assign settings
                            //Player Limit
                            options.MaxPlayers = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit];
                            //Create a new hashtable
                            options.CustomRoomProperties = new Hashtable();
                            //Lobby or not
                            options.CustomRoomProperties.Add("lobby", false);
                            //Map
                            options.CustomRoomProperties.Add("map", hm_currentMap);
                            //Game Mode
                            options.CustomRoomProperties.Add("gameMode", hm_currentGameMode);
                            //Duration
                            options.CustomRoomProperties.Add("duration", hm_currentDuration);
                            //Bots
                            options.CustomRoomProperties.Add("bots", hm_currentBotMode == 1);
                            //Password
                            options.CustomRoomProperties.Add("password", hm_passwordField.text);
                            //Player needed
                            options.CustomRoomProperties.Add("playerNeeded", hm_currentPlayerNeeded);
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
                            if (PhotonNetwork.CreateRoom(hm_nameField.text, options, null))
                            {
                                //TODO Display loading screen
                            }
                        }
                    }
                    else
                    {
                        if (PhotonNetwork.PhotonServerSettings.AppSettings.Server == "" && PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion == "")
                        {
                            //Connect
                            PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
                            PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion);

                        }
                        else
                        {
                            PhotonNetwork.ConnectUsingSettings();

                        }
                    }
                }
                else
                {
                    //Check if the user entered a name
                    if (!hm_nameField.text.IsNullOrWhiteSpace())
                    {
                        if (PhotonNetwork.IsConnected)
                            PhotonNetwork.Disconnect();
                        while (PhotonNetwork.IsConnected) yield return null;
                        PhotonNetwork.OfflineMode = true;
                        RoomOptions options = new RoomOptions();
                        //Assign settings
                        //Player Limit
                        options.MaxPlayers = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit];
                        //Create a new hashtable
                        options.CustomRoomProperties = new Hashtable();
                        //Lobby or not
                        options.CustomRoomProperties.Add("lobby", false);
                        //Map
                        options.CustomRoomProperties.Add("map", hm_currentMap);
                        //Game Mode
                        options.CustomRoomProperties.Add("gameMode", hm_currentGameMode);
                        //Duration
                        options.CustomRoomProperties.Add("duration", hm_currentDuration);
                        //Bots
                        options.CustomRoomProperties.Add("bots", hm_currentBotMode == 1);
                        //Password
                        options.CustomRoomProperties.Add("password", hm_passwordField.text);
                        //Player needed
                        options.CustomRoomProperties.Add("playerNeeded", hm_currentPlayerNeeded);
                        string[] customLobbyProperties = new string[6];
                        customLobbyProperties[0] = "lobby";
                        customLobbyProperties[1] = "map";
                        customLobbyProperties[2] = "gameMode";
                        customLobbyProperties[3] = "duration";
                        customLobbyProperties[4] = "bots";
                        customLobbyProperties[5] = "password";
                        options.CustomRoomPropertiesForLobby = customLobbyProperties;
                        //Try to create a new room
                        if (PhotonNetwork.CreateRoom(hm_nameField.text, options, null))
                        {
                            //TODO Display loading screen
                        }
                    }
                }
            }

            /// <summary>
            /// Selects the next map
            /// </summary>
            public void NextMap()
            {
                //Increase number
                hm_currentMap++;
                //Check if we have that many
                if (hm_currentMap >= gameInformation.allGameModes[hm_currentGameMode].traditionalMaps.Length) hm_currentMap = 0; //If not, reset

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the previous Map
            /// </summary>
            public void PreviousMap()
            {
                //Decrease number
                hm_currentMap--;
                //Check if we are below zero
                if (hm_currentMap < 0) hm_currentMap = gameInformation.allGameModes[hm_currentGameMode].traditionalMaps.Length - 1; //If so, set to end of the array

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the next Game Mode
            /// </summary>
            public void NextGameMode()
            {
                //Increase number
                hm_currentGameMode++;
                //Check if we have that many
                if (hm_currentGameMode >= gameInformation.allGameModes.Length) hm_currentGameMode = 0; //If not, reset

                //Reset settings
                hm_currentDuration = 0;
                hm_currentPlayerLimit = 0;
                hm_currentPlayerNeeded = 0;
                hm_currentMap = 0;

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the previous Game Mode
            /// </summary>
            public void PreviousGameMode()
            {
                //Decrease number
                hm_currentGameMode--;
                //Check if we are below zero
                if (hm_currentGameMode < 0) hm_currentGameMode = gameInformation.allGameModes.Length - 1; //If so, set to end of the array

                //Reset settings
                hm_currentDuration = 0;
                hm_currentPlayerLimit = 0;
                hm_currentPlayerNeeded = 0;
                hm_currentMap = 0;

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the next Duration
            /// </summary>
            public void NextDuration()
            {
                //Increase number
                hm_currentDuration++;
                //Check if we have that many
                if (hm_currentDuration >= gameInformation.allGameModes[hm_currentGameMode].traditionalDurations.Length) hm_currentDuration = 0; //If not, reset

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the previous Duration
            /// </summary>
            public void PreviousDuration()
            {
                //Decrease number
                hm_currentDuration--;
                //Check if we are below zero
                if (hm_currentDuration < 0) hm_currentDuration = gameInformation.allGameModes[hm_currentGameMode].traditionalDurations.Length - 1; //If so, set to end of the array

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the next Player Limit
            /// </summary>
            public void NextPlayerLimit()
            {
                //Increase number
                hm_currentPlayerLimit++;
                //Check if we have that many
                if (hm_currentPlayerLimit >= gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits.Length) hm_currentPlayerLimit = 0; //If not, reset

                //Check if we have more players needed than max players
                while (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded] > gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit] && hm_currentPlayerNeeded > 0)
                {
                    hm_currentPlayerNeeded--;
                }

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the previous Player Limit
            /// </summary>
            public void PreviousPlayerLimit()
            {
                //Decrease number
                hm_currentPlayerLimit--;
                //Check if we are below zero
                if (hm_currentPlayerLimit < 0) hm_currentPlayerLimit = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits.Length - 1; //If so, set to end of the array

                //Check if we have more players needed than max players
                while (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded] > gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit] && hm_currentPlayerNeeded > 0)
                {
                    hm_currentPlayerNeeded--;
                }

                //Update display
                UpdateAllDisplays();
            }

            /// <summary>
            /// Selects the next Player Needed
            /// </summary>
            public void NextPlayerNeeded()
            {
                //Increase number
                hm_currentPlayerNeeded++;
                //Check if we have that many
                if (hm_currentPlayerNeeded >= gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded.Length) hm_currentPlayerNeeded = 0;                                                                                                                                             
                if (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded] > gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit]) hm_currentPlayerNeeded = 0;

                //Update display
                UpdateAllDisplays();
            }

    
            public void PreviousPlayerNeeded()
            {
                //Decrease number
                hm_currentPlayerNeeded--;
                //Check if we are below zero
                if (hm_currentPlayerNeeded < 0) hm_currentPlayerNeeded = gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded.Length - 1; //If so, set to end of the array

                //Check if we have more players needed than max players
                while (gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerNeeded[hm_currentPlayerNeeded] > gameInformation.allGameModes[hm_currentGameMode].traditionalPlayerLimits[hm_currentPlayerLimit] && hm_currentPlayerNeeded > 0)
                {
                    hm_currentPlayerNeeded--;
                }

                //Update display
                UpdateAllDisplays();
            }


   
            public void NextOnlineMode()
            {
                //Increase number
                hm_currentOnlineMode++;
                //Check if we have that many
                if (hm_currentOnlineMode >= 2) hm_currentOnlineMode = 0; //If not, reset

                //Update display
                UpdateAllDisplays();
            }

      
            public void PreviousOnlineMode()
            {
                //Decrease number
                hm_currentOnlineMode--;
                //Check if we are below zero
                if (hm_currentOnlineMode < 0) hm_currentOnlineMode = 1; //If so, set to end of the array

                //Update display
                UpdateAllDisplays();
            }

     
            public void NextBotMode()
            {
                //Increase number
                hm_currentBotMode++;
                //Check if we have that many
                if (hm_currentBotMode >= 2) hm_currentBotMode = 0; //If not, reset

                //Update display
                UpdateAllDisplays();
            }

        
            public void PreviousBotMode()
            {
                //Decrease number
                hm_currentBotMode--;
                //Check if we are below zero
                if (hm_currentBotMode < 0) hm_currentBotMode = 1; //If so, set to end of the array

                //Update display
                UpdateAllDisplays();
            }
            #endregion

            #region Button Calls
 
            public void ChangeRegion(int id)
            {
                //Set reconnect boolean
                reconnectUponDisconnect = true;
                //Copy Region ID
                Kit_GameSettings.selectedRegion = gameInformation.allRegions[id].token;
                //Disconnect
                PhotonNetwork.Disconnect();
                //Go to Main Menu
                ChangeMenuState(MenuState.main);
                //Save
                PlayerPrefs.SetString("region", gameInformation.allRegions[id].token);
            }

    
            public void JoinRoom(RoomInfo room)
            {
                //Check for password
                string password = (string)room.CustomProperties["password"];
                //Join directly when there is no password
                if (password.Length <= 0)
                {
                    if (PhotonNetwork.JoinRoom(room.Name))
                    {

                    }
                }
                else
                {
                    //Ask for password.
                    //Set room
                    sb_passwordRoom = room;
                    //Reset input
                    sb_passwordInput.text = "";
                    //Open
                    sb_password.SetActive(true);
                }
            }

   
            public void JoinRoom(string room)
            {
                if (PhotonNetwork.JoinRoom(room))
                {

                }
            }
            #endregion
            #region Password
            public void PasswordJoin()
            {
                //Check for password
                string password = (string)sb_passwordRoom.CustomProperties["password"];
                if (password == sb_passwordInput.text)
                {
                    if (PhotonNetwork.JoinRoom(sb_passwordRoom.Name))
                    {

                    }
                }
                //Display error
                else
                {
                    DisplayErrorMessage("Password is wrong.");
                }
            }

            public void PasswordAbort()
            {
                //Close
                sb_password.SetActive(false);
            }
            #endregion

            #region Error Message
            public void DisplayErrorMessage(string content)
            {
                //Set text
                em_text.text = content;
                //Show
                em_root.SetActive(true);
                //Select button
                em_button.Select();
            }
            #endregion
        }
    }
}
