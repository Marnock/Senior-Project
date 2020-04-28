using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This holds information for the hitbox
    /// </summary>
    public class Kit_PlayerDamageMultiplier : MonoBehaviour
    {
 
        public float damageMultiplier = 1f;

        /// <summary>
        /// Which ID does this part of the ragdoll have?
        /// </summary>
        public int ragdollId;
    }
}
