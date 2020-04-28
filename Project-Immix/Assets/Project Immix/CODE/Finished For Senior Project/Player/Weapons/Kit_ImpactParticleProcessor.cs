using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This is used for the instantiation of impact particles / bullet marks
    /// </summary>
    public abstract class Kit_ImpactParticleProcessor : ScriptableObject
    {

        public abstract void ProcessImpact(Vector3 pos, Vector3 normal, int materialType, Transform parentObject = null);

        public abstract void ProcessEnemyImpact(Vector3 pos, Vector3 normal);
    }
}