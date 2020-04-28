using UnityEngine;

namespace ImmixKit
{
    namespace Scoreboard
    {
        public abstract class Kit_ScoreboardBase : MonoBehaviour
        {
  
            public bool isOpen;

      
            public abstract void Enable();

      
            public abstract void Disable();
        }
    }
}
