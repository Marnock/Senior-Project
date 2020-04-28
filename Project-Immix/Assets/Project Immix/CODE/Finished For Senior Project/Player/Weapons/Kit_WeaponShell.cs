using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{

    [RequireComponent(typeof(AudioSource))]
    public class Kit_WeaponShell : MonoBehaviour
    {

        public float lifeTime = 15f;


        public float impactSoundThreshold = 2f;
        public AudioClip[] impactSounds;


        // Use this for initialization
        void Start()
        {
            //Automatically destroy this gameobject after lifetime is over
            Destroy(gameObject, lifeTime);
        }

        void OnCollisionEnter(Collision collision)
        {
            //Check if we have sounds assigned
            if (impactSounds.Length > 0)
            {
                //Check magnitude
                if (collision.relativeVelocity.magnitude > impactSoundThreshold)
                {
                    //Play random sound
                    GetComponent<AudioSource>().clip = impactSounds[Random.Range(0, impactSounds.Length)];
                    GetComponent<AudioSource>().Play();
                }
            }
        }
    }
}
