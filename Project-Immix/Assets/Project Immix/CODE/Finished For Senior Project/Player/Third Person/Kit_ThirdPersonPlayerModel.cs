using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This script is the base for a third person model behaviour
    /// </summary>
    public abstract class Kit_ThirdPersonPlayerModel : MonoBehaviour
    {
           public Animator anim;

        [Header("Weapon Placement")]

        public Transform weaponsInHandsGo;

        [Header("Sounds")]

        public AudioSource soundFire;


        public AudioSource soundReload;

   
        public AudioSource soundOther;


        public AudioSource soundVoice;

        [Header("Name Above Head")]
 
        public Collider enemyNameAboveHeadTrigger;

        public Transform enemyNameAboveHeadPos;

        [Header("Customization")]
     
        public CustomizationSlot[] customizationSlots;

    
        [HideInInspector]
        public Kit_PlayerModelInformation information;


        public abstract void SetupModel(Kit_PlayerBehaviour kpb);
    
        public abstract void FirstPerson();
  
        public abstract void ThirdPerson();


        public abstract void SetAnimType(string animType, bool noTrans = false);


        public abstract void PlayWeaponFireAnimation(string animType);

  
        public abstract void PlayWeaponReloadAnimation(string animType);

         public abstract void PlayMeleeAnimation(int animation, int state);

 
        public abstract void PlayGrenadeAnimation(int animation);

    
        public abstract void AbortWeaponAnimations();

        /// <summary>
        /// Called when we die (for everyone). Create ragdoll.
        /// </summary>
        public abstract void CreateRagdoll();


        public abstract void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective);

       public void SetCustomizations(int[] enabledCustomizations, Kit_PlayerBehaviour pb)
        {

        } 
    }
}
