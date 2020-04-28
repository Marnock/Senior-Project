using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public enum CameraAnimationType { Copy, LookAt }

        public class Kit_WeaponRenderer : MonoBehaviour
        {
      
            public Animator anim;

            public Renderer[] allWeaponRenderers;
   
            public Renderer[] hideInCustomiazionMenu;

            [Header("Shell Ejection")]
 
            public Transform shellEjectTransform;

            [Header("Muzzle Flash")]
 
            public ParticleSystem muzzleFlash;

            [Header("Aiming")]

            public Vector3 aimingPos;

            public Vector3 aimingRot;

            public float aimingFov = 40f;

            public bool aimingFullscreen;

            [Header("Run position / rotation")]
 
            public bool useRunPosRot;
   
            public Vector3 runPos;
   
            public Vector3 runRot;

            public float runSmooth = 3f;

            [Header("Camera Animation")]
            public bool cameraAnimationEnabled;

            public CameraAnimationType cameraAnimationType;

            public Transform cameraAnimationBone;

            public Transform cameraAnimationTarget;

            public Vector3 cameraAnimationReferenceRotation;

            [Header("Attachments")]
            public GameObject attachmentsRoot;
   
            public AttachmentSlot[] attachmentSlots;

            [HideInInspector]
            public Kit_AttachmentBehaviour[] cachedSyncAttachments;

            [HideInInspector]
            public Kit_AttachmentBehaviour[] cachedInteractionAttachments;

            [HideInInspector]

            public int[] cachedAttachments;

            #region Cached values
            [HideInInspector]
            public Vector3 cachedAimingPos;
   
            [HideInInspector]
            public Vector3 cachedAimingRot;
            [HideInInspector]
    
            public float cachedAimingFov = 40f;
  
            [HideInInspector]
            public bool cachedUseFullscreenScope;
            [HideInInspector]
            public bool cachedMuzzleFlashEnabled;

            [HideInInspector]
            public AudioClip cachedFireSound;
   
            [HideInInspector]
            public AudioClip cachedFireSoundThirdPerson;

            [HideInInspector]
            public float cachedFireSoundThirdPersonMaxRange = 300f;
      
            [HideInInspector]
            public AnimationCurve cachedFireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);
            #endregion

            [Header("Loadout")]
    
            public Vector3 customizationMenuOffset;

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Weapon renderer from " + gameObject.name + " at index " + i + " not assigned.");
                    }
                }
            }
#endif

            public bool visible
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (!allWeaponRenderers[i].enabled) return false;
                    }
                    return true;
                }
                set
                {
                    //Set renderers
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        allWeaponRenderers[i].enabled = value;
                    }

                    //Loop through all slots
                    for (int i = 0; i < cachedAttachments.Length; i++)
                    {
                        if (i < attachmentSlots.Length)
                        {
                            //Loop through all attachments for that slot
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                //Check if this attachment is enabled
                                if (o == cachedAttachments[i])
                                {
                                    //Tell the behaviours they are active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        //Check what it is
                                        if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentRenderer))
                                        {
                                            Kit_AttachmentRenderer ar = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentRenderer;
                                            for (int a = 0; a < ar.renderersToActivate.Length; a++)
                                            {
                                                ar.renderersToActivate[a].enabled = value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Something must have gone wrong with the attachments. Enabled attachments is longer than all slots.");
                        }
                    }

                  
                }
            }

            public void SetAttachments(int[] enabledAttachments, Kit_ModernWeaponScript ws, Kit_PlayerBehaviour pb)
            {
                //Set default cached values
                cachedAttachments = enabledAttachments;
                cachedAimingPos = aimingPos;
                cachedAimingRot = aimingRot;
                cachedAimingFov = aimingFov;
                cachedUseFullscreenScope = aimingFullscreen;
                cachedMuzzleFlashEnabled = true;
                cachedFireSound = ws.fireSound;
                cachedFireSoundThirdPerson = ws.fireSoundThirdPerson;
                cachedFireSoundThirdPersonMaxRange = ws.fireSoundThirdPersonMaxRange;
                cachedFireSoundThirdPersonRolloff = ws.fireSoundThirdPersonRolloff;;
            }
        }
    }
}