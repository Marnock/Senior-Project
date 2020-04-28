using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    namespace UI
    {
        /// <summary>
        /// This class contains references for the server browser and acts as a sender  
        /// </summary>
        public class Kit_ServerBrowserEntry : MonoBehaviour
        {
            public Text serverName; //The name of this room
            public Text mapName; //The map that is currently played in this room
            public Text gameModeName; //The game mode that is currently played in this room
            public Text players; //How many players are in this room
            public Text ping; //The ping of this room - The cloud
            public Text password; //If this room is password protected

            private Kit_MainMenu mm; //The current Main Menu
            private RoomInfo myRoom;

   
            public void Setup(Kit_MainMenu curMm, RoomInfo curRoom)
            {
                mm = curMm;
                myRoom = curRoom;

                if (myRoom != null)
                {
                    //Set Info
                    serverName.text = myRoom.Name;
                    int gameMode = (int)myRoom.CustomProperties["gameMode"];
                    //Game Mode
                    gameModeName.text = mm.gameInformation.allGameModes[gameMode].gameModeName;
                    //Map
                    mapName.text = mm.gameInformation.allGameModes[gameMode].traditionalMaps[(int)myRoom.CustomProperties["map"]].mapName;
                    bool bots = (bool)myRoom.CustomProperties["bots"];
                    if (bots)
                    {
                        //Players
                        players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers + " (bots)";
                    }
                    else
                    {
                        //Players
                        players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers;
                    }
                    //Ping
                    ping.text = PhotonNetwork.GetPing().ToString();
                    //Password
                    if (myRoom.CustomProperties["password"] != null && ((string)myRoom.CustomProperties["password"]).Length > 0) password.text = "Yes";
                    else password.text = "No";
                }

                //Reset scale (Otherwise it will be offset)
                transform.localScale = Vector3.one;
            }

            //Called from the button that is on this prefab, to join this room (attempt)
            public void OnClick()
            {
                //Check if this button is ready
                if (mm)
                {
                    if (myRoom != null)
                    {
                        //Attempt to join
                        mm.JoinRoom(myRoom);
                    }
                }
            }
        }
    }
}