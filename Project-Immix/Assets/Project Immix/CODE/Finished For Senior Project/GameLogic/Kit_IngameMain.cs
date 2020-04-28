using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using UnityEngine.Events;


namespace ImmixKit
{
    //Pause Menu state enum
    public enum PauseMenuState { teamSelection = -1, main = 0 }

    public enum AfterTeamSelection { PauseMenu, AttemptSpawn, Loadout }

  
    [System.Serializable]
    public class InternalSpawns
    {
        public List<Kit_PlayerSpawn> spawns = new List<Kit_PlayerSpawn>();
    }

    /// <summary>
    /// The Main script of the ingame logic. It's a PunBehavior so it has all the callbacks for that
    /// </summary>
    public class Kit_IngameMain : MonoBehaviourPunCallbacks, IOnEventCallback, IPunObservable
    {
  
        public GameObject ui_root;

    
        public Canvas canvas;

        //The current state of the pause menu
        public PauseMenuState pauseMenuState = PauseMenuState.teamSelection;

      
        #region Game Information
        [Header("Internal Game Information")]
        public Kit_GameInformation gameInformation;

        public GameObject playerPrefab; //The player prefab that we should use
        #endregion

        [Header("Map Settings")]
        /// <summary>
        /// If you are below this position on your y axis, you die. Makes it so you don't fall forever if you escape a maps boundaries
        /// </summary>
        public float mapDeathThreshold = -50f;

        #region Game Mode Variables
        [Header("Game Mode Variables")]
    
        public float timer = 600f;
       
        public int gameModeStage;
        
        private int lastGameModeStage;
        public int currentGameMode; 
     
        public object currentGameModeRuntimeData;

        [HideInInspector]
        public List<InternalSpawns> internalSpawns = new List<InternalSpawns>();
        #endregion

  
        #region Team Selection
        [Header("Team Selection")]
        public GameObject ts_root;
      
        public AfterTeamSelection ts_after;
       
        public Text ts_changeTeamButtonText;
    
        public Text ts_cantJoinTeamText;
      
        public float ts_cantJoinTeamTime = 3f;
        
        private float ts_cantJoinAlpha = 0f;
        #endregion

     
        #region Pause Menu
        [Header("Pause Menu, Use 'B' in the editor to open / close it")]
        public GameObject pm_root; //The root object of the pause menu
        public GameObject pm_main; //The main page of the pause menu
        public GameObject pm_options; //Options page of the pause menu
        /// <summary>
        /// Button for the loadout menu
        /// </summary>
        public GameObject pm_loadoutButton;
        #endregion

   
        #region Scoreboard
        [Header("Scoreboard")]
        public float sb_pingUpdateRate = 1f; //After how many seconds the ping in our Customproperties should be updated
        private float sb_lastPingUpdate; //When our ping was updated for the last time
        #endregion

     
        #region Camera Control
        [Header("Camera Control")]
        public Camera mainCamera; //The main camera to use for the whole game
     
        public Kit_CameraShake cameraShake;
    
