using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{

    public class Kit_SimpleVictoryScreen : Kit_VictoryScreenUI
    {
   
        public Kit_IngameMain main;
    
        public GameObject root;

        [Header("Player Win")]
 
        public GameObject pwRoot;
  
        public Text pwVictoryLoose;
    
        public Text pwName;

        [Header("Team")]
      
        public GameObject teamWinRoot;
      
        public Text teamWinVictoryLoose;
     
        public GameObject teamWinScore;
    
        public Text teamWinScoreTeamOne;
  
        public Text teamWinScoreTeamTwo;

        [Header("Draw")]
       
        public GameObject drawRoot;
     
        public GameObject drawScore;
      
        public Text drawScoreTeamOne;
    
        public Text drawScoreTeamTwo;

        public override void CloseUI()
        {
            //Disable root
            root.SetActive(false);
        }

        public override void DisplayPlayerWinner(Photon.Realtime.Player winner)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            //Check if we won
            if (winner == PhotonNetwork.LocalPlayer)
            {
                //We won
                //Display victory
                pwVictoryLoose.text = "Victory!";
                //Display the name
                pwName.text = winner.NickName;
            }
            else
            {
                //We lost
                //Display loose
                pwVictoryLoose.text = "Defeat!";
                //Display the name
                pwName.text = winner.NickName;
            }

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayBotWinner(Kit_Bot winner)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            //We lost
            //Display loose
            pwVictoryLoose.text = "Defeat!";
            //Display the name
            pwName.text = winner.name;

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

    }
}
