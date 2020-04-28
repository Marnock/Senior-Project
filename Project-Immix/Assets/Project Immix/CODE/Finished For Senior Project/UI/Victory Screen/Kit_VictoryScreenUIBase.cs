using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_VictoryScreenUI : MonoBehaviour
    {

        public abstract void DisplayPlayerWinner(Photon.Realtime.Player winner);

     
        public abstract void DisplayBotWinner(Kit_Bot winner);
    
        public abstract void CloseUI();
    }
}
