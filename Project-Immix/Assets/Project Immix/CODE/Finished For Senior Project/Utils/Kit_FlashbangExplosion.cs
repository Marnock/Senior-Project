using ImmixKit;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    public class Kit_FlashbangExplosion : MonoBehaviour
    {
     
        public float radius = 5f;
  
        public float ragdollForce = 50f;
  
        public LayerMask layers;
    
        public LayerMask linecastLayers;
   
        public float maxTime = 5f;
     
        public float minTime = 1f;
   
        public float liveTime = 5f;
       
        public AudioSource source;
      
        public ParticleSystem system;
  
        public AudioClip[] clips;

        void Start()
        {
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();
            system.Play(true);
            Collider[] affectedByExplosion = Physics.OverlapSphere(transform.position, radius, layers.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < affectedByExplosion.Length; i++)
            {
                if (!Physics.Linecast(transform.position, affectedByExplosion[i].transform.position, linecastLayers))
                {
                    if (affectedByExplosion[i].GetComponent<Rigidbody>())
                    {
                        if (affectedByExplosion[i].GetComponent<Kit_ExplosionRigidbody>())
                        {
                            Kit_ExplosionRigidbody body = affectedByExplosion[i].GetComponent<Kit_ExplosionRigidbody>();
                            StartCoroutine(ApplyExplosionForceNetworked(body, ragdollForce, transform.position, radius));
                        }
                        else
                        {
                            affectedByExplosion[i].GetComponent<Rigidbody>().AddExplosionForce(ragdollForce, transform.position, radius);
                        }
                    }
                }
            }
        }

        public IEnumerator ApplyExplosionForceNetworked(Kit_ExplosionRigidbody body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
        {
            float time = 0f;
            while (body && body.body.isKinematic && time < 1f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            if (body && !body.body.isKinematic)
            {
                body.body.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }
        }

        public void Explode(bool doDamage, bool botShot, int idWhoShot, int gunID)
        {
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();

            List<Kit_PlayerBehaviour> blindedPlayers = new List<Kit_PlayerBehaviour>();

            Collider[] affectedByExplosion = Physics.OverlapSphere(transform.position, radius, layers.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < affectedByExplosion.Length; i++)
            {
                if (doDamage)
                {
                    if (affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>())
                    {
                        Kit_PlayerDamageMultiplier adm = affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>();
                        if (affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            Kit_PlayerBehaviour player = affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                            if (!Physics.Linecast(transform.position, player.playerCameraTransform.position, linecastLayers))
                            {
                                if (main.currentGameModeBehaviour.ArePlayersEnemies(main, idWhoShot, botShot, player, true))
                                {
                                    if (!blindedPlayers.Contains(player))
                                    {
                                        blindedPlayers.Add(player);
                                        //Blind that player muhahaha!!!
                                        player.LocalBlind(Mathf.SmoothStep(maxTime, minTime, Vector3.Distance(transform.position, adm.transform.position) / radius), gunID, transform.position, botShot, idWhoShot);

                                        if (!botShot && idWhoShot == PhotonNetwork.LocalPlayer.ActorNumber && main)
                                        {
                                            main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Destroy(gameObject, liveTime);
        }
    }
}