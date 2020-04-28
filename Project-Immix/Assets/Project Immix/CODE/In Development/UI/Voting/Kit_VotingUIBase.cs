using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_VotingUIBase : MonoBehaviourPunCallbacks
    {

        public Kit_IngameMain main;

     
        public abstract void OpenVotingMenu();

     
        public abstract void CloseVotingMenu();

     
        public abstract void RedrawVotingUI(Kit_VotingBase voting);

    
        public abstract void VoteEnded(Kit_VotingBase voting);
    }
}
