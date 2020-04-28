using UnityEngine;

namespace ImmixKit
{
    public enum PlayerNameState { none, friendlyClose, friendlyFar, enemy }

  
    public abstract class Kit_PlayerNameUIBase : ScriptableObject
    {
   
        public abstract void StartRelay(Kit_PlayerBehaviour pb);

    
        public abstract void UpdateFriendly(Kit_PlayerBehaviour pb);


        public abstract void UpdateEnemy(Kit_PlayerBehaviour pb);

        public abstract void OnDestroyRelay(Kit_PlayerBehaviour pb);

        public abstract void PlayerSpotted(Kit_PlayerBehaviour pb, float validFor);
    }
}