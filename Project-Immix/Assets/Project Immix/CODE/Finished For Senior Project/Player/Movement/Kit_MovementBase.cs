using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_MovementBase : Kit_WeaponInjection
    {

        public abstract void CalculateMovementUpdate(Kit_PlayerBehaviour pb);


        public virtual void CalculateMovementLateUpdate(Kit_PlayerBehaviour pb) //This is optional
        {

        }

  
        public virtual void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit) //This is optional
        {

        }

   
        public abstract bool CanFire(Kit_PlayerBehaviour pb);

    
        public abstract bool IsRunning(Kit_PlayerBehaviour pb);

      
        public abstract int GetCurrentWeaponMoveAnimation(Kit_PlayerBehaviour pb);

       
        public abstract float GetCurrentWalkAnimationSpeed(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Retrieves the current local movement direction
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract Vector3 GetMovementDirection(Kit_PlayerBehaviour pb);

   
        public abstract void CalculateFootstepsUpdate(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Callback for photon serialization
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);

  
        public abstract Vector3 GetVelocity(Kit_PlayerBehaviour pb);

        /// <summary>
        /// A sound playing RPC was received (local or not local)
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="soundID"></param>
        /// <param name="arrayID"></param>
        public abstract void PlaySound(Kit_PlayerBehaviour pb, int soundID, int id2, int arrayID);

        /// <summary>
        /// An animation rpc was received (local or not local)
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="id"></param>
        public abstract void PlayAnimation(Kit_PlayerBehaviour pb, int id, int id2);

        public virtual void OnTriggerEnterRelay(Kit_PlayerBehaviour pb, Collider col)
        {

        }

        public virtual void OnTriggerExitRelay(Kit_PlayerBehaviour pb, Collider col)
        {

        }

         public virtual void OnCameraTriggerEnterRelay(Kit_PlayerBehaviour pb, Collider col)
        {

        }

        public virtual void OnCameraTriggerExitRelay(Kit_PlayerBehaviour pb, Collider col)
        {

        }
    }
}
