using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_MeleeRenderer : MonoBehaviour
        {
            /// <summary>
            /// The weapon animator
            /// </summary>
            public Animator anim;

            public Renderer[] allWeaponRenderers;

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

            /// <summary>
            /// Visibility state of the weapon
            /// </summary>
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
                }
            }
        }
    }
}