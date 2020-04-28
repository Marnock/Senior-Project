using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class WeaponControllerRuntimeData
        {
            public Kit_WeaponRenderer weaponRenderer;
            public Kit_ThirdPersonWeaponRenderer tpWeaponRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); //Objects that were instantiated by this weapon. When it is replaced, they have to be cleaned up first.

            public bool isSelectedAndReady = false;

            public Animator genericAnimator;

            #region Stats
  
            public bool isFiring;
             public float lastFire;
 
            public int bulletsLeft;
  
            public int bulletsLeftToReload;

            public float lastRun;
            #endregion

            #region Reload

            public bool reloadInProgress;

            public float reloadNextEnd; 
    
            public int reloadPhase;
            #endregion

            #region Procedural Reload
   
            public bool cancelProceduralReload = false;
            #endregion

            #region Aiming
    
            public bool isAiming;

  
            public bool lastIsAiming;

            public float aimingProgress;

            public bool isAimedIn
            {
                get
                {
                    return Mathf.Approximately(aimingProgress, 1f);
                }
            }

            public bool sniperWeaponHidden;

            public Transform aimingTransform;
            #endregion

            #region Weapon Delay
            public Transform weaponDelayTransform;
  
            public Vector3 weaponDelayCur;
  
            public float weaponDelayCurrentX;

            public float weaponDelayCurrentY;
            #endregion

            #region Weapon Fall
            public Transform weaponFallTransform;
            #endregion

            #region Shell Ejection
    
            public bool shellEjectEnabled = false;

            public float shellEjectNext;
            #endregion

            #region  Bolt Action
      
            public int boltActionState;
    
            public float boltActionTime;
            #endregion

            public int lastWeaponAnimationID;

            #region Spray Pattern
 
            public float sprayPatternState = 0f;
            #endregion

            #region Run Animation
    
            public bool startedRunAnimation;
            #endregion

            #region Sound
    
            public AudioSource soundFire;
            public AudioSource soundReload;

            public AudioSource soundOther;
            #endregion

            #region Input
            public bool lastLmb;
            public bool lastRmb;
            public bool lastReload;
            #endregion
        }

        public class WeaponControllerOthersRuntimeData
        {
            public Kit_ThirdPersonWeaponRenderer tpWeaponRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); //Objects that were instantiated by this weapon. When it is replaced, they have to be cleaned up first.

            #region Stats
 
            public bool isFiring;
   
            public float lastFire;
            #endregion

            #region Shell Ejection
 
            public bool shellEjectEnabled = false;
        
            public float shellEjectNext;
            #endregion

            #region  Bolt Action
      
            public int boltActionState;
               public float boltActionTime;
            #endregion
        }

        #region enums
        public enum FireMode { Semi, Auto, Burst, BoltAction };
        public enum FireTypeMode { Simple, Pellets }
        public enum BulletMode { Raycast, Physical }
        public enum ReloadMode { Simple, FullEmpty, Chambered, Procedural, ProceduralChambered }
        public enum SpreadMode { Simple, SprayPattern }
        #endregion

        ///<summary>
        ///Heart of the weapons system. Contains methods to set up variety of weapons
        ///</summary>
        public class Kit_ModernWeaponScript : Kit_WeaponBase
        {
            #region Settings
            [Header("Settings")]
            public FireMode fireMode = FireMode.Semi;
 
            public int RPM = 600;
            private float fireRate = 0.1f;
   
            public int bulletsPerMag = 30;
      
            public int bulletsToReloadAtStart = 60;
     
            public float baseDamage = 30f;
     
            public float range = 500f;
      
            public AnimationCurve damageDropoff = AnimationCurve.Linear(0, 1, 500f, 0.8f);
     
            public FireTypeMode fireTypeMode = FireTypeMode.Simple;
            public int burstBulletsPerShot = 3;
       
            public float burstTimeBetweenShots = 0.1f;
      
            public int amountOfPellets = 12;
     
            public float speedMultiplierBase = 1f;
     
            public float ragdollForce = 500f;
            #endregion

            #region Animation Settings
       
            public bool setEmptyBool;
            #endregion

            #region Crosshair
            [Header("Crosshair")]
     
            public bool crosshairEnabled = true;
 
            public float crosshairSizeMultiplier = 1f;
            #endregion

            #region Bullets
            [Header("Bullets")]
 
            public BulletMode bulletsMode = BulletMode.Raycast;
      
            public bool bulletsPenetrationEnabled = true;
  
            public int bulletsPenetrationForce = 3;
    
            public GameObject bulletPrefab;

            public float bulletSpeed = 200;
   
            public float bulletGravityMultiplier = 1f;
    
            public float bulletLifeTime = 10f;

            public bool bulletStaysAliveAfterDeath = false;
    
            public float bulletStaysAliveAfterDeathTime = 10f;
            #endregion

            #region Bullet Spread
            [Header("Bullet Spread")]
     
            public SpreadMode bulletSpreadMode = SpreadMode.Simple;
   
            public float bulletSpreadHipBase = 0.1f;
    
            public float bulletSpreadHipVelocityAdd = 0.1f;
  
            public float bulletSpreadHipVelocityReference = 6f;


            public float bulletSpreadAimBase = 0.01f;

            public float bulletSpreadAimVelocityAdd = 0.02f;
   
            public float bulletSpreadAimVelocityReference = 6f;

 
            public Vector3[] bulletSpreadSprayPattern = new Vector3[30];
   
            public float bulletSpreadSprayPatternRecoverySpeed = 1f;
            #endregion

            #region Recoil
            [Header("Recoil")]
      
            public Vector2 recoilPerShotMin = new Vector2 { x = -0.1f, y = 0.5f };
   
            public Vector2 recoilPerShotMax = new Vector2 { x = 0.1f, y = 1f };
 
            public float recoilApplyTime = 0.1f;
  
            public float recoilReturnSpeed = 6f;
            #endregion

            #region Reload
            [Header("Reload")]
            public ReloadMode reloadMode = ReloadMode.Chambered;
            public bool reloadProceduralAddBulletDuringStartEmpty;
 
            public float reloadTimeOne, reloadTimeTwo; 
   
            public float reloadEmptyTimeOne, reloadEmptyTimeTwo;
               public float reloadProceduralStartTime;

            public float reloadProceduralEmptyStartTime;

            public float reloadProceduralEmptyInsertStartTimeOne;
    
            public float reloadProceduralEmptyInsertStartTimeTwo;
       
            public float reloadProceduralInsertTimeOne;
  
            public float reloadProceduralInsertTimeTwo;
 
            public float reloadProceduralEndTime;

     
            public AudioClip reloadSound;
       
            public AudioClip reloadSoundEmpty;
 
            public float reloadSoundThirdPersonMaxRange = 20;
     
            public AnimationCurve reloadSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
    
            public AudioClip reloadProceduralStartSound;
     
            public AudioClip reloadProceduralStartEmptySound;
     
            public AudioClip reloadProceduralInsertSound;
     
            public AudioClip reloadProceduralEndSound;
            #endregion

            #region Aiming
            [Header("Aiming")]
      
            public float aimInTime = 0.5f;
        
            public float aimOutTime = 0.5f;
           
            public float aimSpeedMultiplier = 0.8f;
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
        
            public bool weaponTiltEnabledWhileAiming;
 
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

            #region Shell Ejection
            [Header("Shell Ejection")]
   
            public GameObject shellEjectionPrefab;
   
            public float shellEjectionTime = 0f;
   
            public Vector3 shellEjectionMinForce;
  
            public Vector3 shellEjectionMaxForce;
       
            public Vector3 shellEjectionMinTorque;
    
            public Vector3 shellEjectionMaxTorque;
            #endregion

            #region Running
            [Header("Running")]
     
            public float timeCannotFireAfterRun = 0.3f;
            #endregion

            #region Sounds
            [Header("Sounds")]
   
            public AudioClip drawSound;
            public AudioClip putawaySound;

            public AudioClip fireSound;
  
            public AudioClip fireSoundThirdPerson;
     
            public float boltActionDelayNormal;
    
            public AudioClip boltActionSoundNormal;
               public float boltActionTimeNormal = 0.3f;
  
            public float boltActionDelayLast;
  
            public AudioClip boltActionSoundLast;

            public float boltActionTimeLast = 0.3f;
 
            public float boltActionSoundThirdPersonMaxRange = 20f;
   
            public AnimationCurve boltActionSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
            public float fireSoundThirdPersonMaxRange = 300f;
      
            public AnimationCurve fireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);

            public AudioClip dryFireSound;
            #endregion

            #region Walk Animations
            [Header("Walking")]
            public bool enableWalkAnimationWhileAiming;
            #endregion

            #region Generic Animations
            [Header("Generic Animations")]
      
            public GameObject genericGunAnimatorControllerPrefab;

            public bool useGenericWalkAnim = true;

            public bool useGenericRunAnim = true;
            #endregion

            [HideInInspector]
            public int gameGunID;


            #region Overriden functions
            public override WeaponDisplayData GetWeaponDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                WeaponDisplayData wdd = new WeaponDisplayData();
                wdd.sprite = weaponHudPicture;
                wdd.name = weaponName;
                return wdd;
            }

            public override void SetupValues(int id)
            {
                //Get our ID
                gameGunID = id;
                //Set RPM
                fireRate = 60f / RPM;
            }

            public override object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments)
            {
                WeaponControllerRuntimeData data = new WeaponControllerRuntimeData();
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

                    //Setup aiming transform
                    GameObject aimTrans = new GameObject("Weapon aiming");
                    aimTrans.transform.parent = genericAnimations.transform; //Set root
                    aimTrans.transform.localPosition = Vector3.zero; //Reset position
                    aimTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    aimTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.aimingTransform = aimTrans.transform;

                    //Delay transform
                    GameObject delayTrans = new GameObject("Weapon delay");
                    delayTrans.transform.parent = aimTrans.transform; //Set root
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
                    if (pb.weaponsGo.GetComponent<AudioSource>()) data.soundFire = pb.weaponsGo.GetComponent<AudioSource>();
                    else data.soundFire = pb.weaponsGo.gameObject.AddComponent<AudioSource>();

                    //Setup reload sound
                    GameObject soundReload = new GameObject("SoundReload"); //Create
                    soundReload.transform.parent = root.transform;
                    soundReload.transform.localPosition = Vector3.zero; //Reset position
                    soundReload.transform.localRotation = Quaternion.identity; //Reset rotation
                    soundReload.transform.localScale = Vector3.one; //Reset scale
                                                                    //Add audio source
                    data.soundReload = soundReload.AddComponent<AudioSource>();

                    //Setup other sound
                    GameObject soundOther = new GameObject("SoundOther"); //Create
                    soundOther.transform.parent = root.transform;
                    soundOther.transform.localPosition = Vector3.zero; //Reset position
                    soundOther.transform.localRotation = Quaternion.identity; //Reset rotation
                    soundOther.transform.localScale = Vector3.one; //Reset scale
                                                                   //Add audio source
                    data.soundOther = soundOther.AddComponent<AudioSource>();

                    //Setup the first person prefab
                    GameObject fpRuntime = Instantiate(firstPersonPrefab, fallTrans.transform, false);
                    fpRuntime.transform.localScale = Vector3.one; //Reset scale

                    //Setup renderer
                    data.weaponRenderer = fpRuntime.GetComponent<Kit_WeaponRenderer>();
                    //Set Attachments
                    data.weaponRenderer.SetAttachments(attachments, this, pb);
                    data.weaponRenderer.visible = false;

                    //Add to the list
                    data.instantiatedObjects.Add(root);
                }

                //Setup start values
                data.bulletsLeft = bulletsPerMag;
                data.bulletsLeftToReload = bulletsToReloadAtStart;

                //Return runtime data
                return data;
            }

            public override void SetupThirdPerson(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, object runtimeData, int[] attachments)
            {
                WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                //Setup renderer
                data.tpWeaponRenderer = tpRuntime.GetComponent<Kit_ThirdPersonWeaponRenderer>();
                data.tpWeaponRenderer.visible = false;
                if (!pb.isBot)
                {
                    //Make it shadows only
                    data.tpWeaponRenderer.shadowsOnly = true;
                }
                //Setup attachments
                data.tpWeaponRenderer.SetAttachments(attachments, ws, pb, data);

                //Add to the list
                data.instantiatedObjects.Add(tpRuntime);
            }

            public override object SetupThirdPersonOthers(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, int[] attachments)
            {
                //Create runtime data (for replication)
                WeaponControllerOthersRuntimeData data = new WeaponControllerOthersRuntimeData();

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                data.instantiatedObjects.Add(tpRuntime);

                //Setup renderer
                data.tpWeaponRenderer = tpRuntime.GetComponent<Kit_ThirdPersonWeaponRenderer>();
                data.tpWeaponRenderer.visible = false;
                //Setup attachments
                data.tpWeaponRenderer.SetAttachments(attachments, ws, pb, null);

                //Return the data
                return data;
            }

            public override void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                    //Set this weapon to selected and ready (for other things)
                    data.isSelectedAndReady = true;

                    //If we are not reloading
                    if (!data.reloadInProgress)
                    {
                        //Input
                        if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                        {
                            //Check if movement and time allows firing
                            if (pb.movement.CanFire(pb) && Time.time - timeCannotFireAfterRun > data.lastRun)
                            {
                                //Fire modes
                                if (fireMode == FireMode.Semi)
                                {
                                    //We cannot fire full auto, set value
                                    data.isFiring = false;
                                    //Check for input
                                    if (data.lastLmb != pb.input.lmb)
                                    {
                                        data.lastLmb = pb.input.lmb;
                                        if (pb.input.lmb)
                                        {
                                            //Compare with fire rate
                                            if (Time.time >= data.lastFire + fireRate)
                                            {
                                                if (data.bulletsLeft > 0)
                                                {
                                                    FireOneShot(pb, data);
                                                }
                                                else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                {
                                                    DryFire(pb, data);
                                                }
                                            }
                                        }
                                    }
                                }
                                else //Fire modes
                                if (fireMode == FireMode.BoltAction)
                                {
                                    //We cannot fire full auto, set value
                                    data.isFiring = false;
                                    //Check for input
                                    if (data.lastLmb != pb.input.lmb)
                                    {
                                        data.lastLmb = pb.input.lmb;
                                        if (pb.input.lmb)
                                        {
                                            //In bolt action, lastFire already includes bolt time
                                            if (Time.time >= data.lastFire)
                                            {
                                                if (data.bulletsLeft > 0)
                                                {
                                                    FireOneShot(pb, data);
                                                }
                                                else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                {
                                                    DryFire(pb, data);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (fireMode == FireMode.Auto)
                                {
                                    //Check for input
                                    if (pb.input.lmb)
                                    {
                                        //Compare with fire rate
                                        if (Time.time >= data.lastFire + fireRate)
                                        {
                                            //Compare bullets left
                                            if (data.bulletsLeft > 0)
                                            {
                                                FireOneShot(pb, data);
                                                //We are firing full auto, set value
                                                data.isFiring = true;
                                            }
                                            else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                            {
                                                DryFire(pb, data);
                                                //We cannot fire, set value
                                                data.isFiring = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //We cannot fire, set value
                                        data.isFiring = false;
                                    }
                                }
                                else if (fireMode == FireMode.Burst)
                                {
                                    //We cannot fire full auto, set value
                                    data.isFiring = false;
                                    //Check for input
                                    if (data.lastLmb != pb.input.lmb)
                                    {
                                        data.lastLmb = pb.input.lmb;
                                        if (pb.input.lmb)
                                        {
                                            //Compare with fire rate
                                            if (Time.time >= data.lastFire + fireRate)
                                            {
                                                if (data.bulletsLeft > 0)
                                                {
                                                    FireBurst(pb, data);
                                                }
                                                else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                {
                                                    DryFire(pb, data);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //We cannot fire, set value
                                data.isFiring = false;
                            }

                            //Aiming input
                            //Hold
                            if (!Kit_GameSettings.isAimingToggle || pb.isBot)
                            {
                                if (pb.movement.CanFire(pb))
                                {
                                    //Check for input
                                    if (pb.input.rmb)
                                    {
                                        //Check if we can aim
                                        if (data.isSelectedAndReady && !data.reloadInProgress)
                                        {
                                            data.isAiming = true;
                                        }
                                        //Else we cannot aim, set to not aiming
                                        else
                                        {
                                            data.isAiming = false;
                                        }
                                    }
                                    //Else we cannot aim, set to not aiming
                                    else
                                    {
                                        data.isAiming = false;
                                    }
                                }
                                //Else we cannot aim, set to not aiming
                                else
                                {
                                    data.isAiming = false;
                                }
                            }
                            //Toggle
                            else
                            {
                                if (pb.movement.CanFire(pb))
                                {
                                    //Check if we can aim
                                    if (data.isSelectedAndReady && !data.reloadInProgress)
                                    {
                                        //Check for input
                                        if (data.lastRmb != pb.input.rmb)
                                        {
                                            data.lastRmb = pb.input.rmb;
                                            if (pb.input.rmb)
                                            {
                                                //Invert toggle
                                                if (data.isAiming) data.isAiming = false;
                                                else data.isAiming = true;
                                            }
                                        }
                                    }
                                    //Else we cannot aim, set to not aiming
                                    else
                                    {
                                        data.isAiming = false;
                                    }
                                }
                                //Else we cannot aim, set to not aiming
                                else
                                {
                                    data.isAiming = false;
                                }
                            }

                            //Check for input
                            if (data.lastReload != pb.input.reload || pb.main.gameInformation.enableAutoReload && data.bulletsLeft == 0 && data.bulletsLeftToReload > 0 && Time.time > data.lastFire + 0.5f)
                            {
                                data.lastReload = pb.input.reload;
                                if (pb.input.reload || pb.main.gameInformation.enableAutoReload && data.bulletsLeft == 0 && data.bulletsLeftToReload > 0)
                                {
                                    //Check if we can start reload
                                    if (fireMode != FireMode.BoltAction || fireMode == FireMode.BoltAction && Time.time > data.lastFire)
                                    {
                                        //Check if we have spare ammo
                                        if (data.bulletsLeftToReload > 0)
                                        {
                                            //Check if the weapon isn't already full (Chambered means we can have one in the chamber)
                                            if ((reloadMode == ReloadMode.Chambered || reloadMode == ReloadMode.ProceduralChambered) && data.bulletsLeft != bulletsPerMag + 1 || reloadMode != ReloadMode.Chambered && reloadMode != ReloadMode.ProceduralChambered && data.bulletsLeft != bulletsPerMag)
                                            {
                                                data.reloadInProgress = true;
                                                //Reset run animation
                                                data.startedRunAnimation = false;
                                                //Set reload time
                                                if (reloadMode == ReloadMode.Simple)
                                                {
                                                    //Simple reload, set time
                                                    data.reloadNextEnd = Time.time + reloadTimeOne;
                                                    if (!pb.isBot)
                                                    {
                                                        if (data.weaponRenderer.anim)
                                                        {
                                                            //Play animation
                                                            data.weaponRenderer.anim.Play("Reload Full", 0, 0f);
                                                        }

                                                        //Play sound
                                                        data.soundReload.PlayOneShot(reloadSound);
                                                    }
                                                    else
                                                    {
                                                        //Set clip
                                                        pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                                        //Set distance
                                                        pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                        //Set rolloff
                                                        pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                        //Play
                                                        pb.thirdPersonPlayerModel.soundReload.Play();
                                                    }
                                                    //Call network
                                                    pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, false);
                                                    //Play third person reload anim
                                                    pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType);
                                                }
                                                else if (reloadMode == ReloadMode.FullEmpty || reloadMode == ReloadMode.Chambered)
                                                {
                                                    //Set time for full
                                                    if (data.bulletsLeft > 0)
                                                    {
                                                        //Set time
                                                        data.reloadNextEnd = Time.time + reloadTimeOne;
                                                        if (!pb.isBot)
                                                        {
                                                            //Play animation
                                                            if (data.weaponRenderer.anim)
                                                            {
                                                                //Play animation
                                                                data.weaponRenderer.anim.Play("Reload Full", 0, 0f);
                                                            }

                                                            //Play sound
                                                            data.soundReload.PlayOneShot(reloadSound);
                                                        }
                                                        else
                                                        {
                                                            //Set clip
                                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                                            //Set distance
                                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                            //Set rolloff
                                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                            //Play
                                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                                        }
                                                        //Call network
                                                        pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, false);
                                                        //Play third person reload anim
                                                        pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType);
                                                    }
                                                    //Empty
                                                    else
                                                    {
                                                        //Set time
                                                        data.reloadNextEnd = Time.time + reloadEmptyTimeOne;
                                                        if (!pb.isBot)
                                                        {
                                                            //Play animation
                                                            if (data.weaponRenderer.anim)
                                                            {
                                                                //Play animation
                                                                data.weaponRenderer.anim.Play("Reload Empty", 0, 0f);
                                                            }
  
                                                            //Play sound
                                                            data.soundReload.PlayOneShot(reloadSoundEmpty);
                                                        }
                                                        else
                                                        {
                                                            //Play reload sound
                                                            //Set clip
                                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadSoundEmpty;
                                                            //Set distance
                                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                            //Set rolloff
                                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                            //Play
                                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                                        }
                                                        //Call network
                                                        pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, true);
                                                        //Play third person reload anim
                                                        pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType);
                                                    }
                                                }
                                                else if (reloadMode == ReloadMode.Procedural || reloadMode == ReloadMode.ProceduralChambered)
                                                {
                                                    if (data.bulletsLeft > 0)
                                                    {
                                                        //Set phase
                                                        data.reloadPhase = 0;
                                                        //Set time
                                                        data.reloadNextEnd = Time.time + reloadProceduralStartTime;
                                                        if (!pb.isBot)
                                                        {
                                                            //Play animation
                                                            if (data.weaponRenderer.anim)
                                                            {
                                                                //Play animation
                                                                data.weaponRenderer.anim.Play("Reload Procedural Start", 0, 0f);
                                                            }
                                                            //Play sound
                                                            data.soundReload.PlayOneShot(reloadProceduralStartSound);
                                                        }
                                                        else
                                                        {
                                                            //Play reload sound
                                                            //Set clip
                                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartSound;
                                                            //Set distance
                                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                            //Set rolloff
                                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                            //Play
                                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                                        }
                                                        //Set cancel state
                                                        data.cancelProceduralReload = false;
                                                        //Call network
                                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 0);
                                                        //Set cancel state
                                                        data.cancelProceduralReload = false;
                                                    }
                                                    else
                                                    {
                                                        //Set phase
                                                        data.reloadPhase = 0;
                                                        //Set time
                                                        if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                                        {
                                                            data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeOne;
                                                        }
                                                        else
                                                        {
                                                            data.reloadNextEnd = Time.time + reloadProceduralEmptyStartTime;
                                                        }
                                                        if (!pb.isBot)
                                                        {
                                                            if (data.weaponRenderer.anim)
                                                            {
                                                                //Play animation
                                                                data.weaponRenderer.anim.Play("Reload Procedural Start Empty", 0, 0f);
                                                            }

                                                            //Play sound
                                                            data.soundReload.PlayOneShot(reloadProceduralStartEmptySound);
                                                        }
                                                        else
                                                        {
                                                            //Play reload sound
                                                            //Set clip
                                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartEmptySound;
                                                            //Set distance
                                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                            //Set rolloff
                                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                            //Play
                                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                                        }
                                                        //Set cancel state
                                                        data.cancelProceduralReload = false;
                                                        //Call network
                                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 0);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (data != null && data.weaponRenderer && data.weaponRenderer.cachedInteractionAttachments != null)
                            {
                                //Attachment interaction, only when we can control.
                                for (int i = 0; i < data.weaponRenderer.cachedInteractionAttachments.Length; i++)
                                {
                                    data.weaponRenderer.cachedInteractionAttachments[i].Interaction(pb);
                                }
                            }
                        }
                        else
                        {
                            //So we don't fire anymore
                            data.isFiring = false;
                        }

                        if (setEmptyBool && !pb.isBot && data.weaponRenderer.anim)
                        {
                            //Set empty bool
                            data.weaponRenderer.anim.SetBool("empty", data.bulletsLeft == 0);
                        }
                    }
                    //Reloading
                    else
                    {
                        //When we are reloading, empty bool will be 'false'
                        if (setEmptyBool && !pb.isBot && data.weaponRenderer.anim)
                        {
                            //Set empty bool
                            data.weaponRenderer.anim.SetBool("empty", false);
                        }
                        //Reset run animation
                        data.startedRunAnimation = false;
                        //Make sure we are not aiming
                        data.isAiming = false;
                        //Make sure we are not firing either
                        data.isFiring = false;
                        //Check for cancelling
                        if (reloadMode == ReloadMode.Procedural || reloadMode == ReloadMode.ProceduralChambered)
                        {
                            if (data.lastLmb != pb.input.lmb)
                            {
                                data.lastLmb = pb.input.lmb;
                                if (pb.input.lmb)
                                {
                                    //User wants to cancel
                                    data.cancelProceduralReload = true;
                                }
                            }
                        }

                        //Check if a reload phase is over
                        if (Time.time >= data.reloadNextEnd)
                        {
                            //Simple reload
                            if (reloadMode == ReloadMode.Simple)
                            {
                                if (data.reloadPhase == 0)
                                {
                                    //Update bullets
                                    if (data.bulletsLeftToReload > bulletsPerMag)
                                    {
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload -= bulletsPerMag - data.bulletsLeft;
                                        data.bulletsLeft = bulletsPerMag;
                                    }
                                    else
                                    {
                                        int wepBullet = Mathf.Clamp(bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                        data.bulletsLeft = wepBullet;
                                    }
                                    //Proceed
                                    data.reloadPhase = 1;
                                    data.reloadNextEnd = Time.time + reloadTimeTwo;
                                }
                                else if (data.reloadPhase == 1)
                                {
                                    //Reload is over
                                    data.reloadInProgress = false;
                                    data.reloadPhase = 0;
                                }
                            }
                            //Reload with different anims / times for full / empty state
                            else if (reloadMode == ReloadMode.FullEmpty)
                            {
                                if (data.reloadPhase == 0)
                                {
                                    //Set time
                                    if (data.bulletsLeft > 0)
                                    {
                                        //Full reload
                                        data.reloadNextEnd = Time.time + reloadTimeTwo;
                                    }
                                    else
                                    {
                                        //Empty reload
                                        data.reloadNextEnd = Time.time + reloadEmptyTimeTwo;
                                    }

                                    //Update bullets
                                    if (data.bulletsLeftToReload > bulletsPerMag)
                                    {
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload -= bulletsPerMag - data.bulletsLeft;
                                        data.bulletsLeft = bulletsPerMag;
                                    }
                                    else
                                    {
                                        int wepBullet = Mathf.Clamp(bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                        data.bulletsLeft = wepBullet;
                                    }
                                    //Proceed
                                    data.reloadPhase = 1;
                                }
                                else if (data.reloadPhase == 1)
                                {
                                    //Reload is over
                                    data.reloadInProgress = false;
                                    data.reloadPhase = 0;
                                }
                            }
                            //Reload with different anims / times for full / empty state and a bullet in the chamber
                            else if (reloadMode == ReloadMode.Chambered)
                            {
                                if (data.reloadPhase == 0)
                                {
                                    //Set time
                                    if (data.bulletsLeft > 0)
                                    {
                                        //Full reload
                                        data.reloadNextEnd = Time.time + reloadTimeTwo;
                                    }
                                    else
                                    {
                                        //Empty reload
                                        data.reloadNextEnd = Time.time + reloadEmptyTimeTwo;
                                    }

                                    //Update bullets
                                    if (data.bulletsLeftToReload > bulletsPerMag)
                                    {
                                        if (data.bulletsLeft > 0)
                                        {
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload -= (bulletsPerMag + 1) - data.bulletsLeft;
                                            data.bulletsLeft = bulletsPerMag + 1;
                                        }
                                        else
                                        {
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload -= bulletsPerMag - data.bulletsLeft;
                                            data.bulletsLeft = bulletsPerMag;
                                        }
                                    }
                                    else
                                    {
                                        if (data.bulletsLeft > 0)
                                        {
                                            int wepBullet = Mathf.Clamp(bulletsPerMag + 1, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                            data.bulletsLeft = wepBullet;
                                        }
                                        else
                                        {
                                            int wepBullet = Mathf.Clamp(bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                            data.bulletsLeft = wepBullet;
                                        }
                                    }
                                    //Proceed
                                    data.reloadPhase = 1;
                                }
                                else if (data.reloadPhase == 1)
                                {
                                    //Reload is over
                                    data.reloadInProgress = false;
                                    data.reloadPhase = 0;
                                }
                            }
                            else if (reloadMode == ReloadMode.Procedural)
                            {
                                if (data.reloadPhase == 0)
                                {
                                    if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                    {
                                        //Set phase
                                        data.reloadPhase = 1;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeTwo;
                                        //Add bullet
                                        data.bulletsLeft++;
                                        //Substract
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload--;
                                        //Sounds and animation is already being played, so ignore that
                                    }
                                    else
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                }
                                else if (data.reloadPhase == 1)
                                {
                                    //Insert is over, check if we have more bullets to reload
                                    if (data.bulletsLeftToReload > 0 && !data.cancelProceduralReload)
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                    else
                                    {
                                        //End
                                        data.reloadPhase = 4;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural End", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                    }
                                }
                                else if (data.reloadPhase == 2)
                                {
                                    //End insert
                                    data.bulletsLeft++;
                                    //Substract
                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                        data.bulletsLeftToReload--;
                                    //Wait
                                    data.reloadPhase = 3;
                                    //Set time
                                    data.reloadNextEnd = Time.time + reloadProceduralInsertTimeTwo;
                                }
                                else if (data.reloadPhase == 3)
                                {
                                    //Insert is over, check if we can insert some more
                                    if (data.bulletsLeftToReload > 0 && data.bulletsLeft < bulletsPerMag && !data.cancelProceduralReload)
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                    else
                                    {
                                        //End
                                        data.reloadPhase = 4;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural End", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                    }
                                }
                                else if (data.reloadPhase == 4)
                                {
                                    //It is over, end reload
                                    data.reloadInProgress = false;
                                    data.reloadPhase = 0;
                                }
                            }
                            else if (reloadMode == ReloadMode.ProceduralChambered)
                            {
                                if (data.reloadPhase == 0)
                                {
                                    if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                    {
                                        //Set phase
                                        data.reloadPhase = 1;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeTwo;
                                        //Add bullet
                                        data.bulletsLeft++;
                                        //Substract
                                        if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                            data.bulletsLeftToReload--;
                                        //Sounds and animation is already being played, so ignore that
                                    }
                                    else
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }
 
                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                }
                                else if (data.reloadPhase == 1)
                                {
                                    //Insert is over, check if we have more bullets to reload
                                    if (data.bulletsLeftToReload > 0 && !data.cancelProceduralReload)
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }
  
                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                    else
                                    {
                                        //End
                                        data.reloadPhase = 4;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural End", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                    }
                                }
                                else if (data.reloadPhase == 2)
                                {
                                    //End insert
                                    data.bulletsLeft++;
                                    //Substract
                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                        data.bulletsLeftToReload--;
                                    //Wait
                                    data.reloadPhase = 3;
                                    //Set time
                                    data.reloadNextEnd = Time.time + reloadProceduralInsertTimeTwo;
                                }
                                else if (data.reloadPhase == 3)
                                {
                                    //Insert is over, check if we can insert some more
                                    if (data.bulletsLeftToReload > 0 && data.bulletsLeft < bulletsPerMag + 1 && !data.cancelProceduralReload)
                                    {
                                        //Go to Phase 2
                                        data.reloadPhase = 2;
                                        //Here we don't need to check if ammo is left because we already did that
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural Insert", 0, 0f);
                                            }
    
                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                    }
                                    else
                                    {
                                        //End
                                        data.reloadPhase = 4;
                                        //Set time
                                        data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                        if (!pb.isBot)
                                        {
                                            //Play animation
                                            if (data.weaponRenderer.anim)
                                            {
                                                //Play animation
                                                data.weaponRenderer.anim.Play("Reload Procedural End", 0, 0f);
                                            }

                                            //Play sound
                                            data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                        }
                                        else
                                        {
                                            //Play reload sound
                                            //Set clip
                                            pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                            //Set distance
                                            pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                            //Set rolloff
                                            pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                            //Play
                                            pb.thirdPersonPlayerModel.soundReload.Play();
                                        }
                                        //Call network
                                        pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                    }
                                }
                                else if (data.reloadPhase == 4)
                                {
                                    //It is over, end reload
                                    data.reloadInProgress = false;
                                    data.reloadPhase = 0;
                                }
                            }
                        }
                    }

                    //Update aiming
                    if (data.isAiming)
                    {
                        //Increase progress
                        if (aimInTime > 0)
                        {
                            if (data.aimingProgress < 1) data.aimingProgress += Time.deltaTime / aimInTime;
                            //Clamp
                            data.aimingProgress = Mathf.Clamp01(data.aimingProgress);
                        }
                        else
                        {
                            //Instant mode
                            data.aimingProgress = 1f;
                        }
                    }
                    else
                    {
                        //Decrease progress
                        if (aimOutTime > 0)
                        {
                            if (data.aimingProgress > 0) data.aimingProgress -= Time.deltaTime / aimOutTime;
                            //Clamp
                            data.aimingProgress = Mathf.Clamp01(data.aimingProgress);
                        }
                        else
                        {
                            //Instant mode
                            data.aimingProgress = 0;
                        }
                    }

                    if (!pb.isBot)
                    {
                        if (data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            if (data.isAimedIn)
                            {
          
                                //Snap fov
                                pb.main.mainCamera.fieldOfView = data.weaponRenderer.cachedAimingFov;
                                //Hide weapon
                                if (!data.sniperWeaponHidden)
                                {
                                    data.sniperWeaponHidden = true;
                                    data.weaponRenderer.visible = false;
                                }
                            }
                            else
                            {
                                //Snap fov
                                pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                                //Show weapon
                                if (data.sniperWeaponHidden)
                                {
                                    data.sniperWeaponHidden = false;
                                    data.weaponRenderer.visible = true;
                                }
                            }
                        }
                        else
                        {
                            //Smoothly fade FoV
                            pb.main.mainCamera.fieldOfView = Mathf.Lerp(Kit_GameSettings.baseFov, data.weaponRenderer.cachedAimingFov, data.aimingProgress);
                        }

                        //Update position and rotation
                        data.aimingTransform.localPosition = Vector3.Lerp(Vector3.zero, data.weaponRenderer.cachedAimingPos, data.aimingProgress);
                        data.aimingTransform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(data.weaponRenderer.cachedAimingRot), data.aimingProgress);
                    }

                    if (bulletSpreadMode == SpreadMode.SprayPattern && !data.isFiring)
                    {
                        //Spray pattern recovery
                        if (data.sprayPatternState > 0f)
                        {
                            data.sprayPatternState -= Time.deltaTime * bulletSpreadSprayPatternRecoverySpeed;
                        }
                        else
                        {
                            data.sprayPatternState = 0f;
                        }
                    }

                    //Shell ejection
                    if (data.shellEjectEnabled && !pb.isBot) //Check if shell ejection is enabled
                    {
                        //Check if enough time has passed
                        if (Time.time >= data.shellEjectNext || shellEjectionTime <= 0)
                        {
                            //Set ejection bool
                            data.shellEjectEnabled = false;
                            //Eject shell
                            EjectShell(pb, data);
                        }
                    }

                    //Check for bolt action
                    if (data.boltActionState == 1 || data.boltActionState == 2)
                    {
                        //Check if the time has passed
                        if (Time.time > data.boltActionTime)
                        {
                            if (!pb.isBot)
                            {
                                //Set values
                                data.soundOther.loop = false;


                                //Set correct clip
                                if (data.boltActionTime == 1)
                                {
                                    data.soundOther.clip = boltActionSoundNormal;
                                }
                                else
                                {
                                    data.soundOther.clip = boltActionSoundLast;
                                }

                                //Play
                                data.soundOther.Play();
                            }

                            //Set back to 0
                            data.boltActionState = 0;
                        }
                    }

                    if (!pb.isBot)
                    {
                        //Update HUD
                        pb.main.hud.DisplayAmmo(data.bulletsLeft, data.bulletsLeftToReload);
                        pb.main.hud.DisplayCrosshair(GetCrosshairSize(pb, data));
                    }
                }
            }

            public override void CalculateWeaponUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;

                    //Full auto fire replica
                    if (data.isFiring)
                    {
                        if (fireMode == FireMode.Auto)
                        {
                            //Compare with fire rate
                            if (Time.time >= data.lastFire + fireRate)
                            {
                                FireOneShotOthers(pb, data);
                            }
                        }
                    }

                    //Check for bolt action
                    if (data.boltActionState == 1 || data.boltActionState == 2)
                    {
                        //Check if the time has passed
                        if (Time.time > data.boltActionTime)
                        {
                            //Set values
                            pb.thirdPersonPlayerModel.soundOther.loop = false;

                            //Set correct clip
                            if (data.boltActionTime == 1)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = boltActionSoundNormal;
                            }
                            else
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = boltActionSoundLast;
                            }

                            //Update range
                            pb.thirdPersonPlayerModel.soundOther.maxDistance = boltActionSoundThirdPersonMaxRange;
                            //Update sound rolloff
                            pb.thirdPersonPlayerModel.soundOther.SetCustomCurve(AudioSourceCurveType.CustomRolloff, boltActionSoundThirdPersonRolloff);

                            //Play
                            pb.thirdPersonPlayerModel.soundOther.Play();

                            //Set back to 0
                            data.boltActionState = 0;
                        }
                    }
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                    //Full auto fire replica
                    if (data.isFiring)
                    {
                        if (fireMode == FireMode.Auto)
                        {
                            //Compare with fire rate
                            if (Time.time >= data.lastFire + fireRate)
                            {
                                FireOneShotOthers(pb, data);
                            }
                        }
                    }

                    //Check for bolt action
                    if (data.boltActionState == 1 || data.boltActionState == 2)
                    {
                        //Check if the time has passed
                        if (Time.time > data.boltActionTime)
                        {
                            //Set values
                            pb.thirdPersonPlayerModel.soundOther.loop = false;

                            //Set correct clip
                            if (data.boltActionTime == 1)
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = boltActionSoundNormal;
                            }
                            else
                            {
                                pb.thirdPersonPlayerModel.soundOther.clip = boltActionSoundLast;
                            }

                            //Update range
                            pb.thirdPersonPlayerModel.soundOther.maxDistance = boltActionSoundThirdPersonMaxRange;
                            //Update sound rolloff
                            pb.thirdPersonPlayerModel.soundOther.SetCustomCurve(AudioSourceCurveType.CustomRolloff, boltActionSoundThirdPersonRolloff);

                            //Play
                            pb.thirdPersonPlayerModel.soundOther.Play();

                            //Set back to 0
                            data.boltActionState = 0;
                        }
                    }
                }
            }

            public override void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed)
            {
                if (pb.isBot) return;
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                    //Camera animation
                    if (data.weaponRenderer.cameraAnimationEnabled)
                    {
                        if (data.weaponRenderer.cameraAnimationType == CameraAnimationType.Copy)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.weaponRenderer.cameraAnimationReferenceRotation) * data.weaponRenderer.cameraAnimationBone.localRotation;
                        }
                        else if (data.weaponRenderer.cameraAnimationType == CameraAnimationType.LookAt)
                        {
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.weaponRenderer.cameraAnimationReferenceRotation) * Quaternion.LookRotation(data.weaponRenderer.cameraAnimationTarget.localPosition - data.weaponRenderer.cameraAnimationBone.localPosition);
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

                    //Aiming multiplier
                    if (data.isAiming)
                    {
                        data.weaponDelayCurrentX *= weaponDelayAimingMultiplier;
                        data.weaponDelayCurrentY *= weaponDelayAimingMultiplier;
                    }

                    //Update Vector
                    data.weaponDelayCur.x = data.weaponDelayCurrentX;
                    data.weaponDelayCur.y = data.weaponDelayCurrentY;
                    data.weaponDelayCur.z = 0f;

                    //Smooth move towards the target
                    data.weaponDelayTransform.localPosition = Vector3.Lerp(data.weaponDelayTransform.localPosition, data.weaponDelayCur, Time.deltaTime * weaponDelaySmooth);

                    //Weapon tilt
                    if (weaponTiltEnabled)
                    {
                        if (!weaponTiltEnabledWhileAiming && data.isAiming)
                        {
                            data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                        }
                        else
                        {
                            data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.Euler(0, 0, -pb.movement.GetMovementDirection(pb).x * weaponTiltIntensity), Time.deltaTime * weaponTiltReturnSpeed);
                        }
                    }
                    else
                    {
                        data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                    }

                    //Weapon Fall
                    data.weaponFallTransform.localRotation = Quaternion.Slerp(data.weaponFallTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                    //Recoil
                    //Apply to transform
                    pb.recoilApplyRotation = Quaternion.Slerp(pb.recoilApplyRotation, Quaternion.identity, Time.deltaTime * recoilReturnSpeed);

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
                    //Check state and if we can move (Selected, not reloading)
                    if (id == 2 && data.isSelectedAndReady && !data.reloadInProgress)
                    {
                        //Move to run pos
                        data.weaponRenderer.transform.localPosition = Vector3.Lerp(data.weaponRenderer.transform.localPosition, data.weaponRenderer.runPos, Time.deltaTime * data.weaponRenderer.runSmooth);
                        //Move to run rot
                        data.weaponRenderer.transform.localRotation = Quaternion.Slerp(data.weaponRenderer.transform.localRotation, Quaternion.Euler(data.weaponRenderer.runRot), Time.deltaTime * data.weaponRenderer.runSmooth);
                        //Set time
                        data.lastRun = Time.time;
                    }
                    else
                    {
                        //Move back to idle pos
                        data.weaponRenderer.transform.localPosition = Vector3.Lerp(data.weaponRenderer.transform.localPosition, Vector3.zero, Time.deltaTime * data.weaponRenderer.runSmooth * 2f);
                        //Move back to idle rot
                        data.weaponRenderer.transform.localRotation = Quaternion.Slerp(data.weaponRenderer.transform.localRotation, Quaternion.identity, Time.deltaTime * data.weaponRenderer.runSmooth * 2f);
                    }


                    //If we are aiming, force idle animation
                    if (data.isAiming && !enableWalkAnimationWhileAiming)
                    {
                        //Play idle animation
                        data.genericAnimator.CrossFade("Idle", 0.3f);
                        //Also set last id to 0, so it can be updated if we stop aiming
                        data.lastWeaponAnimationID = 0;
                    }
                    else
                    {
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
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.SetTrigger("End Run");
                                        }
                                
                                    }
                                }
                            }
                            //Walk
                            else if (id == 1 && (!data.isAiming || enableWalkAnimationWhileAiming))
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
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("Start Run");
                                            data.weaponRenderer.anim.SetTrigger("End Run");
                                        }
 
                                    }
                                }
                            }
                            //Run
                            else if (id == 2 && (!data.isAiming || enableWalkAnimationWhileAiming))
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
                                    if (!data.startedRunAnimation && !data.reloadInProgress && data.isSelectedAndReady)
                                    {
                                        data.startedRunAnimation = true;
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("End Run");
                                            data.weaponRenderer.anim.SetTrigger("Start Run");
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
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("Start Run");
                                            data.weaponRenderer.anim.SetTrigger("End Run");
                                        }
  
                                    }
                                }
                                //Walk
                                else if (id == 1 && (!data.isAiming || enableWalkAnimationWhileAiming))
                                {
                                    //End run animation on weapon animator
                                    if (data.startedRunAnimation)
                                    {
                                        data.startedRunAnimation = false;
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("Start Run");
                                            data.weaponRenderer.anim.SetTrigger("End Run");
                                        }
         
                                    }
                                }
                                //Run
                                else if (id == 2 && !data.isAiming)
                                {
                                    //Start run animation on weapon animator
                                    if (!data.startedRunAnimation && !data.reloadInProgress && data.isSelectedAndReady)
                                    {
                                        data.startedRunAnimation = true;
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("End Run");
                                            data.weaponRenderer.anim.SetTrigger("Start Run");
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
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

            public override void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset firing
                    data.isFiring = false;
                    if (!pb.isBot)
                    {
                        //Reset pos & rot of the renderer
                        data.weaponRenderer.transform.localPosition = Vector3.zero;
                        data.weaponRenderer.transform.localRotation = Quaternion.identity;
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                    }
                    //Reset aiming
                    data.aimingProgress = 0f;
                    if (!pb.isBot)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                    }
                    data.isAiming = false;
                    //Set default values
                    data.reloadInProgress = false;
                    //Reset spray pattern
                    data.sprayPatternState = 0f;
                    if (!pb.isBot)
                    {
                        //Enable anim
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = true;
                            //Play animation
                            data.weaponRenderer.anim.Play("Draw", 0, 0f);
                        }

                        //Reset shell ejection
                        data.shellEjectEnabled = false;
                        data.shellEjectNext = 0f;
                    }
                    //Reset bolt
                    data.boltActionState = 0;
                    data.boltActionTime = 0f;
                    if (!pb.isBot)
                    {
                        //Play sound if it is assigned
                        if (drawSound) data.soundOther.PlayOneShot(drawSound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.weaponRenderer.visible = true;
                        }
                        else
                        {
                            data.weaponRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon and hide
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = false;
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

            public override void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset firing
                    data.isFiring = false;
                    //Reset aiming
                    data.aimingProgress = 0f;
                    if (!pb.isBot)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                    }
                    data.isAiming = false;
                    if (!pb.isBot)
                    {
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Enable anim
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = true;
                        }
            
                        //Stop reload sound
                        data.soundReload.Stop();
                        //Play sound if it is assigned
                        if (putawaySound) data.soundOther.PlayOneShot(putawaySound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.weaponRenderer.visible = true;
                        }
                        else
                        {
                            //Hide
                            data.weaponRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && !pb.isBot)
                    {
                        //Show tp weapon
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    //Reset firing
                    data.isFiring = false;
                    //Reset aiming
                    data.aimingProgress = 0f;
                    if (!pb.isBot)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                    }
                    data.isAiming = false;
                    if (!pb.isBot)
                    {
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Hide weapon
                        data.weaponRenderer.visible = false;
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = false;
                        }

                        //Reset pos & rot of the renderer
                        data.weaponRenderer.transform.localPosition = Vector3.zero;
                        data.weaponRenderer.transform.localRotation = Quaternion.identity;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                    }
                    //Hide tp weapon
                    data.tpWeaponRenderer.visible = false;
                    //Make sure it is not ready
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void DrawWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpWeaponRenderer.visible = true;
                    //Reset firing
                    data.isFiring = false;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    //Show tp weapon
                    data.tpWeaponRenderer.visible = true;
                    //Reset firing
                    data.isFiring = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHideOthers(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    //Hide tp weapon
                    data.tpWeaponRenderer.visible = false;
                    //Reset firing
                    data.isFiring = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData)
            {
                if (stream.IsWriting)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Send firing
                        stream.SendNext(data.isFiring);

                        try
                        {
                            for (int i = 0; i < data.weaponRenderer.cachedSyncAttachments.Length; i++)
                            {
                                int index = i;
                                data.weaponRenderer.cachedSyncAttachments[i].OnPhotonSerializeView(stream, info, pb, data, index);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        //Dummy send
                        stream.SendNext(false);
                    }
                }
                else
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                    {
                        WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                        //Receive firing
                        data.isFiring = (bool)stream.ReceiveNext();

                    }
                    else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Receive firing
                        data.isFiring = (bool)stream.ReceiveNext();

                        try
                        {
                            for (int i = 0; i < data.weaponRenderer.cachedSyncAttachments.Length; i++)
                            {
                                int index = i;
                                data.weaponRenderer.cachedSyncAttachments[i].OnPhotonSerializeView(stream, info, pb, data, index);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        //Dummy read
                        stream.ReceiveNext();
                    }
                }
            }

            public override void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    FireOneShotOthers(pb, data);
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    FireOneShotOthers(pb, data);
                }
            }

            public override void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    FireBoltActionOthers(pb, data, state);
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    FireBoltActionOthers(pb, data, state);
                }
            }

            public override void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int burstLength)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                {
                    WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                    FireBurstOthers(pb, data, burstLength);
                }
                else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    FireBurstOthers(pb, data, burstLength);
                }
            }

            public override void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty, object runtimeData)
            {
                //Play third person reload animation
                pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType);
                //Play reload sound
                if (isEmpty)
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSoundEmpty;
                    //Set distance
                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                    //Set rolloff
                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundReload.Play();
                }
                else
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                    //Set distance
                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                    //Set rolloff
                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundReload.Play();
                }
            }

            public override void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage, object runtimeData)
            {
                //Play sounds
                //0 = Start
                if (stage == 0)
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartSound;
                    //Set distance
                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                    //Set rolloff
                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundReload.Play();
                }
                else if (stage == 2)
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                    //Set distance
                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                    //Set rolloff
                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundReload.Play();
                }
                else if (stage == 4)
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                    //Set distance
                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                    //Set rolloff
                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundReload.Play();
                }
            }

            public override WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData)
            {
                WeaponIKValues toReturn = new WeaponIKValues();
                if (pb.isController)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        toReturn.leftHandIK = data.tpWeaponRenderer.leftHandIK;

                        //Check if third person reload animation is being played
                        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Rifle") || anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Pistol"))
                        {
                            toReturn.canUseIK = false;
                        }
                        else
                        {
                            toReturn.canUseIK = true;
                        }
                    }
                    else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                    {
                        WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                        toReturn.leftHandIK = data.tpWeaponRenderer.leftHandIK;

                        //Check if third person reload animation is being played
                        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Rifle") || anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Pistol"))
                        {
                            toReturn.canUseIK = false;
                        }
                        else
                        {
                            toReturn.canUseIK = true;
                        }
                    }
                }
                else
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        toReturn.leftHandIK = data.tpWeaponRenderer.leftHandIK;

                        //Check if third person reload animation is being played
                        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Rifle") || anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Pistol"))
                        {
                            toReturn.canUseIK = false;
                        }
                        else
                        {
                            toReturn.canUseIK = true;
                        }
                    }
                    else if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerOthersRuntimeData))
                    {
                        WeaponControllerOthersRuntimeData data = runtimeData as WeaponControllerOthersRuntimeData;
                        toReturn.leftHandIK = data.tpWeaponRenderer.leftHandIK;

                        //Check if third person reload animation is being played
                        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Rifle") || anim.GetCurrentAnimatorStateInfo(1).IsName("Reload Pistol"))
                        {
                            toReturn.canUseIK = false;
                        }
                        else
                        {
                            toReturn.canUseIK = true;
                        }
                    }
                }
                return toReturn;
            }

            public override WeaponStats GetStats()
            {
                WeaponStats toReturn = new WeaponStats();
                //Set damage
                toReturn.damage = baseDamage;
                //Set Fire Rate
                toReturn.fireRate = RPM;
                //Set reach
                toReturn.reach = range;
                //Set recoil
                toReturn.recoil = ((Mathf.Abs(recoilPerShotMax.x) + Mathf.Abs(recoilPerShotMax.y) + Mathf.Abs(recoilPerShotMin.x) + Mathf.Abs(recoilPerShotMax.y)) / (float)recoilReturnSpeed);
                return toReturn;
            }

            public override float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.isAimedIn)
                    {
                        if (!pb.isBot && data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            return Kit_GameSettings.fullScreenAimSensitivity;
                        }
                        else
                        {
                            return Kit_GameSettings.aimSensitivity;
                        }
                    }
                    else
                    {
                        return Kit_GameSettings.hipSensitivity;
                    }
                }
                return 1f;

            }

            public override int WeaponState(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.bulletsLeft > 0) return 0;
                    else if (data.bulletsLeftToReload > 0) return 1;
                    else return 2;
                }
                return 0;
            }

            public override int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (fireMode == FireMode.Auto)
                {
                    return 0;
                }
                else if (fireMode == FireMode.Semi)
                {
                    if (fireTypeMode == FireTypeMode.Simple) return 1;
                    else return 2;
                }
                else if (fireMode == FireMode.BoltAction)
                {
                    if (fireTypeMode == FireTypeMode.Simple) return 1;
                    else return 2;
                }
                else
                {
                    return 1;
                }
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    //Get runtime data
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Activate or deactivate based on bool
                    if (perspective == Kit_GameInformation.Perspective.ThirdPerson)
                    {
                        if (data.weaponRenderer)
                        {
                            data.weaponRenderer.visible = false;
                        }
                        if (data.tpWeaponRenderer)
                        {
                            data.tpWeaponRenderer.visible = true;
                            data.tpWeaponRenderer.shadowsOnly = false;
                        }
                    }
                    else
                    {
                        if (data.weaponRenderer && !data.sniperWeaponHidden)
                        {
                            data.weaponRenderer.visible = true;
                        }
                        if (data.tpWeaponRenderer)
                        {
                            data.tpWeaponRenderer.visible = true;
                            data.tpWeaponRenderer.shadowsOnly = true;
                        }
                    }
                }
            }
            #endregion

            #region Unique functions
            /// <summary>
            /// Fires one shot locally
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireOneShot(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Update spawn protection
                if (pb.spawnProtection)
                {
                    pb.spawnProtection.GunFired(pb);
                }

                //Call network (if semi fire)
                if (fireMode == FireMode.Semi)
                {
                    pb.photonView.RPC("WeaponSemiFireNetwork", RpcTarget.Others);
                }
                else if (fireMode == FireMode.BoltAction)
                {
                    pb.photonView.RPC("WeaponBoltActionFireNetwork", RpcTarget.Others, (data.bulletsLeft == 1 ? 2 : 1)); //2 = Last, 1 = Normal; 0 would be none
                }

                //Set bolt action data
                if (fireMode == FireMode.BoltAction)
                {
                    if (data.bulletsLeft == 1)
                    {
                        //Set delay
                        data.boltActionTime = Time.time + boltActionDelayLast;
                        //Set state
                        data.boltActionState = 2;
                        //Set fire rate
                        data.lastFire = Time.time + boltActionTimeLast;
                    }
                    else
                    {
                        //Set delay
                        data.boltActionTime = Time.time + boltActionDelayNormal;
                        //Set state
                        data.boltActionState = 1;
                        //Set fire rate
                        data.lastFire = Time.time + boltActionTimeNormal;
                    }
                }
                else
                {
                    //Set firerate
                    data.lastFire = Time.time;
                }

                if (!pb.isBot)
                {
                    //Play sound
                    data.soundFire.PlayOneShot(data.weaponRenderer.cachedFireSound);
                    //Play fire animation
                    if (data.bulletsLeft == 1)
                    {
                        //Last fire
                        if (data.weaponRenderer.anim)
                        {
                            //Play animation
                            data.weaponRenderer.anim.Play("Fire Last", 0, 0f);
                        }

                    }
                    else
                    {
                        if (data.isAiming)
                        {
                            //Normal fire (in aiming mode)
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire Aim", 0, 0f);
                            }
 
                        }
                        else
                        {
                            //Normal fire
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire", 0, 0f);
                            }

                        }
                    }
                }
                else
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                    //Play Muzzle Flash Particle System, if assigned
                    if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.tpWeaponRenderer.muzzleFlash.Play(true);
                    }
                }

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set shell ejection
                if (shellEjectionPrefab && !pb.isBot)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Play Muzzle Flash Particle System, if assigned
                    if (!pb.isBot && data.weaponRenderer.muzzleFlash && data.weaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.weaponRenderer.muzzleFlash.Play(true);
                    }
                }
                else
                {
                    if (!pb.isBot && data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.tpWeaponRenderer.muzzleFlash.Play(true);
                    }
                }

                //Subtract bullets
                if (!pb.main.gameInformation.debugEnableUnlimitedBullets)
                    data.bulletsLeft--;

                //Simple fire
                if (fireTypeMode == FireTypeMode.Simple)
                {
                    if (bulletsMode == BulletMode.Raycast)
                    {
                        //Fire Raycast
                        FireRaycast(pb, data);
                    }
                    else if (bulletsMode == BulletMode.Physical)
                    {
                        //Fire Physical Bullet
                        FirePhysicalBullet(pb, data);
                    }
                }
                //Pellet fire
                else if (fireTypeMode == FireTypeMode.Pellets)
                {
                    //Count how many have been shot
                    int pelletsShot = 0;
                    while (pelletsShot < amountOfPellets)
                    {
                        //Increase amount of shot ones
                        pelletsShot++;
                        if (bulletsMode == BulletMode.Raycast)
                        {
                            //Fire Raycast
                            FireRaycast(pb, data);
                        }
                        else if (bulletsMode == BulletMode.Physical)
                        {
                            //Fire Physical Bullet
                            FirePhysicalBullet(pb, data);
                        }
                    }
                }

                //Increase spray pattern
                if (bulletSpreadMode == SpreadMode.SprayPattern)
                {
                    data.sprayPatternState++;
                }

                //Apply recoil using coroutine helper
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponApplyRecoil(this, data, pb, RandomExtensions.RandomBetweenVector2(recoilPerShotMin, recoilPerShotMax), recoilApplyTime));
            }

            /// <summary>
            /// Fires one shot (if we are not the controller)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireOneShotOthers(Kit_PlayerBehaviour pb, WeaponControllerOthersRuntimeData data)
            {
                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }
            }

            /// <summary>
            /// Fires one shot (if we are not the controller)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireOneShotOthers(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }
            }

            void FireBoltActionOthers(Kit_PlayerBehaviour pb, WeaponControllerOthersRuntimeData data, int state)
            {
                //Set bolt action
                if (state == 1)
                {
                    //Set state and time for normal
                    data.boltActionState = state;
                    data.boltActionTime = Time.time + boltActionDelayNormal;
                }
                else if (state == 2)
                {
                    //Set state and time for last
                    data.boltActionState = state;
                    data.boltActionTime = Time.time + boltActionDelayLast;
                }

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }
            }

            /// <summary>
            /// Fires one shot with bolt action (if we are not the controller)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireBoltActionOthers(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int state)
            {
                //Set bolt action
                if (state == 1)
                {
                    //Set state and time for normal
                    data.boltActionState = state;
                    data.boltActionTime = Time.time + boltActionDelayNormal;
                }
                else if (state == 2)
                {
                    //Set state and time for last
                    data.boltActionState = state;
                    data.boltActionTime = Time.time + boltActionDelayLast;
                }

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }
            }

            /// <summary>
            /// Fire one burst (local)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireBurst(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Update spawn protection
                if (pb.spawnProtection)
                {
                    pb.spawnProtection.GunFired(pb);
                }

                //Call network and already tell how many shots are going to be fired
                pb.photonView.RPC("WeaponBurstFireNetwork", RpcTarget.Others, Mathf.Clamp(data.bulletsLeft, 0, burstBulletsPerShot));

                //Start Coroutine
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponBurstFire(this, data, pb));
            }

            /// <summary>
            /// Fire one burst (for others)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireBurstOthers(Kit_PlayerBehaviour pb, WeaponControllerOthersRuntimeData data, int burstLength)
            {
                //Start Coroutine
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponBurstFireOthers(this, data, pb, burstLength));
            }

            /// <summary>
            /// Fire one burst (for others)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireBurstOthers(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int burstLength)
            {
                //Start Coroutine
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponBurstFireOthers(this, data, pb, burstLength));
            }

            public void FireRaycast(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (bulletsPenetrationEnabled)
                {
                    //Will be set to true again if bullet penetrated through something
                    bool continueLoop = true;

                    //Bullet penetration ability
                    int bulletLifeLeft = bulletsPenetrationForce;

                    //So that we may not damage the same player twice
                    List<Kit_PlayerBehaviour> damagedPlayers = new List<Kit_PlayerBehaviour>();

                    RaycastHit hit;
                    Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                    Vector3 pos = pb.playerCameraTransform.position;

                    //Override if third person is active
                    if (!pb.isBot && pb.main.gameInformation.thirdPersonCameraShooting && Kit_GameSettings.isThirdPersonActive)
                    {
                        dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                        pos = pb.main.mainCamera.transform.position;
                    }

                    float remainingDistance = range;

                    int tries = 0;

                    while (tries < 10 && Physics.Raycast(pos, dir, out hit, remainingDistance, pb.weaponHitLayers.value))
                    {
                        //Add
                        tries++;
                        //Distract distance
                        remainingDistance -= hit.distance;
                        //Set new pos
                        pos = hit.point;
                        if (continueLoop)
                        {
                            //Check if we hit ourselves
                            if (hit.transform.root != pb.transform.root)
                            {
                                //Set Loop to false
                                continueLoop = false;
                                #region Damage
                                //Check if we hit a player
                                if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>())
                                {
                                    Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                                    if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                                    {
                                        Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                        //Check if we damaged this player already
                                        if (!damagedPlayers.Contains(hitPb))
                                        {
                                            //Add player
                                            damagedPlayers.Add(hitPb);
                                            //First check if we can actually damage that player
                                            if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                            {
                                                //Check if he has spawn protection
                                                if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                                {
                                                    //Apply local damage, sample damage dropoff via distance
                                                    hitPb.LocalDamage(GetDamage(Vector3.Distance(pb.playerCameraTransform.position, hit.point)) * pdm.damageMultiplier, gameGunID, pb.transform.position, dir, ragdollForce, hit.point, pdm.ragdollId, pb.isBot, pb.id);
                                                    if (!pb.isBot)
                                                    {
                                                        //Since we hit a player, show the hitmarker
                                                        pb.main.hud.DisplayHitmarker();
                                                    }
                                                }
                                                else if (!pb.isBot)
                                                {
                                                    //We hit a player but his spawn protection is active
                                                    pb.main.hud.DisplayHitmarkerSpawnProtected();
                                                }
                                            }
                                            //Send to hit processor
                                            pb.main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                                            //Tell other players we hit something
                                            pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, -1);
                                        }
                                    }
                                }
                                #endregion
                                #region Particles and Interface
                                else
                                {
                                    //Proceed to hit processor
                                    if (hit.collider.CompareTag("Dirt")) //Check for dirt
                                    {
                                        //Call
                                        pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 1, hit.transform);
                                        //Tell other players we hit something
                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 1);
                                    }
                                    else if (hit.collider.CompareTag("Metal")) //Check for metal
                                    {
                                        //Call
                                        pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 2, hit.transform);
                                        //Tell other players we hit something
                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 2);
                                    }
                                    else if (hit.collider.CompareTag("Wood")) //Check for wood
                                    {
                                        //Call
                                        pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 3, hit.transform);
                                        //Tell other players we hit something
                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 3);
                                    }
                                    else if (hit.collider.CompareTag("Blood")) //Check for blood
                                    {
                                        //Call
                                        pb.main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                                        //Tell other players we hit something
                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, -1);
                                    }
                                    else //Else use concrete
                                    {
                                        //Call
                                        pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 0, hit.transform);
                                        //Tell other players we hit something
                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 0);
                                    }

                                    if (hit.collider.GetComponentInParent<IKitDamageable>() != null)
                                    {
                                        if (hit.collider.GetComponentInParent<IKitDamageable>().LocalDamage(GetDamage(Vector3.Distance(pb.playerCameraTransform.position, hit.point)), gameGunID, pb.transform.position, dir, ragdollForce, hit.point, pb.isBot, pb.id))
                                        {

                                            if (!pb.isBot)
                                            {
                                                //Since we hit a player, show the hitmarker
                                                pb.main.hud.DisplayHitmarker();
                                            }
                                        }
                                    }
                                }
                                #endregion
                                #region Bullet Penetration
                                //Check if this object can be penetrated
                                if (hit.collider.GetComponent<Kit_PenetrateableObject>())
                                {
                                    Kit_PenetrateableObject penetration = hit.collider.GetComponent<Kit_PenetrateableObject>();
                                    if (bulletLifeLeft >= penetration.cost)
                                    {
                                        //We penetrated, subtract cost
                                        bulletLifeLeft -= penetration.cost;
                                        //Set bool
                                        continueLoop = true;
                                    }
                                }

                                //Abort loop if nothing was penetrated
                                if (!continueLoop) break;
                                #endregion
                            }
                        }
                    }
                }
                else
                {
                    RaycastHit hit;
                    Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                    Vector3 pos = pb.playerCameraTransform.position;

                    //Override if third person is active
                    if (!pb.isBot && pb.main.gameInformation.thirdPersonCameraShooting && Kit_GameSettings.isThirdPersonActive)
                    {
                        dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                        pos = pb.main.mainCamera.transform.position;
                    }

                    RaycastHit[] hits = Physics.RaycastAll(pos, dir, range, pb.weaponHitLayers.value).OrderBy(h => h.distance).ToArray();
                    for (int i = 0; i < hits.Length; i++)
                    {
                        hit = hits[i];
                        //Check if we hit ourselves
                        if (hit.transform.root != pb.transform.root)
                        {
                            //Check if we hit a player
                            if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>())
                            {
                                Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                                if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                                {
                                    Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                    //First check if we can actually damage that player
                                    if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                    {
                                        //Check if he has spawn protection
                                        if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                        {
                                            //Apply local damage, sample damage dropoff via distance
                                            hitPb.LocalDamage(GetDamage(Vector3.Distance(pb.playerCameraTransform.position, hit.point)) * pdm.damageMultiplier, gameGunID, pb.transform.position, dir, ragdollForce, hit.point, pdm.ragdollId, pb.isBot, pb.id);
                                            if (!pb.isBot)
                                            {
                                                //Since we hit a player, show the hitmarker
                                                pb.main.hud.DisplayHitmarker();
                                            }
                                        }
                                        else if (!pb.isBot)
                                        {
                                            //We hit a player but his spawn protection is active
                                            pb.main.hud.DisplayHitmarkerSpawnProtected();
                                        }
                                    }
                                    //Send to hit processor
                                    pb.main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, -1);
                                    break;
                                }
                            }
                            else
                            {
                                //Proceed to hit processor
                                if (hit.collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 1, hit.transform);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 1);
                                }
                                else if (hit.collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 2, hit.transform);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 2);
                                }
                                else if (hit.collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 3, hit.transform);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 3);
                                }
                                else if (hit.collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hit.point, hit.normal, 0, hit.transform);
                                    //Tell other players we hit something
                                    pb.photonView.RPC("WeaponRaycastHit", RpcTarget.Others, hit.point, hit.normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(GetDamage(Vector3.Distance(pb.playerCameraTransform.position, hit.point)), gameGunID, pb.transform.position, dir, ragdollForce, hit.point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

            }

            public override void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir, object runtimeData)
            {
                //Instantiate Game Object
                GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.LookRotation(dir));
                //Call setup
                bullet.GetComponent<Kit_BulletBase>().Setup(pb.main, this, pb, dir);
            }

            public void FirePhysicalBullet(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                Vector3 pos = pb.playerCameraTransform.position;

                //Override if third person is active
                if (!pb.isBot && pb.main.gameInformation.thirdPersonCameraShooting && Kit_GameSettings.isThirdPersonActive)
                {
                    dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                    pos = pb.main.mainCamera.transform.position;
                }

                //Send RPC
                pb.photonView.RPC("WeaponFirePhysicalBulletOthers", RpcTarget.Others, pos, dir);

                //Instantiate Game Object
                GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.LookRotation(dir));
                //Call setup
                bullet.GetComponent<Kit_BulletBase>().Setup(pb.main, this, pb, dir);
            }

            /// <summary>
            /// Returns the damage for given distance
            /// </summary>
            /// <param name="distance"></param>
            /// <returns></returns>
            float GetDamage(float distance)
            {
                return baseDamage * damageDropoff.Evaluate(distance);
            }

            /// <summary>
            /// Returns a direction (for offset) based on data and this behaviour's stats
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            Vector3 GetSpread(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                float velocity = pb.cc.velocity.magnitude;

                #region Hip Spread
                Vector3 spreadHip = Vector3.zero;
                spreadHip.x = Random.Range(-bulletSpreadHipBase, bulletSpreadHipBase);
                spreadHip.y = Random.Range(-bulletSpreadHipBase, bulletSpreadHipBase);
                spreadHip.z = Random.Range(-bulletSpreadHipBase, bulletSpreadHipBase);

                //Velocity add
                if (velocity > 0)
                {
                    spreadHip.x += RandomExtensions.RandomPosNeg() * bulletSpreadHipVelocityAdd * (velocity / bulletSpreadHipVelocityReference);
                    spreadHip.y += RandomExtensions.RandomPosNeg() * bulletSpreadHipVelocityAdd * (velocity / bulletSpreadHipVelocityReference);
                    spreadHip.z += RandomExtensions.RandomPosNeg() * bulletSpreadHipVelocityAdd * (velocity / bulletSpreadHipVelocityReference);
                }
                #endregion

                #region Aim Spread
                Vector3 spreadAim = Vector3.zero;
                spreadAim.x = Random.Range(-bulletSpreadAimBase, bulletSpreadAimBase);
                spreadAim.y = Random.Range(-bulletSpreadAimBase, bulletSpreadAimBase);
                spreadAim.z = Random.Range(-bulletSpreadAimBase, bulletSpreadAimBase);

                //Velocity add
                if (velocity > 0)
                {
                    spreadAim.x += RandomExtensions.RandomPosNeg() * bulletSpreadAimVelocityAdd * (velocity / bulletSpreadAimVelocityReference);
                    spreadAim.y += RandomExtensions.RandomPosNeg() * bulletSpreadAimVelocityAdd * (velocity / bulletSpreadAimVelocityReference);
                    spreadAim.z += RandomExtensions.RandomPosNeg() * bulletSpreadAimVelocityAdd * (velocity / bulletSpreadAimVelocityReference);
                }
                #endregion

                if (bulletSpreadMode == SpreadMode.Simple)
                {
                    //Interpolate between both based on aiming progress
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress);
                }
                else if (bulletSpreadMode == SpreadMode.SprayPattern && bulletSpreadSprayPattern.Length > 0)
                {
                    //Interpolate between both based on aiming progress and add spray pattern
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress) + bulletSpreadSprayPattern[Mathf.Clamp(Mathf.RoundToInt(data.sprayPatternState), 0, bulletSpreadSprayPattern.Length - 1)];
                }
                else
                {
                    //Interpolate between both based on aiming progress
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress);
                }
            }

            float GetCrosshairSize(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (crosshairEnabled)
                {
                    //This is essentially the bullet spread part without the random factors.
                    float velocity = pb.cc.velocity.magnitude;
                    float spreadHip = bulletSpreadHipBase;

                    //Velocity add
                    if (velocity > 0)
                    {
                        spreadHip += bulletSpreadHipVelocityAdd * (velocity / bulletSpreadHipVelocityReference);
                    }

                    return Mathf.Lerp(spreadHip, 0f, data.aimingProgress) * crosshairSizeMultiplier;
                }
                //If crosshair is not enablewd
                else
                {
                    //Return size of 0 which will disable the crosshair
                    return 0f;
                }
            }

            void DryFire(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (!pb.isBot)
                {
                    //Play sound (on Other channel so fire sound can still play)
                    data.soundOther.PlayOneShot(dryFireSound);
                    //Play Dry Fire animation
                    if (data.weaponRenderer.anim)
                    {
                        //Play animation
                        data.weaponRenderer.anim.Play("Dry Fire", 0, 0f);
                    }
 
                }
                //Set firerate
                data.lastFire = Time.time + 1f; //+1 so you don't dry fire too quick
            }

            /// <summary>
            /// Ejects a single shell
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void EjectShell(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (shellEjectionPrefab & data.weaponRenderer.shellEjectTransform)
                {
                    GameObject shellObj = null;
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                    {
                        shellObj = Instantiate(shellEjectionPrefab, data.weaponRenderer.shellEjectTransform.position, data.weaponRenderer.shellEjectTransform.rotation);
                    }
                    else
                    {
                        shellObj = Instantiate(shellEjectionPrefab, data.tpWeaponRenderer.shellEjectTransform.position, data.weaponRenderer.shellEjectTransform.rotation);
                    }
                    if (!shellObj) return;
                    //Apply force
                    Rigidbody rb = shellObj.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.velocity = pb.cc.velocity;
                    //Add force
                    rb.AddRelativeForce(RandomExtensions.RandomBetweenVector3(shellEjectionMinForce, shellEjectionMaxForce));
                    //Add torque
                    rb.AddRelativeTorque(RandomExtensions.RandomBetweenVector3(shellEjectionMinTorque, shellEjectionMaxTorque));
                }
            }

            public override bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    return data.isAiming; //Just relay
                }
                return false;
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    return data.isAiming && data.weaponRenderer.cachedUseFullscreenScope; //Just relay
                }
                return false;
            }

            public override float AimInTime(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return aimInTime;
            }

            public override float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Lerp from base multiplier to base multiplier multiplied with the aim speed multiplier
                    return Mathf.Lerp(speedMultiplierBase, speedMultiplierBase * aimSpeedMultiplier, data.aimingProgress);
                }
                return 1f;
            }
            #endregion
        }
    }
}
