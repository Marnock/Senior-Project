using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
   
    public abstract class Kit_PlayerBotControlBase : ScriptableObject
    {
     
        public abstract void InitializeControls(Kit_PlayerBehaviour pb);

 
        public abstract void WriteToPlayerInput(Kit_PlayerBehaviour pb);


        public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);
    }
}