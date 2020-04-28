using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    namespace UI
    {
    
        public class Kit_LoginSimple : Kit_MenuLogin
        {
      
            #region UI
            [Header("UI")]
            public GameObject loginRoot;
            public InputField userNameField;
            #endregion


            #region References
            [Header("References")]
            public Kit_MainMenu mainMenu; //The main menu
            #endregion

    
            private static bool alreadyLoggedIn;
 
            private static string alreadyLoggedInName;

            //The Login process is initiated
            public override void BeginLogin()
            {
                if (!alreadyLoggedIn)
                {
                    //Disable Menu
                    mainMenu.ChangeMenuState(MenuState.closed);
                    //Enable the name set window
                    loginRoot.SetActive(true);
                    //Generate a guest name
                    userNameField.text = "Guest(" + Random.Range(1, 1000) + ")";
                }
                else
                {
                    userNameField.text = alreadyLoggedInName;
                    Debug.Log("Already logged in");
                    //Disable Login window
                    loginRoot.SetActive(false);
                    //Sucess, continue
                    OnLoggedIn(alreadyLoggedInName);
                }
            }

            public void RequestLogin()
            {
                //Check if the user has set a name
                if (!userNameField.text.IsNullOrWhiteSpace())
                {
                    Debug.Log("Successfully logged in");
                    //Disable Login window
                    loginRoot.SetActive(false);
                    //Set name
                    alreadyLoggedInName = userNameField.text;
                    alreadyLoggedIn = true;
                    //Sucess, continue
                    OnLoggedIn(userNameField.text);
                }
            }
        }
    }
}
