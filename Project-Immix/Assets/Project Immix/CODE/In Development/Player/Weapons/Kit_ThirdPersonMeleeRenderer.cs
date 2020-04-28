using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_ThirdPersonMeleeRenderer : MonoBehaviour
        {

            public Renderer[] allWeaponRenderers;

            [Header("Inverse Kinematics")]
            public Transform leftHandIK;

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Third person melee renderer from " + gameObject.name + " at index " + i + " not assigned.");
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
                }
            }

            public bool shadowsOnly
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (allWeaponRenderers[i].shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) return false;
                    }
                    return true;
                }
                set
                {
                    if (value)
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        }
                    }
                    else
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        }
                    }
                }
            }
        }
    }
}