using UnityEngine;
using UnityEngine.Events;

namespace ImmixKit
{
    public class PlayerSpawnedEvent : UnityEvent<Kit_PlayerBehaviour>
    {

    }

    public class PlayerDiedEvent : UnityEvent<Kit_PlayerBehaviour>
    {

    }

    public class PlayerWonEvent : UnityEvent<Kit_Player>
    {

    }

    public class TeamWonEvent : UnityEvent<int>
    {

    }

    public class TeamWonWithScoreEvent : UnityEvent<int, int, int>
    {

    }

    public class TeamSwitchedEvent : UnityEvent<int>
    {

    }

    /// <summary>
    /// This is the kit's event system. 
    /// </summary>
    public class Kit_Events
    {
        /// <summary>
        /// Called when a player spawns
        /// </summary>
        public static PlayerSpawnedEvent onPlayerSpawned = new PlayerSpawnedEvent();
       
        public static PlayerDiedEvent onPlayerDied = new PlayerDiedEvent();
     
        public static PlayerWonEvent onEndGamePlayerWin = new PlayerWonEvent();
       
        public static TeamWonEvent onEndGameTeamWin = new TeamWonEvent();
      
        public static TeamWonWithScoreEvent onEndGameTeamWinWithScore = new TeamWonWithScoreEvent();
    
        public static TeamSwitchedEvent onTeamSwitched = new TeamSwitchedEvent();
    }
}