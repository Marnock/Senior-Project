using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace ImmixKit
{

    public abstract class Kit_MouseLookBase : ScriptableObject
    {
        public virtual void StartLocalPlayer(Kit_PlayerBehaviour pb)
        {

        }

        public abstract void CalculateLookUpdate(Kit_PlayerBehaviour pb);

 
        public abstract bool ReachedYMax(Kit_PlayerBehaviour pb);

  
        public virtual void CalculateLookLateUpdate(Kit_PlayerBehaviour pb) 
        {

        }

   
        public virtual void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
        {

        }

        public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);

     
        public abstract float GetSpeedMultiplier(Kit_PlayerBehaviour pb);

              public virtual Vector3 GetCameraOffset(Kit_PlayerBehaviour pb)
        {
            return Vector3.zero;
        }

     
        public virtual Quaternion GetCameraRotationOffset(Kit_PlayerBehaviour pb)
        {
            return Quaternion.identity;
        }

    
        public virtual Vector3 GetWeaponOffset(Kit_PlayerBehaviour pb)
        {
            return Vector3.zero;
        }

            public virtual Quaternion GetWeaponRotationOffset(Kit_PlayerBehaviour pb)
        {
            return Quaternion.identity;
        }

        #region Perspective manager
      
        public abstract Kit_GameInformation.Perspective GetPerspective(Kit_PlayerBehaviour pb);
        #endregion
    }
}
