using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    namespace UI
    {
        public class Kit_MenuDefaultPlayerState : Kit_MenuPlayerStateBase
        {
     
            public Text username;

     
            public Image rank;

       
            public Image levelUpFill;

          
            public Text levelUpPercentage;

            public override void Initialize(Kit_MainMenu main)
            {
                if (username) username.text = Kit_GameSettings.userName;

            }
        }
    }
}
