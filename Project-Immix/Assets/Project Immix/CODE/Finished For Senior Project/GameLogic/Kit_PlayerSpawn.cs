using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This class marks a spawn in the scene
    /// </summary>
    public class Kit_PlayerSpawn : MonoBehaviour
    {
    
        public int spawnGroupID = 0;
   

        public Kit_GameModeBase[] gameModes;

        void OnDrawGizmos()
        {
            //Color the spawn based on the group id
            if (spawnGroupID == 0)
                Gizmos.color = Color.black;
            else if (spawnGroupID == 1)
                Gizmos.color = Color.blue;
            else if (spawnGroupID == 2)
                Gizmos.color = Color.cyan;
            else if (spawnGroupID == 3)
                Gizmos.color = Color.gray;
            else if (spawnGroupID == 4)
                Gizmos.color = Color.green;
            else if (spawnGroupID == 5)
                Gizmos.color = Color.magenta;
            else if (spawnGroupID == 6)
                Gizmos.color = Color.red;
            else if (spawnGroupID == 7)
                Gizmos.color = Color.white;
            else
                Gizmos.color = Color.yellow;

            //Draw a cube to indicate
            Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
        }
    }
}
