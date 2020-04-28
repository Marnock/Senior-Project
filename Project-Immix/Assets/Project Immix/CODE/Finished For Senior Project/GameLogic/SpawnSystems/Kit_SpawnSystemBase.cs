using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_SpawnSystemBase : ScriptableObject
    {
        /// <summary>
        /// Returns true if we can spawn at this spawn point
        /// </summary>
      
        public abstract bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Photon.Realtime.Player spawningPlayer);

        public abstract bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Kit_Bot bot);
    }
}
