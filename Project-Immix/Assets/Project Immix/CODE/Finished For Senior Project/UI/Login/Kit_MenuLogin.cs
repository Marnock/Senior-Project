using UnityEngine;

namespace ImmixKit
{
    namespace UI
    {
  
        public abstract class Kit_MenuLogin : MonoBehaviour
        {
     
            public abstract void BeginLogin();
         
            public delegate void LoggedIn(string playerName);

            public LoggedIn OnLoggedIn;
        }
    }
}
