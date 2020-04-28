using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_VotingBase : MonoBehaviourPunCallbacks
    {
        public enum VotingOn { Kick = 0, Map = 1, GameMode = 2 }

        public VotingOn votingOn;
  
        public int argument;

    
        public int myVote = -1;

        public Photon.Realtime.Player voteStartedBy;

     
        public void VoteYes()
        {

            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = 1;
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }
            myVote = 1;
        }


        public void VoteNo()
        {
    
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = 0;
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }
            myVote = 0;
        }


        public abstract int GetYesVotes();

         public abstract int GetNoVotes();
    }
}
