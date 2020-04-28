using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    [System.Serializable]
    public class BulletMarkMaterials
    {
        public Material[] materials;
    }

    public class Kit_SimpleImpactProcessor : Kit_ImpactParticleProcessor
    {
        [Header("Material impacts")]

        public GameObject[] impactParticles;

  
        public GameObject bulletMarksPrefab;

        public BulletMarkMaterials[] bulletMarksMaterials;


        public float bulletMarksNormalOffset;

   
        public float bulletMarksLifetime = 60f;

        [Header("Enemy impacts")]
        public GameObject[] enemyImpactParticles;

        public override void ProcessImpact(Vector3 pos, Vector3 normal, int materialType, Transform parent = null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject go = Instantiate(impactParticles[materialType], pos, rot); //Instantiate appropriate particle
            if (parent) go.transform.parent = parent; //Set parent if we have one

            //Bullet marks
            GameObject bm = Instantiate(bulletMarksPrefab, pos + normal * bulletMarksNormalOffset, rot);
            //The instantiated GO should destroy itself
            //Set material
            bm.GetComponent<Kit_BulletMarks>().SetMaterial(bulletMarksMaterials[materialType].materials[Random.Range(0, bulletMarksMaterials[materialType].materials.Length)], bulletMarksLifetime);
            if (parent) bm.transform.parent = parent; //Set parent if we have one
        }

        public override void ProcessEnemyImpact(Vector3 pos, Vector3 normal)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            Instantiate(enemyImpactParticles[Random.Range(0, enemyImpactParticles.Length)], pos, rot); //Instantiate appropriate particle
        }
    }
}
