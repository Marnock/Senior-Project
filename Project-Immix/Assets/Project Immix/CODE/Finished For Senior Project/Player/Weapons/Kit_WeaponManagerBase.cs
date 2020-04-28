using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {

        public abstract class Kit_WeaponManagerBase : ScriptableObject
        {
           
            public abstract void SetupManager(Kit_PlayerBehaviour pb, object[] instantiationData);

        
            public abstract void SetupManagerBot(Kit_PlayerBehaviour pb, object[] instantiationData);

        
            public abstract void SetupManagerOthers(Kit_PlayerBehaviour pb, object[] instantiationData);

     
            public abstract void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective);

            public abstract void ForceUnselectCurrentWeapon(Kit_PlayerBehaviour pb);

            #region Unity Callback
      
            public abstract void CustomUpdate(Kit_PlayerBehaviour pb);

     
            public abstract void CustomUpdateOthers(Kit_PlayerBehaviour pb);

            public virtual void PlayerDead(Kit_PlayerBehaviour pb) { }

  
            public abstract void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit);

   
            public abstract void OnAnimatorIKCallback(Kit_PlayerBehaviour pb, Animator anim);

            public abstract void FallDownEffect(Kit_PlayerBehaviour pb, bool wasFallDamageApplied);
            #endregion

            #region Photon stuff
   
            public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);
            #endregion

            #region Values for other systems
 
            public abstract int WeaponState(Kit_PlayerBehaviour pb);

      
            public abstract bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb);

    
            public abstract float AimInTime(Kit_PlayerBehaviour pb);


            public abstract int WeaponType(Kit_PlayerBehaviour pb);

     
            public abstract bool IsAiming(Kit_PlayerBehaviour pb);

     
            public abstract float CurrentMovementMultiplier(Kit_PlayerBehaviour pb);

            public abstract float CurrentSensitivity(Kit_PlayerBehaviour pb);

            public abstract bool CanRun(Kit_PlayerBehaviour pb);
            #endregion

            #region Weapon Network Relays
            /// <summary>
            /// We received a semi shot RPC
            /// </summary>
            public abstract void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb);

            /// <summary>
            /// We received a bolt action RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="state">The state of this bolt action</param>
            public abstract void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, int state);

            /// <summary>
            /// We received a burst RPC
            /// </summary>
            /// <param name="pb"></param>
            public abstract void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, int burstLength);

            /// <summary>
            /// We received a reload RPC
            /// </summary>
            /// <param name="isEmpty"></param>
            public abstract void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty);

            /// <summary>
            /// We received a procedural reload RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="stage"></param>
            public abstract void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage);

   
            public abstract void NetworkMeleeStabRPCReceived(Kit_PlayerBehaviour pb, int state, int slot);

            public abstract void NetworkMeleeChargeRPCReceived(Kit_PlayerBehaviour pb, int state, int slot);

            public abstract void NetworkGrenadePullPinRPCReceived(Kit_PlayerBehaviour pb);

    
            public abstract void NetworkGrenadeThrowRPCReceived(Kit_PlayerBehaviour pb);

  
            public abstract void NetworkReplaceWeapon(Kit_PlayerBehaviour pb, int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments);


            public abstract void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir);
            #endregion


            #region Plugin System Calls
            public abstract int[] GetCurrentlySelectedWeapon(Kit_PlayerBehaviour pb);

            public abstract int[] GetCurrentlyDesiredWeapon(Kit_PlayerBehaviour pb);

 
            public abstract void PluginSelectWeapon(Kit_PlayerBehaviour pb, int slot, int id, bool locked = true);
            #endregion
        }
    }
}