        public Transform activeCameraTransform
        {
            get
            {
                if (mainCamera)
                {
                    return mainCamera.transform.parent;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (mainCamera)
                {

                    mainCamera.transform.parent = value;
                    //If the parent is not null, reset position and rotation
                    if (value)
                    {
                        mainCamera.transform.localPosition = Vector3.zero;
                        mainCamera.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
        public Transform spawnCameraPosition; 
        #endregion

        [Header("Modules")]
        [Header("HUD")]
      
        #region HUD
    
        public Kit_PlayerHUDBase hud;
        #endregion

  
        #region Chat
        [Header("Chat")]
        public Kit_ChatBase chat;
        #endregion

        #region Impact Processor
        [Header("Impact Processor")]
        public Kit_ImpactParticleProcessor impactProcessor;
        #endregion

        #region Scoreboard
        [Header("Scoreboard")]
        public Scoreboard.Kit_ScoreboardBase scoreboard;
        #endregion

        #region PointsUI
        [Header("Points UI")]
        public Kit_PointsUIBase pointsUI;
        #endregion

        #region Victory Screen
        [Header("Victory Screen")]
        public Kit_VictoryScreenUI victoryScreenUI;
        #endregion

        #region MapVoting
        [Header("Map Voting")]
        public Kit_MapVotingUIBase mapVotingUI;
        #endregion

        #region Loadout
        [Header("Loadout")]
        public Kit_LoadoutBase loadoutMenu;
        #endregion

        #region Voting
        [Header("Voting")]
        public Kit_VotingUIBase votingMenu;
        [HideInInspector]
        public Kit_VotingBase currentVoting;
        #endregion

        #region Auto Spawn System
        [Header("Auto Spaawn System")]
        public Kit_AutoSpawnSystemBase autoSpawnSystem;
        #endregion

     
        [Header("Plugins")]
        public RectTransform pluginPlayerActiveHudGo;
    
        public RectTransform pluginAlwaysActiveHudGo;
     
        public RectTransform pluginButtonGo;
      
        public GameObject pluginButtonPrefab;
     
        public Transform pluginModuleGo;

        [Header("Instantiateables")]
     
        public GameObject victoryScreen;
        [HideInInspector]
     
        public Kit_VictoryScreen currentVictoryScreen;
      
        public GameObject mapVoting;
        [HideInInspector]
     
        public Kit_MapVotingBehaviour currentMapVoting;
     
        public GameObject playerStartedVoting;

      
        [Header("Bots")]
        public GameObject botManagerPrefab;
        [HideInInspector]
        /// <summary>
        /// If Bots are enabled, this is the bot manager
        /// </summary>
        public Kit_BotManager currentBotManager;
        [HideInInspector]
    
        public Transform[] botNavPoints;

    
     
        #region Internal Variables
        [HideInInspector]
        public int assignedTeamID = 2;
        /// <summary>
        /// Our own player, returns null if we have not spawned
        /// </summary>
        [HideInInspector]
        public Kit_PlayerBehaviour myPlayer;
        [HideInInspector]
        public static bool isPauseMenuOpen; 
        [HideInInspector]
        public Kit_GameModeBase currentGameModeBehaviour;
      
        [HideInInspector]
        public Kit_GameModeHUDBase currentGameModeHUD;
        [HideInInspector]
 

        public bool hasGameModeStarted = false;
        [HideInInspector]
      
        public bool isCameraFovOverridden;

        public List<object> pluginRuntimeData = new List<object>();
    
        public List<Kit_PlayerBehaviour> allActivePlayers = new List<Kit_PlayerBehaviour>();
        #endregion

        #region Unity Calls
        void Awake()
        {
            //Hide HUD initially
            hud.SetVisibility(false);
            //Set pause menu state
            isPauseMenuOpen = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            //Check if we shall replace camera
            if (gameInformation.mainCameraOverride)
            {
                //Instantiate new
                GameObject newCamera = Instantiate(gameInformation.mainCameraOverride, mainCamera.transform, false);
                //Reparent
                newCamera.transform.parent = spawnCameraPosition;
                //Destroy camera
                Destroy(mainCamera.gameObject);
                //Assign new camera
                mainCamera = newCamera.GetComponent<Camera>();
                //Camera Shake
                cameraShake = newCamera.GetComponentInChildren<Kit_CameraShake>();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        void Start()
        {
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPreSetup(this);
            }

            //Set initial states
            ts_root.SetActive(false);
            pm_root.SetActive(false);
            pm_main.SetActive(false);
            pluginOnForceClose.Invoke();
            ui_root.SetActive(true);
            assignedTeamID = 2;

            //Make sure the main camera is child of the spawn camera position
            activeCameraTransform = spawnCameraPosition;

            if (gameInformation)
            {
                //Check if we're connected
                if (PhotonNetwork.InRoom)
                {
    
                    int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                    gameInformation.allGameModes[gameMode].GamemodeSetup(this);
                    currentGameMode = gameMode;
                    currentGameModeBehaviour = gameInformation.allGameModes[gameMode];

                    //Check if we already have enough players to start playing
                    if (currentGameModeBehaviour.AreEnoughPlayersThere(this))
                    {
                        hasGameModeStarted = true;
                    }

                    //If we already have a game mode hud, destroy it
                    if (currentGameModeHUD)
                    {
                        Destroy(currentGameModeHUD.gameObject);
                    }


                    if (loadoutMenu)
                    {
                        loadoutMenu.Initialize();
                        loadoutMenu.ForceClose();
                    }

                    //Setup HUD
                    if (currentGameModeBehaviour.hudPrefab)
                    {
                        GameObject hudPrefab = Instantiate(currentGameModeBehaviour.hudPrefab, hud.transform, false);
                        //Move to the back
                        hudPrefab.transform.SetAsFirstSibling();
                        //Reset scale
                        hudPrefab.transform.localScale = Vector3.one;
                        //Get script
                        currentGameModeHUD = hudPrefab.GetComponent<Kit_GameModeHUDBase>();
                        //Start
                        currentGameModeHUD.HUDInitialize(this);
                    }

                    if (currentGameModeBehaviour.LoadoutMenuSupported())
                    {
                        pm_loadoutButton.SetActive(true);
                    }
                    else
                    {
                        pm_loadoutButton.SetActive(false);
                    }

                    //Set timer
                    int duration = (int)PhotonNetwork.CurrentRoom.CustomProperties["duration"];
                    //Assign global game length
                    Kit_GameSettings.gameLength = duration;

                    
                    //Setup Bots
                    if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["bots"])
                    {
                        //Setup Nav Points
                        Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();

                        if (navPoints.Length == 0) throw new System.Exception("[Bots] No Nav Points have been found for this scene! You need to add some.");
                        List<Transform> tempNavPoints = new List<Transform>();
                        for (int i = 0; i < navPoints.Length; i++)
                        {
                            if (navPoints[i].gameModes.Contains(currentGameModeBehaviour))
                            {
                                if (navPoints[i].navPointGroupID == 0)
                                {
                                    tempNavPoints.Add(navPoints[i].transform);
                                }
                            }
                        }
                        botNavPoints = tempNavPoints.ToArray();

                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (!currentBotManager)
                            {
                                GameObject go = PhotonNetwork.InstantiateSceneObject(botManagerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
                                currentBotManager = go.GetComponent<Kit_BotManager>();
                                if (currentGameModeBehaviour.botManagerToUse)
                                {
                                    currentGameModeBehaviour.botManagerToUse.Inizialize(currentBotManager);
                                }
                            }
                        }
                        else
                        {
                            if (!currentBotManager)
                            {
                                currentBotManager = FindObjectOfType<Kit_BotManager>();
                            }
                        }
                    }

                    //Set initial Custom properties
                    Hashtable myLocalTable = new Hashtable();
                    //Set inital team
                    //2 = No Team
                    myLocalTable.Add("team", 2);
                    //Set inital stats
                    myLocalTable.Add("kills", 0);
                    myLocalTable.Add("deaths", 0);
                    myLocalTable.Add("assists", 0);
                    myLocalTable.Add("ping", PhotonNetwork.GetPing());
                    myLocalTable.Add("vote", -1);
                    //Assign to GameSettings
                    PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);

                    if (!currentMapVoting && !currentVictoryScreen)
                    {
                        //Open Team Selection
                        ts_root.SetActive(true);
                        //Set Pause Menu state
                        pauseMenuState = PauseMenuState.teamSelection;
                    }


                    //Unlock the cursor
                    LockCursor.lockCursor = false;

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].OnSetupDone(this);
                    }
                }
                else
                {
                    //Go back to Main Menu
                    SceneManager.LoadScene(0);
                }
            }
            else
            {
                Debug.LogError("No Game Information assigned. Game will not work.");
            }
        }

        void Update()
        {
            //If we are in a room
            if (PhotonNetwork.InRoom)
            {
                //Host Logic
                if (PhotonNetwork.IsMasterClient && hasGameModeStarted)
                {
                    #region Timer
                    //Decrease timer
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        //Check if the timer has run out
                        if (timer <= 0)
                        {
                            //Call the game mode callback
                            gameInformation.allGameModes[currentGameMode].TimeRunOut(this);
                        }
                    }
                    #endregion
                }

                #region Scoreboard ping update
                //Check if we send a new update
                if (Time.time > sb_lastPingUpdate + sb_pingUpdateRate)
                {
                    //Set last update
                    sb_lastPingUpdate = Time.time;
                    //Update hashtable
                    Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
                    table["ping"] = PhotonNetwork.GetPing();
                    //Update hashtable
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                }
                #endregion

                #region Pause Menu
                //Check if the pause menu is ready to be opened and closed and if nothing is blocking it
                if (pauseMenuState >= 0 && !currentVictoryScreen && !currentMapVoting && (!loadoutMenu || loadoutMenu && !loadoutMenu.isOpen))
                {
                    if (Input.GetKeyDown(KeyCode.Escape) && Application.platform != RuntimePlatform.WebGLPlayer || Input.GetKeyDown(KeyCode.B) && Application.isEditor || Input.GetKeyDown(KeyCode.M) && Application.platform == RuntimePlatform.WebGLPlayer || Application.isMobilePlatform && UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButtonDown("Pause")) //Escape (for non WebGL), B (For the editor), M (For WebGL)
                    {
                        //Change state
                        isPauseMenuOpen = !isPauseMenuOpen;
                        //Set state
                        if (isPauseMenuOpen)
                        {
                            //Enable pause menu
                            pm_root.SetActive(true);
                            //Enable main page
                            pm_main.SetActive(true);
                            //Disable options menu
                            pm_options.SetActive(false);
                            //Unlock cursor
                            LockCursor.lockCursor = false;
                            //Chat callback
                            chat.PauseMenuOpened();
                            //Auto spawn system callack
                            if (autoSpawnSystem)
                            {
                                autoSpawnSystem.Interruption();
                            }
                        }
                        else
                        {
                            //Disable pause menu
                            pm_root.SetActive(false);
                            pluginOnForceClose.Invoke();
                            //Lock cursor
                            LockCursor.lockCursor = true;
                            //Chat callback
                            chat.PauseMenuClosed();
                        }
                    }
                }
                #endregion

                #region HUD Update
                if (currentGameModeHUD)
                {
                    //Relay update
                    currentGameModeHUD.HUDUpdate(this);
                }
                #endregion

                #region Game Mode
                if (PhotonNetwork.IsMasterClient)
                {
                    if (currentGameModeBehaviour)
                    {
                        currentGameModeBehaviour.GameModeUpdate(this);
                    }
                }
                else
                {
                    if (currentGameModeBehaviour)
                    {
                        currentGameModeBehaviour.GameModeUpdateOthers(this);
                    }
                }

                //Check if the game mode stage has changed
                if (lastGameModeStage != gameModeStage)
                {
                    //Call the callback
                    GameModeStageChanged(lastGameModeStage, gameModeStage);
                    //Set value
                    lastGameModeStage = gameModeStage;
                }
                #endregion

                #region Waiting for Players
                //Check if the game mode should begin
                if (!hasGameModeStarted)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //Check if we now have enough players
                        if (currentGameModeBehaviour.AreEnoughPlayersThere(this))
                        {
                            hasGameModeStarted = true;
                            currentGameModeBehaviour.GameModeBeginMiddle(this);
                        }
                    }
                    //Show waiting on the HUD
                    hud.SetWaitingStatus(true);
                }
                else
                {
                    //Hide waiting on the HUD
                    hud.SetWaitingStatus(false);
                }
                #endregion

                #region Cannot Join Team
                if (ts_cantJoinAlpha > 0)
                {
                    //Decrease
                    ts_cantJoinAlpha -= Time.deltaTime;

                    //Set alpha
                    ts_cantJoinTeamText.color = new Color(ts_cantJoinTeamText.color.r, ts_cantJoinTeamText.color.g, ts_cantJoinTeamText.color.b, ts_cantJoinAlpha);

                    //Enable
                    ts_cantJoinTeamText.enabled = true;
                }
                else
                {
                    //Just disable
                    ts_cantJoinTeamText.enabled = false;
                }
                #endregion

                #region Team - Suicide Button
                if (myPlayer)
                {
                    ts_changeTeamButtonText.text = "Suicide";
                }
                else
                {
                    ts_changeTeamButtonText.text = "Change Team";
                }
                #endregion

                #region FOV
                if (!myPlayer)
                {
                    if (!isCameraFovOverridden)
                        mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }
                #endregion

                #region Plugin
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PluginUpdate(this);
                }
                #endregion
            }
        }

        void LateUpdate()
        {
            if (PhotonNetwork.InRoom)
            {
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PluginLateUpdate(this);
                }
            }
        }
        #endregion

        #region Photon Calls
        public override void OnPlayerLeftRoom(Player player)
        {
            //Someone left
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                //We are the master client, clean up.
                Debug.Log("Clean up after player " + player);
                PhotonNetwork.DestroyPlayerObjects(player);
            }

            if (currentBotManager && currentGameModeBehaviour.botManagerToUse && PhotonNetwork.IsMasterClient)
            {
                currentGameModeBehaviour.botManagerToUse.PlayerLeftTeam(currentBotManager);
            }

            //Inform chat
            chat.PlayerLeft(player);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerLeftRoom(this, player);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //Inform chat
            chat.PlayerJoined(newPlayer);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerJoinedRoom(this, newPlayer);
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            //Check if we are the new master client
            if (PhotonNetwork.IsMasterClient || newMasterClient == PhotonNetwork.LocalPlayer)
            {
                Debug.Log("We are the new Master Client");
            }

            //Inform chat
            chat.MasterClientSwitched(newMasterClient);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].MasterClientSwitched(this, newMasterClient);
            }
        }

        bool isShuttingDown = false;

        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (!isShuttingDown)
            {
                Debug.Log("Disconnected!");
                //We have disconnected from Photon, go to Main Menu
                SceneManager.LoadScene(0);
            }
        }

        public override void OnLeftRoom()
        {
            if (!isShuttingDown)
            {
                Debug.Log("Left room!");
                SceneManager.LoadScene(0);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentMapVoting)
                {
                    currentMapVoting.RecalculateVotes();
                }
            }

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPlayerPropertiesChanged(this, target, changedProps);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //Synchronize timer
                stream.SendNext(timer);
                //Synchronize stage
                stream.SendNext(gameModeStage);
                //Synchronize playing stage
                stream.SendNext(hasGameModeStarted);
            }
            else
            {
                //Set timer
                timer = (float)stream.ReceiveNext();
                //Set stage
                gameModeStage = (int)stream.ReceiveNext();
                //Set playing stage
                hasGameModeStarted = (bool)stream.ReceiveNext();
            }
            //Relay to game mode
            if (currentGameModeBehaviour)
            {
                currentGameModeBehaviour.OnPhotonSerializeView(this, stream, info);
            }
            //Relay to loadout
            if (loadoutMenu)
            {
                loadoutMenu.OnPhotonSerializeView(stream, info);
            }
            //Relay to plugins
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Game Logic calls
        /// <summary>
        /// Tries to spawn a player
        /// </summary>
        public void Spawn()
        {
            //We can only spawn if we do not have a player currently
            if (!myPlayer)
            {
                //Check if we can currently spawn
                if (!currentGameModeBehaviour.UsesCustomSpawn())
                {
                    if (gameInformation.allGameModes[currentGameMode].CanSpawn(this, PhotonNetwork.LocalPlayer))
                    {
                        //Get a spawn
                        Transform spawnLocation = gameInformation.allGameModes[currentGameMode].GetSpawn(this, PhotonNetwork.LocalPlayer);
                        if (spawnLocation)
                        {
                            //Create object array for photon use
                            object[] instData = new object[0];
                            if (loadoutMenu)
                            {
                                //Get the current loadout
                                Loadout curLoadout = loadoutMenu.GetCurrentLoadout();
                                int length = 1;
                                Hashtable playerDataTable = new Hashtable();
                                playerDataTable["team"] = assignedTeamID;
                                playerDataTable["bot"] = false;
                                if (assignedTeamID == 0)
                                {
                                    playerDataTable["playerModelID"] = curLoadout.teamOnePlayerModelID;
                                    playerDataTable["playerModelCustomizations"] = curLoadout.teamOnePlayerModelCustomizations;
                                }
                                else
                                {
                                    playerDataTable["playerModelID"] = curLoadout.teamTwoPlayerModelID;
                                    playerDataTable["playerModelCustomizations"] = curLoadout.teamTwoPlayerModelCustomizations;
                                }
                                length++; 
                                length += curLoadout.loadoutWeapons.Length;
                                //Create instData
                                instData = new object[length];
                                instData[0] = playerDataTable;
                                instData[1] = curLoadout.loadoutWeapons.Length;
                                for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                                {
                                    Hashtable weaponTable = new Hashtable();
                                    weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                                    weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                                    weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                                    instData[2 + i] = weaponTable;
                                }
                            }
                            else
                            {
                                throw new System.Exception("No Loadout menu assigned. This is not allowed.");
                            }
                            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
                            //Copy player
                            myPlayer = go.GetComponent<Kit_PlayerBehaviour>();
                            //Take control using the token
                            myPlayer.TakeControl();
                        }
                    }
                }
                else
                {
                    GameObject player = currentGameModeBehaviour.DoCustomSpawn(this);
                    if (player)
                    {
                        //Copy player
                        myPlayer = player.GetComponent<Kit_PlayerBehaviour>();
                        //Take control using the token
                        myPlayer.TakeControl();
                    }
                }
            }
        }

        private void InternalJoinTeam(int teamID)
        {
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            //Update our player's Hashtable
            table["team"] = teamID;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            //Assign local team ID
            assignedTeamID = teamID;
            //Call Event
            Kit_Events.onTeamSwitched.Invoke(teamID);
            //Tell all players that we switched teams
            PhotonNetwork.RaiseEvent(5, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            if (loadoutMenu)
            {
                loadoutMenu.TeamChanged(assignedTeamID);
            }

            if (ts_after == AfterTeamSelection.AttemptSpawn)
            {
                //Proceed in the menu
                ts_root.SetActive(false); //Deactivate team selection
                pm_root.SetActive(false); //Keep the pause menu deactivated
                pm_main.SetActive(true); //Activate main page
                pluginOnForceClose.Invoke();
                pauseMenuState = PauseMenuState.main;
                //Activate scoreboard
                scoreboard.Enable();
                //Try to spawn
                Spawn();
            }
            else if (ts_after == AfterTeamSelection.Loadout)
            {
                //Proceed in the menu
                ts_root.SetActive(false); //Deactivate team selection
                pm_root.SetActive(true); //Activate pause menu root
                //Enable main page
                pm_main.SetActive(true);
                //Disable options menu
                pm_options.SetActive(false);
                pauseMenuState = PauseMenuState.main;
                isPauseMenuOpen = true;
                //Activate scoreboard
                scoreboard.Enable();

                OpenLoadoutMenu();
            }
            else
            {
                //Proceed in the menu
                ts_root.SetActive(false); //Deactivate team selection
                pm_root.SetActive(true); //Activate pause menu root
                //Enable main page
                pm_main.SetActive(true);
                //Disable options menu
                pm_options.SetActive(false);
                pauseMenuState = PauseMenuState.main;
                isPauseMenuOpen = true;
                //Activate scoreboard
                scoreboard.Enable();
            }

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].LocalPlayerChangedTeam(this, teamID);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            object content = photonEvent.CustomData;
            int senderId = photonEvent.Sender;
            //Relay
            currentGameModeBehaviour.OnPhotonEvent(this, eventCode, content, senderId);
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPhotonEvent(this, eventCode, content, senderId);
            }
            //Find sender
            Photon.Realtime.Player sender = Kit_PhotonPlayerExtensions.Find(senderId);  
            //Player was killed
            if (eventCode == 0)
            {
                Hashtable deathInformation = (Hashtable)content;

                bool botShot = (bool)deathInformation[(byte)0];
                int killer = (int)deathInformation[(byte)1];
                bool botKilled = (bool)deathInformation[(byte)2];
                int killed = (int)deathInformation[(byte)3];

                //Update death stat
                if (botKilled)
                {
                    if (PhotonNetwork.IsMasterClient && currentBotManager)
                    {
                        Kit_Bot killedBot = currentBotManager.GetBotWithID(killed);
                        killedBot.deaths++;

                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].BotWasKilled(this, killedBot);
                        }
                    }
                }
                else
                {
                    if (killed == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        Hashtable myTable = PhotonNetwork.LocalPlayer.CustomProperties;
                        int deaths = (int)myTable["deaths"];
                        deaths++;
                        myTable["deaths"] = deaths;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myTable);

                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].LocalPlayerWasKilled(this);
                        }
                    }
                }

                if (botShot)
                {
                    //Check if bot killed himself
                    if (!botKilled || botKilled && killer != killed)
                    {
                        if (PhotonNetwork.IsMasterClient && currentBotManager)
                        {
                            Kit_Bot killerBot = currentBotManager.GetBotWithID(killer);
                            killerBot.kills++;

                            if (PhotonNetwork.IsMasterClient)
                            {
                                //Call on game mode
                                currentGameModeBehaviour.MasterClientBotScoredKill(this, killerBot);
                            }

                            for (int i = 0; i < gameInformation.plugins.Length; i++)
                            {
                                gameInformation.plugins[i].BotScoredKill(this, killerBot, deathInformation);
                            }
                        }
                    }
                }
                else
                {
                    if (killer == PhotonNetwork.LocalPlayer.ActorNumber && (botKilled || killed != PhotonNetwork.LocalPlayer.ActorNumber))
                    {
                        Hashtable myTable = PhotonNetwork.LocalPlayer.CustomProperties;
                        int kills = (int)myTable["kills"];
                        kills++;
                        myTable["kills"] = kills;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myTable);
                        //Display points
                        pointsUI.DisplayPoints(gameInformation.pointsPerKill, PointType.Kill);
                    
                        //Call on game mode
                        currentGameModeBehaviour.LocalPlayerScoredKill(this);

                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].LocalPlayerScoredKill(this, deathInformation);
                        }
                    }
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    //Game Mode callback
                    currentGameModeBehaviour.PlayerDied(this, botShot, killer, botKilled, killed);
                }

            }
            //Request chat message
            else if (eventCode == 1)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get information out of the hashtable
                int type = (int)chatInformation[(byte)0];
                //Message sent from player
                if (type == 0)
                {
                    //Master client only message
                    if (PhotonNetwork.IsMasterClient)
                    {
                        string message = (string)chatInformation[(byte)1];
                        int targets = (int)chatInformation[(byte)2];

                        //Check game mode
                        if (currentGameModeBehaviour.isTeamGameMode && targets == 1)
                        {
                            Hashtable CustomProperties = sender.CustomProperties;
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                Hashtable CustomPropertiesReceiver = PhotonNetwork.PlayerList[i].CustomProperties;
                                if (CustomProperties["team"] != null && CustomPropertiesReceiver["team"] != null && (int)CustomProperties["team"] == (int)CustomPropertiesReceiver["team"])
                                {
                                    Hashtable chatMessage = new Hashtable(3);
                                    chatMessage[(byte)0] = message;
                                    chatMessage[(byte)1] = targets;
                                    chatMessage[(byte)2] = senderId;
                                    //Send it to this player
                                    PhotonNetwork.RaiseEvent(2, chatMessage, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.PlayerList[i].ActorNumber } }, SendOptions.SendReliable);
                                }
                            }
                        }
                        else
                        {
                            //Send the message to everyone
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                Hashtable chatMessage = new Hashtable(3);
                                chatMessage[(byte)0] = message;
                                chatMessage[(byte)1] = 0; 
                                chatMessage[(byte)2] = senderId;
                                //Send it to this player
                                PhotonNetwork.RaiseEvent(2, chatMessage, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.PlayerList[i].ActorNumber } }, SendOptions.SendReliable);
                            }
                        }
                    }
                }
                //Message sent directly from bot
                else if (type == 1)
                {
                    string botSender = (string)chatInformation[(byte)1];
                    int messageType = (int)chatInformation[(byte)2];

                    if (messageType == 0)
                    {
                        chat.BotJoined(botSender);
                    }
                    else if (messageType == 1)
                    {
                        chat.BotLeft(botSender);
                    }
                }
            }
            //Chat message received
            else if (eventCode == 2)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get sender
                Photon.Realtime.Player chatSender = Kit_PhotonPlayerExtensions.Find((int)chatInformation[(byte)2]);
                if (chatSender != null)
                {
            
                    chat.DisplayChatMessage(chatSender, (string)chatInformation[(byte)0], (int)chatInformation[(byte)1]);
                }
            }
            //Master Client asks us to reset ourselves.
            else if (eventCode == 3)
            {
                //Reset Stats
                //Set initial Custom properties
                Hashtable myLocalTable = PhotonNetwork.LocalPlayer.CustomProperties;
                myLocalTable["kills"] = 0;
                myLocalTable["deaths"] = 0;
                myLocalTable["assists"] = 0;
                myLocalTable["ping"] = PhotonNetwork.GetPing();
                myLocalTable["vote"] = -1; //For Map voting menu
                PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);
                //Kill our player and respawn
                if (myPlayer)
                {
                    PhotonNetwork.Destroy(myPlayer.photonView);
                }
                myPlayer = null;
                //Respawn
                Spawn();
            }
            //Start vote
            else if (eventCode == 4)
            {
                if (playerStartedVoting)
                {
                    //Check if vote can be started
                    if (currentGameModeBehaviour.CanStartVote(this))
                    {
                        //Check if there is not vote in progress
                        if (!currentVoting)
                        {
                            //Get data
                            Hashtable voteInformation = (Hashtable)content;
                            int type = (byte)voteInformation[(byte)0];
                            int id = (int)voteInformation[(byte)1];

                            object[] data = new object[3];
                            data[0] = type; //Which type to vote on
                            data[1] = id; //What to vote on
                            data[2] = sender.ActorNumber; //Starter

                            PhotonNetwork.Instantiate(playerStartedVoting.name, transform.position, transform.rotation, 0, data);
                        }
                    }
                }
            }
            //Player joined team
            else if (eventCode == 5)
            {
                if (currentBotManager && currentGameModeBehaviour.botManagerToUse && PhotonNetwork.IsMasterClient)
                {
                    currentGameModeBehaviour.botManagerToUse.PlayerJoinedTeam(currentBotManager);
                }
            }
            //Spawn Scene Object event
            else if (eventCode == 6)
            {
                Hashtable instantiateInformation = (Hashtable)content;
                PhotonNetwork.InstantiateSceneObject((string)instantiateInformation[(byte)0], (Vector3)instantiateInformation[(byte)1], (Quaternion)instantiateInformation[(byte)2], (byte)instantiateInformation[(byte)3], (object[])instantiateInformation[(byte)4]);
            }
            //Hitmarker event
            else if (eventCode == 7)
            {
                hud.DisplayHitmarker();
            }
            
            else if (eventCode == 8)
            {
                //Display points
                pointsUI.DisplayPoints((int)content, PointType.Kill);         
            }
        }

   
        public void EndGame(Kit_Player winner)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[3];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 0;
                data[1] = winner.isBot;
                data[2] = winner.id;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGamePlayerWin.Invoke(winner);
            }
        }

 
        public void EndGame(int winner)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[2];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGameTeamWin.Invoke(winner);
            }
        }

  
        public void EndGame(int winner, int scoreTeamOne, int scoreTeamTwo)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[4];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                data[2] = scoreTeamOne;
                data[3] = scoreTeamTwo;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGameTeamWinWithScore.Invoke(winner, scoreTeamOne, scoreTeamTwo);
            }
        }

    
        public void OpenVotingMenu()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                List<MapGameModeCombo> usedCombos = new List<MapGameModeCombo>();

                //Get combos
                while (usedCombos.Count < mapVotingUI.amountOfAvailableVotes)
                {
                    //Get a new combo
                    usedCombos.Add(Kit_MapVotingBehaviour.GetMapGameModeCombo(gameInformation, usedCombos));
                }

                List<int> networkCombos = new List<int>();

                //Turn into an int list
                for (int i = 0; i < usedCombos.Count; i++)
                {
                    networkCombos.Add(usedCombos[i].gameMode);
                    networkCombos.Add(usedCombos[i].map);
                }

                object[] data = new object[mapVotingUI.amountOfAvailableVotes * 2];
                //Copy all combos
                for (int i = 0; i < networkCombos.Count; i++)
                {
                    data[i] = networkCombos[i];
                }

                PhotonNetwork.InstantiateSceneObject(mapVoting.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

      
        public void DeleteAllPlayers()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.PlayerList[i]);
                }

                if (currentBotManager)
                {
                    for (int i = 0; i < currentBotManager.bots.Count; i++)
                    {
                        if (currentBotManager.IsBotAlive(currentBotManager.bots[i]))
                        {
                            PhotonNetwork.Destroy(currentBotManager.GetAliveBot(currentBotManager.bots[i]).photonView);
                        }
                    }
                    currentBotManager.enabled = false;
                }
            }
        }

     
        public void VictoryScreenOpened()
        {
            //Reset alpha
            ts_cantJoinAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

        public void MapVotingOpened()
        {
            ts_cantJoinAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

    
        public void SwitchMap(int to)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                //Update table
                table["gameMode"] = currentGameMode;
                table["map"] = to;
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentGameModeBehaviour.traditionalMaps[to].sceneName);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentGameModeBehaviour.lobbyMaps[to].sceneName);
                }
            }
        }

   
        public void SwitchGameMode(int to)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //Get active map
                int map = gameInformation.GetCurrentLevel();
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                //Update table
                table["gameMode"] = to;
                table["map"] = map;
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentGameModeBehaviour.traditionalMaps[map].sceneName);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentGameModeBehaviour.lobbyMaps[map].sceneName);
                }
            }
        }
        #endregion

        public void DisplayMessage(string msg)
        {
            //Display message
            ts_cantJoinTeamText.text = msg;
            //Set alpha
            ts_cantJoinAlpha = ts_cantJoinTeamTime;
        }

        #region ButtonCalls
  
        public void JoinTeam(int teamID)
        {
            //We can just do this if we are in a room
            if (PhotonNetwork.InRoom)
            {
                //We only allow to change teams if we have not spawned
                if (!myPlayer)
                {
                    //Clamp the team id to the available teams
                    teamID = Mathf.Clamp(teamID, 0, 1);
                    //Check if we can join this team OR if we are already in that team
                    if (gameInformation.allGameModes[currentGameMode].CanJoinTeam(this, PhotonNetwork.LocalPlayer, teamID) || teamID == assignedTeamID)
                    {
                        //Join the team
                        InternalJoinTeam(teamID);
                        //Hide message
                        ts_cantJoinAlpha = 0f;
                    }
                    else
                    {
                        //Display message
                        DisplayMessage("Could not join team");
                    }
                }
            }
        }

        public void ChangeTeam()
        {
            //We only allow to change teams if we have not spawned
            if (!myPlayer)
            {
                //Go back in the menu
                ts_root.SetActive(true); //Activate team selection
                pm_root.SetActive(false); //Deactivate pause menu root
                pm_main.SetActive(false); //Deactivate main page
                pauseMenuState = PauseMenuState.teamSelection;
            }
            else
            {
                myPlayer.Suicide();
            }
        }

  
        public void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }

  
        public void ResumeButton()
        {
            //Check if we have spawned
            if (myPlayer)
            {
                //We have, just lock cursor
                //Close pause menu
                isPauseMenuOpen = false;
                pm_root.SetActive(false);
                pluginOnForceClose.Invoke();
                //Lock Cursor
                LockCursor.lockCursor = true;
            }
            else
            {
                Spawn();
            }
        }

        public void OpenLoadoutMenu()
        {
            //Check if something is blocking that
            if (!currentVictoryScreen && !currentMapVoting)
            {
                if (loadoutMenu)
                {
                    loadoutMenu.Open();
                }
            }
        }

        public void StartVote()
        {
            if (votingMenu)
            {
                votingMenu.OpenVotingMenu();
            }
        }
        #endregion

        #region Plugin Calls

        public UnityEvent pluginOnForceClose = new UnityEvent();

        public Button InjectButtonIntoPauseMenu(string txt)
        {
            GameObject go = Instantiate(pluginButtonPrefab, pluginButtonGo, false);
            go.transform.SetSiblingIndex(3);
            go.GetComponentInChildren<Text>().text = txt;
            return go.GetComponent<Button>();
        }
        #endregion

        #region Other Calls
  
        public void SetPauseMenuState(bool open, bool canLockCursor = true)
        {
            if (isPauseMenuOpen != open)
            {
                isPauseMenuOpen = open;
                //Set state
                if (isPauseMenuOpen)
                {
                    //Enable pause menu
                    pm_root.SetActive(true);
                    //Enable main page
                    pm_main.SetActive(true);
                    //Disable options menu
                    pm_options.SetActive(false);
                    //Unlock cursor
                    LockCursor.lockCursor = false;
                    //Chat callback
                    chat.PauseMenuOpened();
                    //Auto spawn system callack
                    if (autoSpawnSystem)
                    {
                        autoSpawnSystem.Interruption();
                    }
                }
                else
                {
                    //Disable pause menu
                    pm_root.SetActive(false);
                    pluginOnForceClose.Invoke();
                    if (canLockCursor)
                    {
                        //Lock cursor
                        LockCursor.lockCursor = true;
                        //Chat callback
                        chat.PauseMenuClosed();
                    }
                }
            }
        }


        public UnityEvent pluginOnResetStats;

    
        public void ResetStats()
        {
            //Set initial Custom properties
            Hashtable myLocalTable = new Hashtable();
            //Set inital team
            //2 = No Team
            myLocalTable.Add("team", 2);
            //Set inital stats
            myLocalTable.Add("kills", 0);
            myLocalTable.Add("deaths", 0);
            myLocalTable.Add("assists", 0);
            myLocalTable.Add("ping", PhotonNetwork.GetPing());
            myLocalTable.Add("vote", -1); //For Map voting menu
                                          //Assign to GameSettings
            PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);

            //Call event
            pluginOnResetStats.Invoke();
        }


        void GameModeStageChanged(int from, int to)
        {
            //If we have gone back to 0 we need to call Start again. It can happen when the same map is played twice in a row since Photon does for some reason not sync the scene.
            if (to == 0 && from != 0)
            {
                Start();
            }
        }
        #endregion
    }
}
