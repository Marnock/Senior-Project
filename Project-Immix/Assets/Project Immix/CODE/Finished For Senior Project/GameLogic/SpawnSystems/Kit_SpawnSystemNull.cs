using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This spawn system will always return true
    /// </summary>
    public class Kit_SpawnSystemNull : Kit_SpawnSystemBase
    {
        public override bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Photon.Realtime.Player spawningPlayer)
        {
            return true;
        }

        public override bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Kit_Bot bot)
        {
            return true;
        }
    }
}