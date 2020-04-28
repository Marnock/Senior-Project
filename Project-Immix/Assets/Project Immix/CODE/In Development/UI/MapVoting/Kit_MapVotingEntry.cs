using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    /// <summary>
    /// This class holds information for a voting entry
    /// </summary>
    public class Kit_MapVotingEntry : MonoBehaviour
    {
  
        public Image mapImage;

        public Text mapName;
 
        public Text gameModeName;

  
        public Image votePercentageImage;

        public Text votePercentageText;


        public int myVote;

     
        public void VoteForThis()
        {
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            table["vote"] = myVote;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        }
    }
}
