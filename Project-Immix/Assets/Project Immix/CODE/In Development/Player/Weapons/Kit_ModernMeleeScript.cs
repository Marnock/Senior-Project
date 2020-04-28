using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class MeleeControllerRuntimeData
        {
            public Kit_MeleeRenderer meleeRenderer;
            public Kit_ThirdPersonMeleeRenderer tpMeleeRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); 

      
            public bool isSelectedAndReady = false;

            public Animator genericAnimator;

            #region Weapon Delay
  
            public Transform weaponDelayTransform;
 
            public Vector3 weaponDelayCur;
  
            public float weaponDelayCurrentX;

            public float weaponDelayCurrentY;
            #endregion

            #region Weapon Fall
            public Transform weaponFallTransform;
            #endregion

            #region Run Animation

            public bool startedRunAnimation;
            #endregion

            #region Sound
            /// <summary>
            /// Audio source used for fire sounds
            /// </summary>
            public AudioSource sounds;
            #endregion


            public float lastRun;

     
            public int lastWeaponAnimationID;

            #region Attack
            public bool isCharging;
 
            public float chargingProgress = 0f;
   
            public bool chargingPrimary;
     
            public bool chargingSecondary;
    
            public float nextActionPossibleAt;
    
            public float quickChargeStartedAt;
            #endregion

            #region Input
            public bool lastLmb;
            public bool lastRmb;
            #endregion
        }

        public class MeleeControllerOthersRuntimeData
        {
            public Kit_ThirdPersonMeleeRenderer tpMeleeRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); 
        }

        public enum AttackType { Stab, Charge, None }

        [System.Serializable]
        public class AttackSettings
        {
 
            [Header("Stab")]
            public float stabDamage = 50f;
   
            public int stabPenetrationPower = 2;
     
            public float stabReach = 2f;
  
            public Vector3 stabHalfExtents = new Vector3(0.1f, 0.1f, 1f);
    
            public float stabRagdollForce = 500f;
            [Header("Windup")]
   
            public string stabWindupAnimationName = "Stab Windup";
  
            public AudioClip stabWindupSound;
 
            public float stabWindupTime = 0.3f;
            [Header("Hit")]
 
            public string stabAnimationHitName = "Stab Hit";
       
            public AudioClip stabHitSound;
     
            public float stabHitTime = 0.5f;
            [Header("Hit Object")]
       
            public string stabAnimationHitObjectName = "Stab Hit Object";
    
            public AudioClip stabHitObjectSound;
        
            public float stabHitObjectTime = 0.5f;
            [Header("Miss")]
    
            public string stabAnimationMissName = "Stab Miss";
     
            public AudioClip stabMissSound;
     
            public float stabMissTime = 0.5f;
            [Header("Charge")]
            public float chargeDamageStart = 10f;
     
            public float chargeDamageCharged = 90f;
   
            public int chargePenetrationPower = 2;
     
            public float chargeReach = 2f;
               public Vector3 chargeHalfExtents = new Vector3(0.1f, 0.1f, 1f);

            public float chargeRagdollForce = 500f;
            [Header("Charge")]
      
            public string chargeChargeAnimation = "Charge Windup";

            public AudioClip chargeChargeSound;
    
            public float chargeChargeTime = 0.3f;
            [Header("Windup")]
    
            public string chargeWindupAnimationName = "Charge Windup";
  
            public AudioClip chargeWindupSound;
            public float chargeWindupTime = 0.3f;
            [Header("Hit")]

            public string chargeAnimationHitName = "Charge Hit";
    
            public AudioClip chargeHitSound;
     
            public float chargeHitTime = 0.5f;
            [Header("Hit Object")]
   
            public string chargeAnimationHitObjectName = "Charge Hit Object";
  
            public AudioClip chargeHitObjectSound;

            public float chargeHitObjectTime = 0.5f;
            [Header("Miss")]
    
            public string chargeAnimationMissName = "Charge Miss";
     
            public AudioClip chargeMissSound;
      
            public float chargeMissTime = 0.5f;
        }

        public class Kit_ModernMeleeScript : Kit_WeaponBase
        {
            #region Attack
            [Header("Attacks")]
            public AttackType primaryAttack = AttackType.Stab;
            public AttackSettings primaryAttackSettings = new AttackSettings();
    
            public AttackType secondaryAttack = AttackType.Charge;
   
            public AttackSettings secondaryAttackSettings = new AttackSettings();
      
            public AttackType quickAttack = AttackType.Stab;
       
            public AttackSettings quickAttackSettings = new AttackSettings();
       
            public bool quickAttackSkipsPutaway = true;
            #endregion

            #region Sounds
            [Header("Sounds")]
     
            public AudioClip drawSound;
      
            public AudioClip putawaySound;
    
            public int voiceMeleeSoundID;
            #endregion

            #region Weapon Delay
            [Header("Weapon Delay")]
    
            public float weaponDelayBaseAmount = 1f;
     
            public float weaponDelayMaxAmount = 0.02f;
      
            public float weaponDelayAimingMultiplier = 0.3f;
 
            public float weaponDelaySmooth = 3f;
            #endregion

            #region Weapon Tilt
    
            public bool weaponTiltEnabled = true;
     
            public float weaponTiltIntensity = 5f;
  
            public float weaponTiltReturnSpeed = 3f;
            #endregion

            #region Weapon Fall
            [Header("Fall Down effect")]
            public float fallDownAmount = 10.0f;
            public float fallDownMinOffset = -6.0f;
            public float fallDownMaxoffset = 6.0f;
            public float fallDownTime = 0.1f;
            public float fallDownReturnSpeed = 1f;
            #endregion

            #region Generic Animations
            [Header("Generic Animations")]
     
            public GameObject genericGunAnimatorControllerPrefab;
            public bool useGenericWalkAnim = true;

            public bool useGenericRunAnim = true;
            #endregion

            [HideInInspector]
            public int gameGunID;

            public override WeaponDisplayData GetWeaponDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (primaryAttack != AttackType.None && secondaryAttack != AttackType.None)
                {
                    WeaponDisplayData wdd = new WeaponDisplayData();
                    wdd.sprite = weaponHudPicture;
                    wdd.name = weaponName;
                    return wdd;
                }
                else
                {
                    return null;
                }
            }

            public override WeaponQuickUseDisplayData GetWeaponQuickUseDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (quickAttack != AttackType.None)
                {
                    WeaponQuickUseDisplayData wdd = new WeaponQuickUseDisplayData();
                    wdd.sprite = weaponQuickUsePicture;
                    wdd.name = weaponName;
                    wdd.amount = 1;
                    return wdd;
                }
                else
                {
                    return null;
                }
            }

            public override bool SupportsCustomization()
            {
                return false;
            }

            public override bool CanBeSelected(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (primaryAttack == AttackType.None && secondaryAttack == AttackType.None) return false;
                return true;
            }

            public override bool SupportsQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (quickAttack == AttackType.None) return false;
                return true;
            }

            public override bool QuickUseSkipsPutaway(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return quickAttackSkipsPutaway;
            }

            public override bool WaitForQuickUseButtonRelease()
            {
                return primaryAttack == AttackType.Charge;
            }

            public override float BeginQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                    if (!pb.isBot)
                    {
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.meleeRenderer.visible = true;
                        }
                        else
                        {
                            data.meleeRenderer.visible = false;
                        }
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.enabled = true;
                        }
  
                    }

                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon and hide
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = false;
                    }

                    if (quickAttack == AttackType.Stab)
                    {
                        //Start Coroutine!
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecuteQuickStab(this, data, pb));
                        return quickAttackSettings.stabWindupTime + quickAttackSettings.stabHitTime;
                    }
                    else if (quickAttack == AttackType.Charge)
                    {
                        ///Start charging!
                        if (!pb.isBot)
                        {
                            if (quickAttackSettings.chargeChargeAnimation != "")
                            {
                                //Play animation
                                if (data.meleeRenderer.anim)
                                {
                                    data.meleeRenderer.anim.Play(quickAttackSettings.chargeChargeAnimation);
                                }
   
                            }
                            //Play sound
                            if (quickAttackSettings.chargeChargeSound)
                            {
                                data.sounds.clip = quickAttackSettings.chargeChargeSound;
                                data.sounds.Play();
                            }
                        }

                        //Set charge time
                        data.quickChargeStartedAt = Time.time;

                        return 0f;
                    }
                }

                //In case of failure...
                return 0f;
            }

            public override float EndQuickUse(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                    if (quickAttack == AttackType.Stab)
                    {
                        //Do nuthin'
                        return 0f;
                    }
                    else if (quickAttack == AttackType.Charge)
                    {
                        //Finish him.
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecuteQuickCharge(this, data, pb));
                        return quickAttackSettings.chargeWindupTime + quickAttackSettings.chargeHitTime;
                    }
                }

                //In case of failure...
                return 0f;
            }

            public override void EndQuickUseAfter(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                    if (!pb.isBot)
                    {
                        data.meleeRenderer.visible = false;
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.enabled = false;
                        }

                    }

                    data.tpMeleeRenderer.visible = false;
                }
            }

            public override float BeginQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;

                    //Make weapon visible!
                    data.tpMeleeRenderer.visible = true;

                    if (quickAttack == AttackType.Stab)
                    {
                        return quickAttackSettings.stabWindupTime + quickAttackSettings.stabHitTime;
                    }
                    else if (quickAttack == AttackType.Charge)
                    {
                        return 0f;
                    }
                }

                //In case of failure...
                return 0f;
            }

            public override float EndQuickUseOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;

                }

                //In case of failure...
                return 0f;
            }

            public override void EndQuickUseAfterOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;

                    if (data.tpMeleeRenderer)
                    {
                        data.tpMeleeRenderer.visible = false;
                    }
                }
            }

            public override float AimInTime(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0.5f;
            }

            public override void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed)
            {
                if (pb.isBot) return;
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                    //Camera animation
                    if (data.meleeRenderer.cameraAnimationEnabled)
                    {
                        if (data.meleeRenderer.cameraAnimationType == CameraAnimationType.Copy)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.meleeRenderer.cameraAnimationReferenceRotation) * data.meleeRenderer.cameraAnimationBone.localRotation;
                        }
                        else if (data.meleeRenderer.cameraAnimationType == CameraAnimationType.LookAt)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.meleeRenderer.cameraAnimationReferenceRotation) * Quaternion.LookRotation(data.meleeRenderer.cameraAnimationTarget.localPosition - data.meleeRenderer.cameraAnimationBone.localPosition);
                        }
                    }
                    else
                    {
                        //Go back to 0,0,0
                        pb.playerCameraAnimationTransform.localRotation = Quaternion.Slerp(pb.playerCameraAnimationTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                    }

                    //Weapon delay calculation
                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        //Get input from the mouse
                        data.weaponDelayCurrentX = -pb.input.mouseX * weaponDelayBaseAmount * Time.deltaTime;
                        if (!pb.looking.ReachedYMax(pb)) //Check if we should have delay on y looking
                            data.weaponDelayCurrentY = -pb.input.mouseY * weaponDelayBaseAmount * Time.deltaTime;
                        else //If not, just set it to zero
                            data.weaponDelayCurrentY = 0f;
                    }
                    else
                    {
                        //Cursor is not locked, set values to zero
                        data.weaponDelayCurrentX = 0f;
                        data.weaponDelayCurrentY = 0f;
                    }

                    //Clamp
                    data.weaponDelayCurrentX = Mathf.Clamp(data.weaponDelayCurrentX, -weaponDelayMaxAmount, weaponDelayMaxAmount);
                    data.weaponDelayCurrentY = Mathf.Clamp(data.weaponDelayCurrentY, -weaponDelayMaxAmount, weaponDelayMaxAmount);

                    //Update Vector
                    data.weaponDelayCur.x = data.weaponDelayCurrentX;
                    data.weaponDelayCur.y = data.weaponDelayCurrentY;
                    data.weaponDelayCur.z = 0f;

                    //Smooth move towards the target
                    data.weaponDelayTransform.localPosition = Vector3.Lerp(data.weaponDelayTransform.localPosition, data.weaponDelayCur, Time.deltaTime * weaponDelaySmooth);

                    //Weapon tilt
                    if (weaponTiltEnabled)
                    {
                        data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.Euler(0, 0, -pb.movement.GetMovementDirection(pb).x * weaponTiltIntensity), Time.deltaTime * weaponTiltReturnSpeed);
                    }
                    else
                    {
                        data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                    }

                    //Weapon Fall
                    data.weaponFallTransform.localRotation = Quaternion.Slerp(data.weaponFallTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                    //Set speed
                    if (id != 0)
                    {
                        data.genericAnimator.SetFloat("speed", speed);
                    }
                    //If idle, set speed to 1
                    else
                    {
                        data.genericAnimator.SetFloat("speed", 1f);
                    }

                    //Run position and rotation
                    //Check state and if we can move
                    if (id == 2 && data.isSelectedAndReady)
                    {
                        //Move to run pos
                        data.meleeRenderer.transform.localPosition = Vector3.Lerp(data.meleeRenderer.transform.localPosition, data.meleeRenderer.runPos, Time.deltaTime * data.meleeRenderer.runSmooth);
                        //Move to run rot
                        data.meleeRenderer.transform.localRotation = Quaternion.Slerp(data.meleeRenderer.transform.localRotation, Quaternion.Euler(data.meleeRenderer.runRot), Time.deltaTime * data.meleeRenderer.runSmooth);
                        //Set time
                        data.lastRun = Time.time;
                    }
                    else
                    {
                        //Move back to idle pos
                        data.meleeRenderer.transform.localPosition = Vector3.Lerp(data.meleeRenderer.transform.localPosition, Vector3.zero, Time.deltaTime * data.meleeRenderer.runSmooth * 2f);
                        //Move back to idle rot
                        data.meleeRenderer.transform.localRotation = Quaternion.Slerp(data.meleeRenderer.transform.localRotation, Quaternion.identity, Time.deltaTime * data.meleeRenderer.runSmooth * 2f);
                    }


                    //Check if state changed
                    if (id != data.lastWeaponAnimationID)
                    {
                        //Idle
                        if (id == 0)
                        {
                            //Play idle animation
                            data.genericAnimator.CrossFade("Idle", 0.3f);

                            if (!useGenericRunAnim)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("Start Run");
                                        data.meleeRenderer.anim.SetTrigger("End Run");
                                    }

                                }
                            }
                        }
                        //Walk
                        else if (id == 1)
                        {
                            //Check if we should use generic anim
                            if (useGenericWalkAnim)
                            {
                                //Play run animation
                                data.genericAnimator.CrossFade("Walk", 0.2f);
                            }
                            //If not continue to play Idle
                            else
                            {
                                //Play idle animation
                                data.genericAnimator.CrossFade("Idle", 0.3f);
                            }

                            if (!useGenericRunAnim)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("Start Run");
                                        data.meleeRenderer.anim.SetTrigger("End Run");
                                    }
     
                                }
                            }
                        }
                        //Run
                        else if (id == 2)
                        {
                            //Check if we should use generic anim
                            if (useGenericRunAnim)
                            {
                                //Play run animation
                                data.genericAnimator.CrossFade("Run", 0.2f);
                            }
                            //If not continue to play Idle
                            else
                            {
                                //Play idle animation
                                data.genericAnimator.CrossFade("Idle", 0.3f);
                                //Start run animation on weapon animator
                                if (!data.startedRunAnimation && data.isSelectedAndReady)
                                {
                                    data.startedRunAnimation = true;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("End Run");
                                        data.meleeRenderer.anim.SetTrigger("Start Run");
                                    }
             
                                }
                            }
                        }
                        //Update last state
                        data.lastWeaponAnimationID = id;
                    }
                    else
                    {
                        if (!useGenericRunAnim)
                        {
                            //Idle
                            if (id == 0)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("Start Run");
                                        data.meleeRenderer.anim.SetTrigger("End Run");
                                    }
            
                                }
                            }
                            //Walk
                            else if (id == 1)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("Start Run");
                                        data.meleeRenderer.anim.SetTrigger("End Run");
                                    }
   
                                }
                            }
                            //Run
                            else if (id == 2)
                            {
                                //Start run animation on weapon animator
                                if (!data.startedRunAnimation && data.isSelectedAndReady)
                                {
                                    data.startedRunAnimation = true;
                                    if (data.meleeRenderer.anim)
                                    {
                                        data.meleeRenderer.anim.ResetTrigger("End Run");
                                        data.meleeRenderer.anim.SetTrigger("Start Run");
                                    }

                                }
                            }
                        }
                    }
                }
            }

            public override void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                    //Set this weapon to selected and ready (for other things)
                    data.isSelectedAndReady = true;

                    //Input
                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        if (primaryAttack == AttackType.Stab)
                        {
                            //Check for input
                            if (data.lastLmb != pb.input.lmb)
                            {
                                data.lastLmb = pb.input.lmb;
                                if (pb.input.lmb)
                                {
                                    if (Time.time > data.nextActionPossibleAt && !data.isCharging)
                                    {
                                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecutePrimaryStab(this, data, pb));
                                    }
                                }
                            }
                        }
                        else if (primaryAttack == AttackType.Charge)
                        {
                            if (!data.chargingSecondary)
                            {
                                if (pb.input.lmb)
                                {
                                    if (Time.time > data.nextActionPossibleAt)
                                    {
                                        if (!data.isCharging)
                                        {
                                            ///Start charging!
                                            if (!pb.isBot)
                                            {
                                                if (primaryAttackSettings.chargeChargeAnimation != "")
                                                {
                                                    //Play animation
                                                    if (data.meleeRenderer.anim)
                                                    {
                                                        data.meleeRenderer.anim.Play(primaryAttackSettings.chargeChargeAnimation);
                                                    }
                  
                                                }
                                                //Play sound
                                                if (primaryAttackSettings.chargeChargeSound)
                                                {
                                                    data.sounds.clip = primaryAttackSettings.chargeChargeSound;
                                                    data.sounds.Play();
                                                }
                                            }

                                            //Call network
                                            pb.photonView.RPC("MeleeChargeNetwork", RpcTarget.Others, 0, 0);
                                            //Play third person reload anim
                                            pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 0);
                                        }
                                        else
                                        {
                                            //Increase progress
                                            if (data.chargingProgress < 1f)
                                            {
                                                data.chargingProgress += Time.deltaTime / primaryAttackSettings.chargeChargeTime;
                                            }
                                        }

                                        //Set bool
                                        data.chargingPrimary = true;
                                        data.isCharging = true;
                                    }
                                }
                                else
                                {
                                    if (data.isCharging)
                                    {
                                        //RELEASE...
                                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecutePrimaryCharge(this, data, pb));
                                        //Reset bools
                                        data.isCharging = false;
                                        data.chargingPrimary = false;
                                        //data.chargingProgress = 0f;
                                    }
                                }
                            }
                        }

                        if (secondaryAttack == AttackType.Stab)
                        {
                            //Check for input
                            if (data.lastRmb != pb.input.rmb)
                            {
                                data.lastRmb = pb.input.rmb;
                                if (pb.input.rmb)
                                {
                                    if (Time.time > data.nextActionPossibleAt && !data.isCharging)
                                    {
                                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecuteSecondaryStab(this, data, pb));
                                    }
                                }
                            }
                        }
                        else if (secondaryAttack == AttackType.Charge)
                        {
                            if (!data.chargingPrimary)
                            {
                                if (pb.input.rmb)
                                {
                                    if (Time.time > data.nextActionPossibleAt)
                                    {
                                        if (!data.isCharging)
                                        {
                                            //Start charging!
                                            if (!pb.isBot)
                                            {
                                                if (secondaryAttackSettings.chargeChargeAnimation != "")
                                                {
                                                    //Play animation
                                                    if (data.meleeRenderer.anim)
                                                    {
                                                        data.meleeRenderer.anim.Play(secondaryAttackSettings.chargeChargeAnimation);
                                                    }
                
                                                }
                                                //Play sound
                                                if (secondaryAttackSettings.chargeChargeSound)
                                                {
                                                    data.sounds.clip = secondaryAttackSettings.chargeChargeSound;
                                                    data.sounds.Play();
                                                }
                                            }

                                            //Call network
                                            pb.photonView.RPC("MeleeChargeNetwork", RpcTarget.Others, 0, 1);
                                            //Play third person reload anim
                                            pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 0);
                                        }
                                        else
                                        {
                                            //Increase progress
                                            if (data.chargingProgress < 1f)
                                            {
                                                data.chargingProgress += Time.deltaTime / secondaryAttackSettings.chargeChargeTime;
                                            }
                                        }

                                        //Set bool
                                        data.chargingSecondary = true;
                                        data.isCharging = true;
                                    }
                                }
                                else
                                {
                                    if (data.isCharging)
                                    {
                                        //RELEASE...
                                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.MeleeExecuteSecondaryCharge(this, data, pb));
                                        //Reset bools
                                        data.isCharging = false;
                                        data.chargingSecondary = false;
                                        //data.chargingProgress = 0f;
                                    }
                                }
                            }
                        }
                    }

                    if (!pb.isBot)
                    {
                        pb.main.hud.DisplayAmmo(0, 0, false);
                    }
                }
            }

            public override void CalculateWeaponUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                    if (!pb.isBot)
                    {
                        //Reset pos & rot of the renderer
                        data.meleeRenderer.transform.localPosition = Vector3.zero;
                        data.meleeRenderer.transform.localRotation = Quaternion.identity;
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                        //Play animation
                        if (data.meleeRenderer.anim)
                        {
                            //Enable anim
                            data.meleeRenderer.anim.enabled = true;
                            data.meleeRenderer.anim.Play("Draw", 0, 0f);
                        }
    
                        //Play sound if it is assigned
                        if (drawSound) data.sounds.PlayOneShot(drawSound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.meleeRenderer.visible = true;
                        }
                        else
                        {
                            data.meleeRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon and hide
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void DrawWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpMeleeRenderer.visible = true;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                    if (wasFallDamageApplied)
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                    }
                    else
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                    }
                }
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    //Get runtime data
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                    //Activate or deactivate based on bool
                    if (perspective == Kit_GameInformation.Perspective.ThirdPerson)
                    {
                        if (data.meleeRenderer)
                        {
                            data.meleeRenderer.visible = false;
                        }
                        if (data.tpMeleeRenderer)
                        {
                            data.tpMeleeRenderer.visible = true;
                            data.tpMeleeRenderer.shadowsOnly = false;
                        }
                    }
                    else
                    {
                        if (data.meleeRenderer)
                        {
                            data.meleeRenderer.visible = true;
                        }
                        if (data.tpMeleeRenderer)
                        {
                            data.tpMeleeRenderer.visible = true;
                            data.tpMeleeRenderer.shadowsOnly = true;
                        }
                    }
                }
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData)
            {
                return new WeaponIKValues();
            }

            public override WeaponStats GetStats()
            {
                return new WeaponStats();
            }

            public override bool SupportsStats()
            {
                return false;
            }

            public override bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override void NetworkMeleeChargeRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int id, int slot)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;
                    if (slot == 0)
                    {
                        if (id == 0)
                        {
                            if (primaryAttackSettings.chargeChargeSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.chargeChargeSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 1)
                        {
                            if (primaryAttackSettings.chargeWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.chargeWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 2)
                        {
                            if (primaryAttackSettings.chargeMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.chargeMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 3)
                        {
                            if (primaryAttackSettings.chargeHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.chargeHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 4)
                        {
                            if (primaryAttackSettings.chargeHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.chargeHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                    else if (slot == 1)
                    {
                        if (id == 0)
                        {
                            if (secondaryAttackSettings.chargeChargeSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.chargeChargeSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 1)
                        {
                            if (secondaryAttackSettings.chargeWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.chargeWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 2)
                        {
                            if (secondaryAttackSettings.chargeMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.chargeMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 3)
                        {
                            if (secondaryAttackSettings.chargeHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.chargeHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 4)
                        {
                            if (secondaryAttackSettings.chargeHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.chargeHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                    else if (slot == 2)
                    {
                        if (id == 0)
                        {
                            if (quickAttackSettings.chargeChargeSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.chargeChargeSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 1)
                        {
                            if (quickAttackSettings.chargeWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.chargeWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 2)
                        {
                            if (quickAttackSettings.chargeMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.chargeMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 3)
                        {
                            if (quickAttackSettings.chargeHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.chargeHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (id == 4)
                        {
                            if (quickAttackSettings.chargeHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.chargeHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                }
            }

            public override void NetworkMeleeStabRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state, int slot)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;
                    if (slot == 0)
                    {
                        if (state == 0)
                        {
                            if (primaryAttackSettings.stabWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.stabWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 1)
                        {
                            if (primaryAttackSettings.stabMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.stabMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 2)
                        {
                            if (primaryAttackSettings.stabHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.stabHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 3)
                        {
                            if (primaryAttackSettings.stabHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = primaryAttackSettings.stabHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                    else if (slot == 1)
                    {
                        if (state == 0)
                        {
                            if (secondaryAttackSettings.stabWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.stabWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 1)
                        {
                            if (secondaryAttackSettings.stabMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.stabMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 2)
                        {
                            if (secondaryAttackSettings.stabHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.stabHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 3)
                        {
                            if (secondaryAttackSettings.stabHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = secondaryAttackSettings.stabHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                    else if (slot == 2)
                    {
                        if (state == 0)
                        {
                            if (quickAttackSettings.stabWindupSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.stabWindupSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 1)
                        {
                            if (quickAttackSettings.stabMissSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.stabMissSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 2)
                        {
                            if (quickAttackSettings.stabHitSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.stabHitSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                        else if (state == 3)
                        {
                            if (quickAttackSettings.stabHitObjectSound)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = quickAttackSettings.stabHitObjectSound;
                                pb.thirdPersonPlayerModel.soundOther.Play();
                            }
                        }
                    }
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                }
            }


            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData)
            {
                if (stream.IsWriting)
                {

                }
                else
                {

                }
            }

            public override void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                    if (!pb.isBot)
                    {
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Play animation
                        if (data.meleeRenderer.anim)
                        {
                            //Enable anim
                            data.meleeRenderer.anim.enabled = true;
                            data.meleeRenderer.anim.Play("Putaway", 0, 0f);
                        }
 
                        //Play sound if it is assigned
                        if (putawaySound) data.sounds.PlayOneShot(putawaySound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.meleeRenderer.visible = true;
                        }
                        else
                        {
                            //Hide
                            data.meleeRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        data.tpMeleeRenderer.visible = true;
                        data.tpMeleeRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    if (!pb.isBot)
                    {
                        //Hide weapon
                        data.meleeRenderer.visible = false;
                        //Disable anim
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.enabled = false;
                        }
       
                        //Reset pos & rot of the renderer
                        data.meleeRenderer.transform.localPosition = Vector3.zero;
                        data.meleeRenderer.transform.localRotation = Quaternion.identity;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                    }
                    //Hide tp weapon
                    data.tpMeleeRenderer.visible = false;
                    //Make sure it is not ready
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHideOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;
                    //Hide tp weapon
                    data.tpMeleeRenderer.visible = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(MeleeControllerOthersRuntimeData))
                {
                    MeleeControllerOthersRuntimeData data = runtimeData as MeleeControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpMeleeRenderer.visible = true;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments)
            {
                MeleeControllerRuntimeData data = new MeleeControllerRuntimeData();

                if (!pb.isBot)
                {
                    //Setup root for this weapon
                    GameObject root = new GameObject("Weapon root");
                    root.transform.parent = pb.weaponsGo; //Set root
                    root.transform.localPosition = Vector3.zero; //Reset position
                    root.transform.localRotation = Quaternion.identity; //Reset rotation
                    root.transform.localScale = Vector3.one; //Reset scale

                    //Setup generic animations
                    GameObject genericAnimations = Instantiate(genericGunAnimatorControllerPrefab);
                    genericAnimations.transform.parent = root.transform;
                    genericAnimations.transform.localPosition = Vector3.zero; //Reset position
                    genericAnimations.transform.localRotation = Quaternion.identity; //Reset rotation
                    genericAnimations.transform.localScale = Vector3.one; //Reset scale

                    //Get animator
                    Animator anim = genericAnimations.GetComponent<Animator>(); ;
                    anim.Play("Idle");
                    data.genericAnimator = anim;

                    //Delay transform
                    GameObject delayTrans = new GameObject("Weapon delay");
                    delayTrans.transform.parent = genericAnimations.transform; //Set root
                    delayTrans.transform.localPosition = Vector3.zero; //Reset position
                    delayTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    delayTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponDelayTransform = delayTrans.transform;

                    //Delay transform
                    GameObject fallTrans = new GameObject("Weapon fall");
                    fallTrans.transform.parent = delayTrans.transform; //Set root
                    fallTrans.transform.localPosition = Vector3.zero; //Reset position
                    fallTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    fallTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponFallTransform = fallTrans.transform;

                    //Get Fire Audio (Needs to be consistent)
                    if (pb.weaponsGo.GetComponent<AudioSource>()) data.sounds = pb.weaponsGo.GetComponent<AudioSource>();
                    else data.sounds = pb.weaponsGo.gameObject.AddComponent<AudioSource>();

                    //Setup the first person prefab
                    GameObject fpRuntime = Instantiate(firstPersonPrefab, fallTrans.transform, false);
                    fpRuntime.transform.localScale = Vector3.one; //Reset scale

                    //Setup renderer
                    data.meleeRenderer = fpRuntime.GetComponent<Kit_MeleeRenderer>();
                    data.meleeRenderer.visible = false;

                    //Add to the list
                    data.instantiatedObjects.Add(root);
                }

                //Return runtime data
                return data;
            }

            public override void SetupThirdPerson(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, object runtimeData, int[] attachments)
            {
                MeleeControllerRuntimeData data = runtimeData as MeleeControllerRuntimeData;

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                //Setup renderer
                data.tpMeleeRenderer = tpRuntime.GetComponent<Kit_ThirdPersonMeleeRenderer>();
                data.tpMeleeRenderer.visible = false;
                if (!pb.isBot)
                {
                    //Make it shadows only
                    data.tpMeleeRenderer.shadowsOnly = true;
                }

                //Add to the list
                data.instantiatedObjects.Add(tpRuntime);
            }

            public override object SetupThirdPersonOthers(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, int[] attachments)
            {
                //Create runtime data (for replication)
                MeleeControllerOthersRuntimeData data = new MeleeControllerOthersRuntimeData();

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                data.instantiatedObjects.Add(tpRuntime);

                //Setup renderer
                data.tpMeleeRenderer = tpRuntime.GetComponent<Kit_ThirdPersonMeleeRenderer>();
                data.tpMeleeRenderer.visible = false;

                //Return the data
                return data;
            }

            public override void SetupValues(int id)
            {
                //Get our ID
                gameGunID = id;
            }

            public override float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override int WeaponState(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 0;
            }

            public override int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 2;
            }
        }
    }
}