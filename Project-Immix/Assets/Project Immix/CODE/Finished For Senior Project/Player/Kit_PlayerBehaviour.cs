using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace ImmixKit
{
    /// <summary>
    /// All input for the player (e.g. LMB, W,A,S,D, etc) should be stored here, so that bots may use the same scripts.
    /// </summary>
    public class Kit_PlayerInput
    {
        public float hor;
        public float ver;
        public bool crouch;
        public bool sprint;
        public bool jump;
        public bool dropWeapon;
        public bool lmb;
        public bool rmb;
        public bool reload;
        public float mouseX;
        public float mouseY;
        public bool leanLeft;
        public bool leanRight;
        public bool thirdPerson;
        public bool flashlight;
        public bool laser;
        public bool[] weaponSlotUses;
    }

    public class Kit_PlayerBehaviour : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Game Information
        [Header("Internal Game Information")]
        public Kit_GameInformation gameInformation;
        #endregion

        //This section contains everything for the local camera control
        #region Camera Control
        [Header("Camera Control")]
        public Transform playerCameraTransform;

        public Transform playerCameraAnimationTransform;

        public Transform playerCameraFallDownTransform;
  
        public Transform playerCameraHitReactionsTransform;
        #endregion

        #region Movement
        [Header("Movement")]
        public Kit_MovementBase movement; //The system used for movement
        [HideInInspector]
        public object customMovementData;
        public CharacterController cc;

        public AudioSource footstepSource;
  
        public AudioSource movementSoundSource;
        #endregion

  
        #region Looking
        [Header("Mouse Look")]
        public Kit_MouseLookBase looking; //The system used for looking
        public Transform mouseLookObject; //The transform used for looking around
        [HideInInspector]

        public Quaternion recoilApplyRotation;
        [HideInInspector]
        public object customMouseLookData; //Used to store custom mouse look data
        #endregion


        #region Weapons
        [Header("Weapons")]
        public Weapons.Kit_WeaponManagerBase weaponManager; //The system used for weapon management
        public Transform weaponsGo;
   
        public Transform weaponsHitReactions;
        [HideInInspector]
        public object customWeaponManagerData; //Used to store custom weapon manager data

        public LayerMask weaponHitLayers;
        #endregion

        #region Player Vitals
        [Header("Player Vitals")]
        public Kit_VitalsBase vitalsManager;
        [HideInInspector]
        public object customVitalsData;
        #endregion

        #region Player Name UI
        [Header("Player Name UI")]
        public Kit_PlayerNameUIBase nameManager;
        public object customNameData;
        #endregion

        #region Spawn Protection
        [Header("Spawn Protection")]
        public Kit_SpawnProtectionBase spawnProtection;
        public object customSpawnProtectionData;
        #endregion

        #region Bots
        [Header("Bot Controls")]
 
        public Kit_PlayerBotControlBase botControls;
  
        public object botControlsRuntimeData;
        #endregion


        #region Input Manager
        [Header("Input Manager")]
        public Kit_InputManagerBase inputManager;
  
        public object inputManagerData;
        #endregion

        //This section contains internal variables
        #region Internal Variables
        //Team
        public int myTeam = -1;

        [HideInInspector]
        public bool isController = false; 

   
        public object gameModeCustomRuntimeData;

        public Kit_PlayerInput input;

        [HideInInspector]
        public bool isBot;
     
        [HideInInspector]
        public int botId;

        public int id
        {
            get
            {
                if (isBot) return botId;
                else return photonView.OwnerActorNr;
            }
        }
        [HideInInspector]
        public Kit_IngameMain main; 

        //Position and rotation are synced by photon transform view
        private bool syncSetup;

        //We cache this value to avoid to calculate it many times
        [HideInInspector]
        public bool canControlPlayer = true;

        //Third Person Model
        [HideInInspector]
        public Kit_ThirdPersonPlayerModel thirdPersonPlayerModel;
        [HideInInspector]
        public Vector3 ragdollForward;
        [HideInInspector]
        public float ragdollForce;
        [HideInInspector]

        public Vector3 ragdollPoint;
        [HideInInspector]
  
        public int ragdollId;
        [HideInInspector]
         public int deathSoundCategory;
        [HideInInspector]
   
        public int deathSoundID;
        #endregion

        public void TakeControl()
        {
            if (photonView.IsMine)
            {
                //Assign input
                input = new Kit_PlayerInput();
                //Start manager
                inputManager.InitializeControls(this);
                //Start coroutine to take control after player is setup.
                StartCoroutine(TakeControlWait());
            }
        }

        IEnumerator TakeControlWait()
        {
            if (photonView.IsMine)
            {
                while (!thirdPersonPlayerModel) yield return null;

                isController = true;
                //Move camera to the right position
                main.activeCameraTransform = playerCameraTransform;
                //Setup third person model
                thirdPersonPlayerModel.FirstPerson();
                //Setup weapon manager
                weaponManager.SetupManager(this, photonView.InstantiationData);
                //Setup Vitals
                vitalsManager.Setup(this);
        
                //Auto spawn system callack
                if (main.autoSpawnSystem)
                {
                    main.autoSpawnSystem.LocalPlayerSpawned();
                }
                //Tell Game Mode
                main.currentGameModeBehaviour.OnLocalPlayerSpawned(this);
                //Show HUD
                main.hud.SetVisibility(true);
                //Tell looking
                looking.StartLocalPlayer(this);
                //Lock the cursor
                LockCursor.lockCursor = true;
                //Close pause menu
                Kit_IngameMain.isPauseMenuOpen = false;
                main.pm_root.SetActive(false);
                main.pluginOnForceClose.Invoke();
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].LocalPlayerSpawned(this);
                }
            }
        }

        #region Unity Calls
        void Start()
        {
            //0 = Team
            //1 = Primary
            //2 = Secondary
            object[] instObjects = photonView.InstantiationData;
            Hashtable playerData = (Hashtable)instObjects[0];
            //Copy team
            myTeam = (int)playerData["team"];
            //Assign input if this is a bot
            isBot = (bool)playerData["bot"];
            int modelToUse = (int)playerData["playerModelID"];
            int[] playerModelCustomizations = (int[])playerData["playerModelCustomizations"];
            if (isBot)
            {
                //Check for game mode override
                if (main.currentGameModeBehaviour.botControlOverride)
                {
                    botControls = main.currentGameModeBehaviour.botControlOverride;
                }
                input = new Kit_PlayerInput();
                botId = (int)playerData["botid"];
                Kit_BotManager manager = FindObjectOfType<Kit_BotManager>();
                manager.AddActiveBot(this);
                //Initialize bot input
                botControls.InitializeControls(this);
            }
            //Set up player model
            if (myTeam == 0) //Team 1
            {
                //Instantiate one random player model for team 1
                GameObject go = Instantiate(main.gameInformation.allTeamOnePlayerModels[modelToUse].prefab, transform, false);
                //Reset scale
                go.transform.localScale = Vector3.one;
                //Assign
                thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                //Set information
                thirdPersonPlayerModel.information = main.gameInformation.allTeamOnePlayerModels[modelToUse];
                //Setup
                thirdPersonPlayerModel.SetupModel(this);
                //Setup Customization
                thirdPersonPlayerModel.SetCustomizations(playerModelCustomizations, this);
                //Make it third person initially
                thirdPersonPlayerModel.ThirdPerson();
            }
            else //Team 2
            {
                //Instantiate one random player model for team 2
                GameObject go = Instantiate(main.gameInformation.allTeamTwoPlayerModels[modelToUse].prefab, transform, false);
                //Reset scale
                go.transform.localScale = Vector3.one;
                //Assign
                thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                //Set information
                thirdPersonPlayerModel.information = main.gameInformation.allTeamTwoPlayerModels[modelToUse];
                //Setup
                thirdPersonPlayerModel.SetupModel(this);
                //Setup Customization
                thirdPersonPlayerModel.SetCustomizations(playerModelCustomizations, this);
                //Make it third person initially
                thirdPersonPlayerModel.ThirdPerson();
            }

            //Start Spawn Protection
            if (spawnProtection)
            {
                spawnProtection.CustomStart(this);
            }

            if (isBot)
            {
                weaponManager.SetupManagerBot(this, photonView.InstantiationData);

                //Setup Vitals
                vitalsManager.Setup(this);

                //Setup done
                syncSetup = true;

                //Setup marker
                if (nameManager)
                {
                    nameManager.StartRelay(this);
                }
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PlayerSpawned(this);
                }

                //Tell Game Mode
                main.currentGameModeBehaviour.OnPlayerSpawned(this);
            }
            else
            {
                //Setup weapon manager for the others
                if (!photonView.IsMine)
                {
                    weaponManager.SetupManagerOthers(this, photonView.InstantiationData);

                    //Setup done
                    syncSetup = true;

                    if (nameManager)
                    {
                        nameManager.StartRelay(this);
                    }

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerSpawned(this);
                    }

                    //Tell Game Mode
                    main.currentGameModeBehaviour.OnPlayerSpawned(this);
                }
                else
                {
                    main.hud.PlayerStart(this);
                    //Disable our own name hitbox
                    thirdPersonPlayerModel.enemyNameAboveHeadTrigger.enabled = false;
                }
            }
            //Call event system
            Kit_Events.onPlayerSpawned.Invoke(this);

            //Add us to player list
            main.allActivePlayers.Add(this);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            //Find main reference
            main = FindObjectOfType<Kit_IngameMain>();
        }

        bool isShuttingDown = false;

        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        void OnDestroy()
        {
            if (!isShuttingDown)
            {
                //Hide HUD if we were killed
                if (isController && !isBot)
                {
                    main.hud.SetVisibility(false);
                }
                if (!photonView.IsMine || !photonView.IsOwnerActive || isBot)
                {
                    //Release marker
                    if (nameManager)
                    {
                        nameManager.OnDestroyRelay(this);
                    }
                }

                if (!isBot && isController)
                {
                    //Auto spawn system callack
                    if (main.autoSpawnSystem)
                    {
                        main.autoSpawnSystem.LocalPlayerDied();
                    }
                    //Tell Game Mode
                    main.currentGameModeBehaviour.OnLocalPlayerDestroyed(this);
                    //Tell HUD
                    main.hud.PlayerEnd(this);
                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].LocalPlayerDied(this);
                    }
                }
                else
                {
                    //Tell Game Mode
                    main.currentGameModeBehaviour.OnPlayerDestroyed(this);
                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerDied(this);
                    }
                }

                //Make sure the camera never gets destroyed
                if (main.activeCameraTransform == playerCameraTransform && !isBot)
                {
                    main.activeCameraTransform = main.spawnCameraPosition;
                    //Set Fov
                    main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }

                if (PhotonNetwork.InRoom && main.currentGameModeBehaviour.CanControlPlayer(main))
                {
                    if (thirdPersonPlayerModel)
                    {
                        //Unparent sounds
                        thirdPersonPlayerModel.soundFire.transform.parent = null;
                        if (thirdPersonPlayerModel.soundFire.clip)
                        {
                            Destroy(thirdPersonPlayerModel.soundFire.gameObject, thirdPersonPlayerModel.soundFire.clip.length);
                        }
                        else
                        {
                            Destroy(thirdPersonPlayerModel.soundFire.gameObject, 1f);
                        }

                        //Setup ragdoll
                        thirdPersonPlayerModel.CreateRagdoll();
                    }
                }

                //Call event system
                Kit_Events.onPlayerDied.Invoke(this);

                //Remove us from list
                main.allActivePlayers.Remove(this);
            }
        }

        void Update()
        {
            if (photonView)
            {
                //If we are not the owner of the photonView, we need to update position and rotation
                if (!photonView.IsMine)
                {
                    if (syncSetup)
                    {
                        //Weapon manager update for others
                        weaponManager.CustomUpdateOthers(this);
                    }

                    if (isBot)
                    {
                        if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                        }
                    }
                    else
                    {
                        if (main.currentGameModeBehaviour.AreWeEnemies(main, false, photonView.Owner.ActorNumber))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                        }
                    }

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerUpdate(this);
                    }
                }
                else if (isBot)
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }

                if (photonView.IsMine)
                {
                    if (!isBot)
                    {
                        inputManager.WriteToPlayerInput(this);
                    }
                    else
                    {
                        //Get Bot Input
                        botControls.WriteToPlayerInput(this);
                        //Call Plugin
                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].PlayerUpdate(this);
                        }
                    }

                    //Update control value
                    canControlPlayer = main.currentGameModeBehaviour.CanControlPlayer(main);

                    //If we are the controller, update everything
                    if (isController || isBot && PhotonNetwork.IsMasterClient)
                    {
                        movement.CalculateMovementUpdate(this);
                        looking.CalculateLookUpdate(this);
                        weaponManager.CustomUpdate(this);
                        vitalsManager.CustomUpdate(this);
                        //Update spawn protection
                        if (spawnProtection)
                        {
                            spawnProtection.CustomUpdate(this);
                        }

                        //Update hud
                        if (main && main.hud && !isBot)
                        {
                            main.hud.PlayerUpdate(this);
                        }

                        if (!isBot)
                        {
                            //Call Plugin
                            for (int i = 0; i < gameInformation.plugins.Length; i++)
                            {
                                gameInformation.plugins[i].LocalPlayerUpdate(this);
                            }
                        }
                    }
                }
                //Footstep callback
                movement.CalculateFootstepsUpdate(this);
            }
        }

        void LateUpdate()
        {
            //If we are the controller, update everything
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                movement.CalculateMovementLateUpdate(this);
                looking.CalculateLookLateUpdate(this);
            }

            //If we are not the owner of the photonView, we need to update position and rotation
            if (!photonView.IsMine)
            {
                if (isBot)
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
                else
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, false, photonView.Owner.ActorNumber))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
            }
            else if (isBot)
            {
                if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                {
                    if (nameManager)
                    {
                        nameManager.UpdateEnemy(this);
                    }
                }
                else
                {
                    if (nameManager)
                    {
                        nameManager.UpdateFriendly(this);
                    }
                }
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Relay to movement script
            movement.OnControllerColliderHitRelay(this, hit);
            //Relay to mouse look script
            looking.OnControllerColliderHitRelay(this, hit);
            //Relay to weapon manager
            weaponManager.OnControllerColliderHitRelay(this, hit);
        }

        void OnTriggerEnter(Collider col)
        {
            //Relay to movement
            movement.OnTriggerEnterRelay(this, col);
        }

        void OnTriggerExit(Collider col)
        {
            //Relay to movement
            movement.OnTriggerExitRelay(this, col);
        }
        #endregion

        #region Photon Calls
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Sync data for ragdoll
            if (stream.IsWriting)
            {
                stream.SendNext(ragdollForce);
                stream.SendNext(ragdollForward);
                stream.SendNext(ragdollId);
                stream.SendNext(ragdollPoint);
                stream.SendNext(deathSoundCategory);
                stream.SendNext(deathSoundID);
            }
            else
            {
                ragdollForce = (float)stream.ReceiveNext();
                ragdollForward = (Vector3)stream.ReceiveNext();
                ragdollId = (int)stream.ReceiveNext();
                ragdollPoint = (Vector3)stream.ReceiveNext();
                deathSoundCategory = (int)stream.ReceiveNext();
                deathSoundID = (int)stream.ReceiveNext();
            }
            //Movement
            movement.OnPhotonSerializeView(this, stream, info);
            //Mouse Look
            looking.OnPhotonSerializeView(this, stream, info);
            //Spawn Protection
            if (spawnProtection)
            {
                spawnProtection.OnPhotonSerializeView(this, stream, info);
            }
            //Weapon manager
            weaponManager.OnPhotonSerializeView(this, stream, info);
            //Relay
            if (isBot)
            {
                //Bot Controls
                botControls.OnPhotonSerializeView(this, stream, info);
            }
            //Game Mode Relay
            main.currentGameModeBehaviour.PlayerOnPhotonSerializeView(this, stream, info);
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerOnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Custom Calls
        public void LocalDamage(float dmg, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id, bool botShot, int idWhoShot)
        {
            ragdollForce = force;
            ragdollForward = forward;
            ragdollId = id;
            ragdollPoint = hitPos;
            deathSoundCategory = gameInformation.allWeapons[gunID].deathSoundCategory;

            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", RpcTarget.MasterClient, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", photonView.Owner, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
            }
        }

        public void LocalDamage(float dmg, string deathCause, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id, bool botShot, int idWhoShot)
        {
            ragdollForce = force;
            ragdollForward = forward;
            ragdollId = id;
            ragdollPoint = hitPos;
            deathSoundCategory = gameInformation.allWeapons[0].deathSoundCategory;

            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", RpcTarget.MasterClient, dmg, botShot, idWhoShot, deathCause, shotPos, forward, force, hitPos, id);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", photonView.Owner, dmg, botShot, idWhoShot, deathCause, shotPos, forward, force, hitPos, id);
                }
            }
        }

        public void LocalBlind(float time, int gunID, Vector3 shotPos, bool botShot, int idWhoShot)
        {
            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyBlindNetwork", RpcTarget.MasterClient, time, gunID, shotPos, botShot, idWhoShot);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyBlindNetwork", photonView.Owner, time, gunID, shotPos, botShot, idWhoShot);
                }
            }
        }

        public void ApplyFallDamage(float dmg)
        {
            if (isController && photonView)
            {
                vitalsManager.ApplyFallDamage(this, dmg);
            }
        }

        public void Suicide()
        {
            if (isController && photonView)
            {
                vitalsManager.Suicide(this);
            }
        }

        public void Die(int cause)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = 0; //Event 0 = player dead
                                     //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(5);
                    if (isBot)
                    {
                        deathInformation[(byte)0] = true;
                        //Who killed us?
                        deathInformation[(byte)1] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)0] = false;
                        //Who killed us?
                        deathInformation[(byte)1] = photonView.Owner.ActorNumber;
                    }
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    deathInformation[(byte)4] = cause;
                    //With which weapon were we killed?
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(photonView);
                }
            }
        }

        public void Die(bool botShot, int killer, int gunID)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = 0; //Event 0 = player dead
                    //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(5);
                    deathInformation[(byte)0] = botShot;
                    //Who killed us?
                    deathInformation[(byte)1] = killer;
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    //With which weapon were we killed?
                    deathInformation[(byte)4] = gunID;
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        public void Die(bool botShot, int killer, string cause)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = 0; //Event 0 = player dead
                    //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(5);
                    deathInformation[(byte)0] = botShot;
                    //Who killed us?
                    deathInformation[(byte)1] = killer;
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    //With which weapon were we killed?
                    deathInformation[(byte)4] = cause;
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                ragdollForce = force;
                ragdollForward = forward;
                ragdollPoint = hitPos;
                ragdollId = id;
                deathSoundCategory = gameInformation.allWeapons[gunID].deathSoundCategory;
                //Relay to the assigned manager
                vitalsManager.ApplyDamage(this, dmg, botShot, idWhoShot, gunID, shotPos);
                if (!isBot)
                {
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool botShot, int idWhoShot, string deathCause, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                ragdollForce = force;
                ragdollForward = forward;
                ragdollPoint = hitPos;
                ragdollId = id;
                deathSoundCategory = gameInformation.allWeapons[0].deathSoundCategory;

                //Relay to the assigned manager
                vitalsManager.ApplyDamage(this, dmg, botShot, idWhoShot, deathCause, shotPos);
                if (!isBot)
                {
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        [PunRPC]
        public void ApplyBlindNetwork(float time, int gunID, Vector3 shotPos, bool botShot, int idWhoShot)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                if (!isBot)
                {
                    main.hud.DisplayBlind(time);
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        //If we fire using a semi auto weapon, this is called
        [PunRPC]
        public void WeaponSemiFireNetwork()
        {
            //Relay to weapon manager
            weaponManager.NetworkSemiRPCReceived(this);
        }

        //If we fire using a bolt action weapon, this is called
        [PunRPC]
        public void WeaponBoltActionFireNetwork(int state)
        {
            //Relay to weapon manager
            weaponManager.NetworkBoltActionRPCReceived(this, state);
        }

        [PunRPC]
        public void WeaponBurstFireNetwork(int burstLength)
        {
            //Relay to weapon manager
            weaponManager.NetworkBurstRPCReceived(this, burstLength);
        }

        [PunRPC]
        public void WeaponFirePhysicalBulletOthers(Vector3 pos, Vector3 dir)
        {
            //Relay to weapon manager
            weaponManager.NetworkPhysicalBulletFired(this, pos, dir);
        }

        //When we reload, this is called
        [PunRPC]
        public void WeaponReloadNetwork(bool empty)
        {
            //Reload to weapon manager
            weaponManager.NetworkReloadRPCReceived(this, empty);
        }

        //When a procedural reload occurs, this will be called with the correct stage
        [PunRPC]
        public void WeaponProceduralReloadNetwork(int stage)
        {
            //Relay to weapon manager
            weaponManager.NetworkProceduralReloadRPCReceived(this, stage);
        }

        [PunRPC]
        public void WeaponRaycastHit(Vector3 pos, Vector3 normal, int material)
        {
            if (material == -1)
            {
                //Relay to impact processor
                main.impactProcessor.ProcessEnemyImpact(pos, normal);
            }
            else
            {
                //Relay to impact processor
                main.impactProcessor.ProcessImpact(pos, normal, material);
            }
        }

        [PunRPC]
        public void MeleeStabNetwork(int state, int slot)
        {
            //Send to player model
            thirdPersonPlayerModel.PlayMeleeAnimation(0, state);
            //Weapon Manager
            weaponManager.NetworkMeleeStabRPCReceived(this, state, slot);
        }

        [PunRPC]
        public void MeleeChargeNetwork(int id, int slot)
        {
            //Send to player model
            thirdPersonPlayerModel.PlayMeleeAnimation(1, id);
            //Weapon Manager
            weaponManager.NetworkMeleeChargeRPCReceived(this, id, slot);
        }

        [PunRPC]
        public void GrenadePullPinNetwork()
        {
            //Relay
            weaponManager.NetworkGrenadePullPinRPCReceived(this);
        }

        [PunRPC]
        public void GrenadeThrowNetwork()
        {
            //Relay
            weaponManager.NetworkGrenadeThrowRPCReceived(this);
        }

        [PunRPC]
        public void ReplaceWeapon(int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
        {
            //Relay to weapon manager
            weaponManager.NetworkReplaceWeapon(this, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments);
        }


        [PunRPC]
        public void MovementPlaySound(int id, int id2, int arrayID)
        {
            //Relay to movement
            movement.PlaySound(this, id, id2, arrayID);
        }

        [PunRPC]
        public void MovementPlayAnimation(int id, int id2)
        {
            //Relay to movement
            movement.PlayAnimation(this, id, id2);
        }
        #endregion
    }
}
