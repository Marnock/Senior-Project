using UnityEngine;
using System.Collections.Generic;

namespace ImmixKit
{
    public abstract class Kit_MapVotingUIBase : MonoBehaviour
    {
  
        public int amountOfAvailableVotes = 4;

    
        public abstract void SetupVotes(List<MapGameModeCombo> combos);

      
        public abstract void RedrawVotes(Kit_MapVotingBehaviour behaviour);

        public abstract void Hide();
    }
}