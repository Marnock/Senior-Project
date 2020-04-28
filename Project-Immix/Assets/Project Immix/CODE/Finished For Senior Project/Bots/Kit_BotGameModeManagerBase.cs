using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_BotGameModeManagerBase : ScriptableObject
    {
    
        public abstract void Inizialize(Kit_BotManager manager);
     
        public abstract void PlayerJoinedTeam(Kit_BotManager manager);
      
        public abstract void PlayerLeftTeam(Kit_BotManager manager);
    }
}
