using Photon.Pun;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ImmixKit
{
    //Runtime data to be stored as an object on the player
    public class BootsOnGroundRuntimeData
    {
        public Vector3 desiredMoveDirection = Vector3.zero;

        public Vector3 swimmingWorldMoveDirection = Vector3.zero;

        public Vector3 moveDirection = Vector3.zero;

        public Vector3 localMoveDirection = Vector3.zero;

        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// </summary>
        public int state;
 
        public bool isJumping;
 
        public int jumpCount;
    
        public bool isSprinting;
  
        public string currentMaterial;
 
        public float nextFootstep;

        public bool lastCrouch = false;

        public bool lastJump = false;

        public bool playSlowWalkAnimation;

        #region Fall Damage
        public bool falling;
      
        public float fallDistance;
        public float fallHighestPoint;
        #endregion

        #region Stamina
        public float staminaLeft = 100f;
     
        public float staminaRegenerationTime = 0f;
     
        public float staminaDepletedSprintingBlock;
        #endregion
    }

    public class BootsOnGroundSyncRuntimeData
    {
        public Vector3 velocity;
      
        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// </summary>
        public int state;
      
        public bool isSprinting;
      
        public string currentMaterial;
       
        public float nextFootstep;
       
        public bool playSlowWalkAnimation;
    }

  
    [System.Serializable]
    public class Footstep
    {
    
        public AudioClip[] clips;
      
        public float maxDistance = 20f;
      
        public AnimationCurve rollOff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
    }

    [System.Serializable]
    public class StringFootstepDictionary : SerializableDictionary<string, Footstep> { }

    [System.Serializable]
    public class NestedSound
    {
        public AudioClip[] clips;
    }

    public class Kit_Movement_BootsOnGround : Kit_MovementBase
    {
        [Header("Stats")]
        [Tooltip("Sprinting speed")]
        public float sprintSpeed = 6f;
        [Tooltip("Normal walk speed")]
        public float walkSpeed = 6f;
        [Tooltip("Crouch walk speed")]
        public float crouchSpeed = 6f;

     
        public bool airControlEnabled = false;

     
        public float gravityMultiplier = 1f;

     
        public float jumpSpeed = 8f;
     
      
        public int jumpMaxCount = 1;

     
        public string[] jumpAnimations;

     
        public NestedSound[] jumpSound;

        
        public NestedSound[] jumpLandSound;

        [Header("Character Heights")]
        public float standHeight = 1.8f; //State 0 height
        public float crouchHeight = 1.2f; //State 1 height

        [Header("Camera Positions")]
        public float camPosSmoothSpeed = 6f; 
        public Vector3 camPosStand = new Vector3(0, 1.65f, 0f);
        public Vector3 camPosCrouch = new Vector3(0, 1.05f, 0f);

        [Header("Fall Damage")]
        public float fallDamageThreshold = 10;
      
        public float fallDamageMultiplier = 5f;

        [Header("Others")]
      
        public float defaultYmove = -2f; //How many units should we be moved down by default? To be able to walk down stairs properly

        [Header("Footsteps")]
        public float footstepsRunTime = 0.25f; 
        public float footstepsWalkTime = 0.4f; 
        public float footstepsCrouchTime = 0.7f; 

        public float footstepsRunVolume = 0.8f; 
        public float footstepsWalkVolume = 0.4f; 
        public float footstepsCrouchVolume = 0.1f; 

        public StringFootstepDictionary allFootsteps; 

        [Header("Fall Down effect")]
        public float fallDownAmount = 10.0f;
        public float fallDownMinOffset = -6.0f;
        public float fallDownMaxoffset = 6.0f;
        public float fallDownTime = 0.3f;
        public float fallDownReturnSpeed = 1f;

  
        [Header("Stamina")]
        public bool staminaSystemEnabled = true;
      
        public float staminaDecreaseRate = 5f;
     
        public float staminaIncreaseRate = 3f;
      
        public float staminaPauseRateNotEmpty = 2f;
      
        public float staminaPauseRateEmpty = 5f;
      
        public float staminaDepletedSprintBlockTime = 7f;
      
        public AudioClip[] staminaExhaustedSound;

     

        public override void CalculateMovementUpdate(Kit_PlayerBehaviour pb)
        {
           
            if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundRuntimeData))
            {
                pb.customMovementData = new BootsOnGroundRuntimeData();

            }

            BootsOnGroundRuntimeData data = pb.customMovementData as BootsOnGroundRuntimeData;

            //Move transform back
            pb.playerCameraFallDownTransform.localRotation = Quaternion.Slerp(pb.playerCameraFallDownTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                if (pb.cc.isGrounded || airControlEnabled)
                {
                    float yMovement = data.moveDirection.y;
                    #region Main Input
                    //Only get input if the cursor is locked
                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        //Calculate move direction based on input
                        data.moveDirection.x = pb.input.hor;
                        data.moveDirection.y = 0f;
                        data.moveDirection.z = pb.input.ver;

                        //Check if we want to move
                        if (data.isGrounded && data.moveDirection.sqrMagnitude > 0.005f)
                        {
                            //Call for spawn protection
                            if (pb.spawnProtection)
                            {
                                pb.spawnProtection.PlayerMoved(pb);
                            }
                        }

                        //Correct strafe
                        data.moveDirection = Vector3.ClampMagnitude(data.moveDirection, 1f);

                        if (pb.cc.isGrounded)
                        {
                            data.moveDirection.y = defaultYmove;
                        }
                        else
                        {
                            data.moveDirection.y = 0f;
                        }

                        //Get sprinting input
                        if (pb.input.sprint && data.moveDirection.z > 0.3f && pb.weaponManager.CanRun(pb))
                        {
                            //Check if we can sprint
                            if (data.state == 0 && (!staminaSystemEnabled || staminaSystemEnabled && data.staminaLeft >= 0f && Time.time > data.staminaDepletedSprintingBlock))
                            {
                                data.isSprinting = true;

                                if (staminaSystemEnabled)
                                {
                                    data.staminaLeft -= Time.deltaTime * staminaDecreaseRate;

                                    if (data.staminaLeft <= 0f)
                                    {
                                        data.staminaRegenerationTime = Time.time + staminaPauseRateEmpty;
                                        data.staminaDepletedSprintingBlock = Time.time + staminaDepletedSprintBlockTime;

                                      
                                        if (staminaExhaustedSound.Length > 0)
                                        {
                                            pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 0, 0, Random.Range(0, staminaExhaustedSound.Length));
                                        }
                                    }
                                    else
                                    {
                                        //Set time for regen
                                        data.staminaRegenerationTime = Time.time + staminaPauseRateNotEmpty;
                                    }
                                }

                            }
                            //We cannot sprint
                            else
                            {
                                data.isSprinting = false;
                            }
                        }
                        else
                        {
                            //We are not sprinting
                            data.isSprinting = false;
                        }
                    }
                    //If not, don't move
                    else
                    {
                        //Reset move direction
                        data.moveDirection = new Vector3(0f, defaultYmove, 0f);
                        //Reset sprinting
                        data.isSprinting = false;
                    }
                    #endregion

                    data.moveDirection = pb.transform.TransformDirection(data.moveDirection);
                    //Apply speed based on state
                    //Standing
                    if (data.state == 0)
                    {
                        //Sprinting
                        if (data.isSprinting)
                        {
                            data.moveDirection *= sprintSpeed;
                        }
                        //Not sprinting
                        else
                        {
                            data.moveDirection *= walkSpeed;
                        }
                    }
                    //Crouching
                    else if (data.state == 1)
                    {
                        data.moveDirection *= crouchSpeed;
                    }

                    if (!pb.cc.isGrounded)
                    {
                        data.moveDirection.y = yMovement;
                    }
                    else
                    {
                        //Mouse Look multiplier
                        data.moveDirection *= pb.looking.GetSpeedMultiplier(pb);
                        //Weapon multiplier
                        data.moveDirection *= pb.weaponManager.CurrentMovementMultiplier(pb); //Retrive from weapon manager
                                                                                              //Should play slow animation?
                        data.playSlowWalkAnimation = pb.weaponManager.IsAiming(pb); //Retrive from weapon manager
                    }
                }

                if (pb.cc.isGrounded)
                {
                    #region Fall Damage
                    if (data.falling)
                    {
                        //Calculate distance we have fallen
                        data.fallDistance = data.fallHighestPoint - pb.transform.position.y;
                        data.falling = false;
                        if (data.fallDistance > fallDamageThreshold)
                        {
                            //Apply Fall distance multiplied with the multiplier to get damage amount
                            pb.ApplyFallDamage(data.fallDistance * fallDamageMultiplier);
                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                            //Tell weapon manager
                            pb.weaponManager.FallDownEffect(pb, true);
                        }
                        else if (data.fallDistance > 0.1f)
                        {
                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                            //Tell weapon manager
                            pb.weaponManager.FallDownEffect(pb, false);
                        }
                    }
                    #endregion

                    #region Crouch Input
                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        if (Kit_GameSettings.isCrouchToggle)
                        {
                            if (data.lastCrouch != pb.input.crouch)
                            {
                                data.lastCrouch = pb.input.crouch;
                                //Get crouch input
                                if (pb.input.crouch)
                                {
                                    //Change state
                                    if (data.state == 0)
                                    {
                                        //We are standing, crouch
                                        data.state = 1;
                                    }
                                    else if (data.state == 1)
                                    {
                                        //We are crouching, stand up
                                        data.state = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (pb.input.crouch)
                            {
                                data.state = 1;
                            }
                            else
                            {
                                data.state = 0;
                            }
                            data.lastCrouch = pb.input.crouch;
                        }
                    }
                    #endregion

                    #region Jump
                    //Reset jump counter
                    if (data.jumpCount > 0)
                    {
                        //We Landed
                        pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 2, data.jumpCount - 1, Random.Range(0, jumpLandSound[data.jumpCount - 1].clips.Length));
                        data.jumpCount = 0;
                        data.isJumping = false;
                    }

                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        if (data.lastJump != pb.input.jump)
                        {
                            data.lastJump = pb.input.jump;
                            //Get Jump input
                            if (pb.input.jump)
                            {
                                //Check if we can jump
                                if (data.state == 0)
                                {
                                    data.moveDirection.y = jumpSpeed;

                                    data.isJumping = true;

                                    //Play Animation and Jump Sound
                                    pb.photonView.RPC("MovementPlayAnimation", RpcTarget.All, 0, data.jumpCount);
                                    pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 1, data.jumpCount, Random.Range(0, jumpSound[data.jumpCount].clips.Length));

                                    data.jumpCount = 1;

                                    //Play animation

                                    //Call for spawn protection
                                    if (pb.spawnProtection)
                                    {
                                        pb.spawnProtection.PlayerMoved(pb);
                                    }
                                }
                                //If we try to jump and we try to jump, stand up
                                else if (data.state == 1)
                                {
                                    data.state = 0;
                                }
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    //Save initial falling point
                    if (!data.falling)
                    {
                        data.fallHighestPoint = pb.transform.position.y;
                        data.falling = true;
                    }
                    if (pb.transform.position.y > data.fallHighestPoint)
                    {
                        data.fallHighestPoint = pb.transform.position.y;
                    }

                    if (data.isJumping)
                    {
                        if (data.jumpCount < jumpMaxCount)
                        {
                            if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                            {
                                if (data.lastJump != pb.input.jump)
                                {
                                    data.lastJump = pb.input.jump;
                                    //Get Jump input
                                    if (pb.input.jump)
                                    {
                                        //Check if we can jump
                                        if (data.state == 0)
                                        {
                                            data.moveDirection.y = jumpSpeed;
                                            data.isJumping = true;

                                            pb.photonView.RPC("MovementPlayAnimation", RpcTarget.All, 0, data.jumpCount);
                                            pb.photonView.RPC("MovementPlaySound", RpcTarget.All, 1, data.jumpCount, Random.Range(0, jumpSound[data.jumpCount].clips.Length));

                                            data.jumpCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #region Character Height
                //Change character height based on the state
                //Standing
                if (data.state == 0)
                {
                    pb.cc.height = standHeight; //Set height
                    pb.cc.center = new Vector3(0f, standHeight / 2, 0f); //Set center
                }
                //Crouch
                else if (data.state == 1)
                {
                    pb.cc.height = crouchHeight; //Set height
                    pb.cc.center = new Vector3(0f, crouchHeight / 2, 0f); //Set center
                }
                #endregion

                //Apply gravity
                data.moveDirection += Physics.gravity * Time.deltaTime * gravityMultiplier;
                //Move
                CollisionFlags collision = pb.cc.Move(data.moveDirection * Time.deltaTime);
                //Get local movement direction
                data.localMoveDirection = pb.transform.InverseTransformDirection(pb.cc.velocity);
                //Check grounded
                data.isGrounded = pb.cc.isGrounded;


                //Check if we hit a roof
                if (collision.HasFlag(CollisionFlags.Above))
                {
                    if (data.moveDirection.y > 0f)
                    {
                        data.moveDirection.y = -data.moveDirection.y;
                    }
                }
            

            #region CameraMove
            //Standing
            if (data.state == 0)
            {     
                pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosStand + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
            }
            //Crouching
            else if (data.state == 1)
            {         
                pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosCrouch + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
            }
            #endregion

            #region Stamina regen
            if (staminaSystemEnabled)
            {
                //Check if we can  regen
                if (Time.time > data.staminaRegenerationTime)
                {
                    //Check if we need to regen
                    if (data.staminaLeft < 100f)
                    {
                        data.staminaLeft += Time.deltaTime * staminaIncreaseRate;
                    }
                }

                if (!pb.isBot)
                {
                    //Display stamina
                    pb.main.hud.DisplayStamina(data.staminaLeft);
                    
                }
            }
            #endregion

        }

        public override int GetCurrentWeaponMoveAnimation(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.5f)
                    {
                        //Check if we're sprinting, if return 2
                        if (bogrd.isSprinting) return 2;
                        //If not return 1
                        else
                            return 1;
                    }
                }
            }
            return 0;
        }

        public override float GetCurrentWalkAnimationSpeed(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.1f)
                    {
                        //Check if we're sprinting, if return speed divided by sprintSpeed
                        if (bogrd.isSprinting) return pb.cc.velocity.magnitude / sprintSpeed;
                        //If not return speed divided by normal walking speed
                        else
                            return pb.cc.velocity.magnitude / walkSpeed;
                    }
                }
            }
            return 1f;
        }

        public override Vector3 GetMovementDirection(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                return bogrd.localMoveDirection.normalized;
            }
            return Vector3.zero;
        }

        public override bool CanFire(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
         
                if (bogrd.isSprinting && bogrd.isGrounded) return false;
                else return true;
            }
            return false;
        }

        public override bool IsRunning(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
      
                if (bogrd.isSprinting) return true;
                else return false;
            }
            return false;
        }

        public override void CalculateFootstepsUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.isController)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    if (bogrd.isGrounded)
                    {
                        if (pb.cc.velocity.magnitude > 0.5f)
                        {
                            //We are moving
                            if (Time.time >= bogrd.nextFootstep)
                            {
                                //Set next footstep sound
                                pb.footstepSource.clip = allFootsteps[bogrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogrd.currentMaterial].clips.Length)];
                                //Set footstep source rolloff and distance
                                pb.footstepSource.maxDistance = allFootsteps[bogrd.currentMaterial].maxDistance;
                                pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogrd.currentMaterial].rollOff);
                                //Set volume and time
                                if (bogrd.state == 0)
                                {
                                    if (bogrd.isSprinting) //Sprinting
                                    {
                                        pb.footstepSource.volume = footstepsRunVolume;
                                        bogrd.nextFootstep = Time.time + footstepsRunTime;
                                    }
                                    else //Normal walking
                                    {
                                        pb.footstepSource.volume = footstepsWalkVolume;
                                        bogrd.nextFootstep = Time.time + footstepsWalkTime;
                                    }
                                }
                                else if (bogrd.state == 1) //Crouching
                                {
                                    pb.footstepSource.volume = footstepsCrouchVolume;
                                    bogrd.nextFootstep = Time.time + footstepsCrouchTime;
                                }
                                //Play
                                pb.footstepSource.Play();
                            }
                        }
                    }
                }
            }
            else
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                {
                    BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                    if (bogsrd.isGrounded)
                    {
                        if (bogsrd.velocity.magnitude > 0.5f)
                        {
                            //We are moving
                            if (Time.time >= bogsrd.nextFootstep)
                            {
                                //Set next footstep sound
                                pb.footstepSource.clip = allFootsteps[bogsrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogsrd.currentMaterial].clips.Length)];
                                //Set footstep source rolloff and distance
                                pb.footstepSource.maxDistance = allFootsteps[bogsrd.currentMaterial].maxDistance;
                                pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogsrd.currentMaterial].rollOff);
                                //Set volume and time
                                if (bogsrd.state == 0)
                                {
                                    if (bogsrd.isSprinting) //Sprinting
                                    {
                                        pb.footstepSource.volume = footstepsRunVolume;
                                        bogsrd.nextFootstep = Time.time + footstepsRunTime;
                                    }
                                    else //Normal walking
                                    {
                                        pb.footstepSource.volume = footstepsWalkVolume;
                                        bogsrd.nextFootstep = Time.time + footstepsWalkTime;
                                    }
                                }
                                else if (bogsrd.state == 1) //Crouching
                                {
                                    pb.footstepSource.volume = footstepsCrouchVolume;
                                    bogsrd.nextFootstep = Time.time + footstepsCrouchTime;
                                }
                                //Play
                                pb.footstepSource.Play();
                            }
                        }
                    }
                }
            }
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    //Send velocity
                    stream.SendNext(pb.cc.velocity);
                    //Send grounded
                    stream.SendNext(bogrd.isGrounded);
                    //Send state
                    stream.SendNext(bogrd.state);
                    //Send sprinting
                    stream.SendNext(bogrd.isSprinting);
                    //Send material type
                    stream.SendNext(bogrd.currentMaterial);
                    //Send slow walk animation
                    stream.SendNext(bogrd.playSlowWalkAnimation);
                }
                else
                {
                    //Send dummies
                    //Send velocity
                    stream.SendNext(Vector3.zero);
                    //Send grounded
                    stream.SendNext(true);
                    //Send state
                    stream.SendNext(0);
                    //Send sprinting
                    stream.SendNext(false);
                    //Send material type
                    stream.SendNext(0);
                    //Send slow walk animation
                    stream.SendNext(false);
                }
            }
            else if (stream.IsReading) 
            {
                if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundSyncRuntimeData))
                {
                    pb.customMovementData = new BootsOnGroundSyncRuntimeData();
                }
                BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Read velocity
                bogsrd.velocity = (Vector3)stream.ReceiveNext();
                //Read grounded
                bogsrd.isGrounded = (bool)stream.ReceiveNext();
                //Read state
                bogsrd.state = (int)stream.ReceiveNext();
                //Read isSprinting
                bogsrd.isSprinting = (bool)stream.ReceiveNext();
                //Read material type
                bogsrd.currentMaterial = (string)stream.ReceiveNext();
                //Read slow animation
                bogsrd.playSlowWalkAnimation = (bool)stream.ReceiveNext();
            }
        }

        public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;

                Terrain terrain = hit.collider.GetComponent<Terrain>();
                Kit_TerrainFootstepConverter convertedFootsteps = hit.collider.GetComponent<Kit_TerrainFootstepConverter>();

                if (terrain && convertedFootsteps)
                {
                    //Get texture from helper
                    int texture = GetMainTexture(pb.transform.position, terrain);

                    if (texture < convertedFootsteps.textureToString.Length)
                    {
                        //Convert texture id to 'tag' equivalent
                        if (allFootsteps.ContainsKey(convertedFootsteps.textureToString[texture]))
                        {
                            bogrd.currentMaterial = convertedFootsteps.textureToString[texture];
                        }
                        else
                        {
                            bogrd.currentMaterial = "Concrete";
                        }
                    }
                    else
                    {
                        Debug.LogError("Terrain texture outside of texture to foosteps conversion array! Playing concrete footstep.");
                        bogrd.currentMaterial = "Concrete";
                    }
                }
                else
                {
                    //Use tag if it exists
                    if (allFootsteps.ContainsKey(hit.collider.tag))
                    {
                        bogrd.currentMaterial = hit.collider.tag;
                    }
                    else
                    {
                        bogrd.currentMaterial = "Concrete";
                    }
                }
            }
        }

        public override Vector3 GetVelocity(Kit_PlayerBehaviour pb)
        {
            if (pb.isController)
            {
                return pb.cc.velocity;
            }
            else
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                {
                    BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                    return bogsrd.velocity;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        public override void PlaySound(Kit_PlayerBehaviour pb, int soundID, int id2, int arrayID)
        {
            //Check if sound isnt playing right now
            //Check for id
            if (soundID == 0) //Exhausted
            {
                if (!pb.movementSoundSource.isPlaying)
                {
                    //Set clip
                    pb.movementSoundSource.clip = staminaExhaustedSound[arrayID];
                    //Play
                    pb.movementSoundSource.Play();
                }
            }
            else if (soundID == 1)
            {
                //Set clip
                pb.movementSoundSource.clip = jumpSound[id2].clips[arrayID];
                //Play
                pb.movementSoundSource.Play();
            }
            else if (soundID == 2)
            {
                //Set clip
                pb.movementSoundSource.clip = jumpLandSound[id2].clips[arrayID];
                //Play
                pb.movementSoundSource.Play();
            }
        }

        public override void PlayAnimation(Kit_PlayerBehaviour pb, int id, int id2)
        {
            if (id == 0)
            {
                pb.thirdPersonPlayerModel.anim.SetTrigger(jumpAnimations[Mathf.Clamp(id2, 0, jumpAnimations.Length - 1)]);
            }
        }


        #region Weapon Injection
        public override WeaponsFromPlugin WeaponsToInjectIntoWeaponManager(Kit_PlayerBehaviour player)
        {
            return base.WeaponsToInjectIntoWeaponManager(player);    
        }

        public override void ReportSlotOfInjectedWeapons(Kit_PlayerBehaviour pb, int slotWhereTheyWereInjected)
        {
            //Check if the object is correct
            if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundRuntimeData))
            {
                pb.customMovementData = new BootsOnGroundRuntimeData();
            }

            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
            }
        }
        #endregion

        #region Helpers for Terrain
        public static float[] GetTextureMix(Vector3 worldPos, Terrain terrain)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            // calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            // extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
            for (int n = 0; n < cellMix.Length; ++n)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }

            return cellMix;
        }

        public static int GetMainTexture(Vector3 worldPos, Terrain terrain)
        {
            // returns the zero-based index of the most dominant texture
            // on the main terrain at this world position.
            float[] mix = GetTextureMix(worldPos, terrain);
            float maxMix = 0;
            int maxIndex = 0;
            // loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; ++n)
            {
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }
            return maxIndex;
        }
        #endregion
    }
}
