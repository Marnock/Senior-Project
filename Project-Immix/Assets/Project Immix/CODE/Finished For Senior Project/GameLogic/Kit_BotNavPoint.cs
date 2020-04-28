using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This class marks a bot navigation point in the scene
    /// </summary>
    public class Kit_BotNavPoint : MonoBehaviour
    {
        /// <summary>
        /// ID of this spawn point
        /// </summary>
        public int navPointGroupID;
        public Kit_GameModeBase[] gameModes;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            //Draw a cube to indicate
            Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
        }
    }
}
