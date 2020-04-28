using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public abstract class Kit_BulletBase : MonoBehaviour
        {
            /// <summary>
            /// Called after the bullet was instantiated by weapon script
            /// </summary>
            public abstract void Setup(Kit_IngameMain main, Kit_ModernWeaponScript settings, Kit_PlayerBehaviour pb, Vector3 dir);
        }
    }
}