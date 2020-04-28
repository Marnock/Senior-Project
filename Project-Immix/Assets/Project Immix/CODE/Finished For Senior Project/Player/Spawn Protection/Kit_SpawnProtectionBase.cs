using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_SpawnProtectionBase : ScriptableObject
    {
        public virtual void CustomStart(Kit_PlayerBehaviour pb)
        {

        }

        public virtual void CustomUpdate(Kit_PlayerBehaviour pb)
        {

        }

        public virtual void PlayerMoved(Kit_PlayerBehaviour pb)
        {

        }


        public virtual void GunFired(Kit_PlayerBehaviour pb)
        {

        }


        public abstract bool CanTakeDamage(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Photonview Serialize callback
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {

        }
    }
}
