using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class WeaponIKValues
        {
            public Transform leftHandIK;
            public bool canUseIK;
        }

        public class WeaponStats
        {
    
            public float damage;
       
            public float fireRate;
        
            public float recoil;
      
            public float reach;
        }

        public class WeaponDisplayData
        {
     
            public Sprite sprite;
    
            public string name;
       
            public bool selected;
        }

        public class WeaponQuickUseDisplayData
        {
       
            public Sprite sprite;
        
            public string name;
   
            public int amount;
        }

        /// <summary>
        /// This script is executed when this weapon is active
        /// </summary>
        public abstract class Kit_WeaponBase : ScriptableObject
        {
      
            public string weaponName;

          
            public Sprite weaponPicture;

  
            public Sprite weaponHudPicture;

             public Sprite weaponQuickUsePicture;

        
            public string weaponType = "Primary";

        
            public int[] canFitIntoSlots = new int[1];

            public int levelToUnlockAt = -1;
            public Sprite unlockImage;

            public bool IsWeaponUnlocked(Kit_GameInformation game)
            {
                return true;
            }

            public bool IsWeaponUnlocked(int lvl)
            {
                
                return true;
            }

            #region Prefabs
            [Header("Prefabs")]
            public GameObject firstPersonPrefab; //The prefab to use for first person
            public GameObject thirdPersonPrefab; //The prefab to use for third person
            public GameObject dropPrefab; //The prefab to use for drop
            #endregion


            public string thirdPersonAnimType = "Rifle";
    
            public float drawTime = 0.5f;
       
            public float putawayTime = 0.5f; 
     
            public int deathSoundCategory;
   
            public virtual bool SupportsCustomization()
            {
                return true;
            }

  
            public virtual bool CanBeSelected(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return true;
            }

    
            public virtual bool SupportsQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }


            public virtual bool QuickUseSkipsPutaway(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

    
            public virtual float BeginQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0f;
            }


            public virtual float EndQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0f;
            }

            public virtual void EndQuickUseAfter(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }


            public virtual float BeginQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0f;
            }


            public virtual float EndQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0f;
            }

     
            public virtual void EndQuickUseAfterOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public virtual bool WaitForQuickUseButtonRelease()
            {
                return true;
            }

  
            public abstract void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData);

  
            public virtual void CalculateWeaponLateUpdate(Kit_PlayerBehaviour pb, object runtimeData) //This is optional
            {

            }

            public virtual WeaponDisplayData GetWeaponDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return null;
            }

            public virtual WeaponQuickUseDisplayData GetWeaponQuickUseDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return null;
            }

              public abstract void CalculateWeaponUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData);

    
            public virtual void CalculateWeaponLateUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData) //This is optional
            {

            }


            public abstract void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed);

              public abstract void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied);

    
            public virtual void OnControllerColliderHitCallback(Kit_PlayerBehaviour pb, object runtimeData, ControllerColliderHit hit) //This is optional
            {

            }

    
            public abstract void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData);

 
            public abstract void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData);

     
            public abstract void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData);

       
            public abstract void DrawWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData);

   
            public abstract void PutawayWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData);

      
            public abstract void PutawayWeaponHideOthers(Kit_PlayerBehaviour pb, object runtimeData);

       
            public abstract void SetupValues(int id);

     
            public abstract object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments);

    
            public abstract void SetupThirdPerson(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, object runtimeData, int[] attachments);

            public abstract object SetupThirdPersonOthers(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, int[] attachments);

            public abstract void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData);

  
            public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData);

            #region Weapon Network Relays
            /// <summary>
            /// We received a semi shot RPC
            /// </summary>
            public virtual void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            /// <summary>
            /// We received a bolt action shot RPC
            /// </summary>
            public virtual void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state)
            {

            }

            /// <summary>
            /// We received a burst fire RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int burstLength)
            {

            }

            /// <summary>
            /// We received a reload RPC
            /// </summary>
            /// <param name="isEmpty"></param>
            public virtual void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty, object runtimeData)
            {

            }

            /// <summary>
            /// We received a procedural reload RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="stage"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage, object runtimeData)
            {

            }

            /// <summary>
            /// Fire (dummy) physical bullet.
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="pos"></param>
            /// <param name="dir"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir, object runtimeData)
            {

            }

            /// <summary>
            /// Charge RPC received
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="id"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkMeleeChargeRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int id, int slot)
            {

            }

            /// <summary>
            /// Stab rpc received
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkMeleeStabRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state, int slot)
            {

            }

            /// <summary>
            /// PullPin RPC received
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkGrenadePullPinRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            /// <summary>
            /// Throw RPC received
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public virtual void NetworkGrenadeThrowRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }
            #endregion

            #region For Other Scripts
            public abstract bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData);

  
            public abstract bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData);

   
            public abstract float AimInTime(Kit_PlayerBehaviour pb, object runtimeData);


            public abstract float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData);

  
            public abstract float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData);

  
            public abstract WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData);

   
            public abstract WeaponStats GetStats();

                  public virtual bool SupportsStats()
            {
                return true;
            }


            public abstract int WeaponState(Kit_PlayerBehaviour pb, object runtimeData);

            public abstract int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData);
            #endregion

        }
    }
}