using UnityEngine;
using System.Collections;
using System;
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ImmixKit
{
  
    public class Kit_Player
    {
    
        public bool isBot;



        /// <summary>
        /// Either photon ID or bot ID
        /// </summary>
        public int id;
    }

    public abstract class Kit_GameModeBase : ScriptableObject
    {
        public string gameModeName;

        [Header("Traditional room browser & host")]
        public Kit_MapInformation[] traditionalMaps;
        public byte[] traditionalPlayerLimits = new byte[3] { 2, 5, 10 };
        public int[] traditionalPlayerNeeded = new int[3] { 0, 2, 4 };
        public int[] traditionalDurations = new int[3] { 5, 10, 20 };
        [Header("Lobby Matchmaking")]
        public Kit_MapInformation[] lobbyMaps;
     
        public int lobbyMinimumPlayersNeeded = 8;
    
        public byte lobbyMaximumPlayers = 12;
      
        public int lobbyGameDuration = 600;
       
    
        public bool lobbyBotsEnabled;
     
        public int lobbyAmountOfMapsToVoteFor = 2;
     
        public float lobbyStartWithBotsAfterSeconds = 60f;

        [Header("Modules")]
        /// <summary>
        /// Which HUD prefab should be used for this game mode? Can be null.
        /// </summary>
        public GameObject hudPrefab;

        public Kit_SpawnSystemBase spawnSystemToUse;

        public Kit_BotGameModeManagerBase botManagerToUse;

        /// <summary>
        /// Use this to override bot controls
        /// </summary>
        public Kit_PlayerBotControlBase botControlOverride;

        public bool isTeamGameMode;

    
        public abstract void GamemodeSetup(Kit_IngameMain main);

        /// <summary>
        /// Called when the game mode starts when enough players are connected. Not called if there are enough players when the game mode initially began.
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeBeginMiddle(Kit_IngameMain main);

        /// <summary>
        /// Called every frame as long as this game mode is active
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeUpdate(Kit_IngameMain main);

        /// <summary>
        /// Called every frame as long as this game mode is active for other players
        /// </summary>
        /// <param name="main"></param>
        public virtual void GameModeUpdateOthers(Kit_IngameMain main)
        {

        }

           public abstract void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed);

    
        public virtual void OnPlayerSpawned(Kit_PlayerBehaviour pb)
        {

        }

      
        public virtual void OnPlayerDestroyed(Kit_PlayerBehaviour pb)
        {

        }

  
        public virtual void OnLocalPlayerSpawned(Kit_PlayerBehaviour pb)
        {

        }

  
        public virtual void OnLocalPlayerDestroyed(Kit_PlayerBehaviour pb)
        {

        }

         public virtual void LocalPlayerScoredKill(Kit_IngameMain main)
        {

        }

    
        public virtual void MasterClientBotScoredKill(Kit_IngameMain main, Kit_Bot bot)
        {

        }

  
        public abstract void TimeRunOut(Kit_IngameMain main);

 
        public abstract Transform GetSpawn(Kit_IngameMain main, Photon.Realtime.Player player);

  
        public abstract Transform GetSpawn(Kit_IngameMain main, Kit_Bot bot);

       
        public abstract bool CanSpawn(Kit_IngameMain main, Photon.Realtime.Player player);

  
        public virtual bool UsesCustomSpawn()
        {
            return false;
        }

        public virtual GameObject DoCustomSpawn(Kit_IngameMain main)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [players]!");
        }

        public virtual Loadout DoCustomSpawnBot(Kit_IngameMain main, Kit_Bot bot)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [bots]!");
        }

      
        public abstract bool CanJoinTeam(Kit_IngameMain main, Photon.Realtime.Player player, int team);

      
        public abstract bool CanControlPlayer(Kit_IngameMain main);

     
        public virtual void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {

        }

        /// <summary>
        /// Relay for serialization to sync data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Relay for serialization to sync custom game mode data that is stored on the player
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void PlayerOnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {

        }

     
        public abstract bool AreEnoughPlayersThere(Kit_IngameMain main);

     
        public virtual bool CanDropWeapons(Kit_IngameMain main)
        {
            return true;
        }

     
        public abstract bool AreWeEnemies(Kit_IngameMain main, bool botEnemy, int enemyId);

    
        public abstract bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo);

            public abstract bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, int playerTwoID, bool playerTwoBot, bool canKillSelf = false);

        /// <summary>
        /// Are these two players enemies? (Used by bullets.)
        /// </summary>
        /// <param name="playerOneID"></param>
        /// <param name="playerOneBot"></param>
        /// <param name="playerTwo"></param>
        /// <returns></returns>
        public abstract bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, Kit_PlayerBehaviour playerTwo, bool canKillSelf);

      
        public abstract bool CanStartVote(Kit_IngameMain main);

        public virtual bool LoadoutMenuSupported()
        {
            return true;
        }

   
        public virtual bool AutoSpawnSupported()
        {
            return true;
        }

        public virtual void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {

        }

#if UNITY_EDITOR
        /// <summary>
        /// For the scene checker, returns state to display
        /// </summary>
        public abstract string[] GetSceneCheckerMessages();

        /// <summary>
        /// For the scene checker, returns state to display
        /// </summary>
        /// <returns></returns>
        public abstract MessageType[] GetSceneCheckerMessageTypes();
#endif
    }
}
