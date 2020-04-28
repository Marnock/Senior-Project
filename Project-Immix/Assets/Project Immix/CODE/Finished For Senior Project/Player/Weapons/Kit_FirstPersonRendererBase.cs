using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_FirstPersonRendererBase : MonoBehaviour
        {
            /// <summary>
            /// The weapon animator
            /// </summary>
            public Animator anim;

            public Renderer[] allWeaponRenderers;

            public Renderer[] hideInCustomiazionMenu;
        }
    }
}
