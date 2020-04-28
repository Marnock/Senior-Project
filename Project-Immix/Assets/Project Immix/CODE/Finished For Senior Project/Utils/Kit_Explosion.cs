using ImmixKit;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    public class Kit_Explosion : MonoBehaviour
    {
     
        public float radius = 5f;
      
        public LayerMask layers;
       
        public LayerMask linecastLayers;
    
        public float ragdollForce;
     
        public float maxDamage = 150f;
   
        public float minDamage = 50f;
   
        public float liveTime = 5f;
     
        public AudioSource source;
  
        public ParticleSystem system;
      
        public AudioClip[] clips;

        public float shakeAmount;
     
        public float shakeDuration;
   
        public float shakeDistance = 10f;

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

            if (shakeAmount > 0f && shakeDuration > 0f)
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                if (main)
                {
                    float dist = Vector3.Distance(main.mainCamera.transform.position, transform.position);
                    if (dist < shakeDistance)
                    {
                        main.cameraShake.ShakeCamera(Mathf.Lerp(shakeAmount, 0f, dist / shakeDistance), Mathf.Lerp(shakeDuration, 0f, dist / shakeDistance));
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
                                    player.LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, adm.transform.position) / radius), gunID, transform.position, adm.transform.position - transform.position, ragdollForce, transform.position, adm.ragdollId, botShot, idWhoShot);

                                    if (!botShot)
                                    {
                                        PhotonNetwork.RaiseEvent(7, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                                    }
                                }
                            }
                        }
                    }
                    else if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>() != null)
                    {
                        if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, affectedByExplosion[i].transform.position) / radius), gunID, transform.position, affectedByExplosion[i].transform.position - transform.position, ragdollForce, transform.position, botShot, idWhoShot))
                        {

                            if (!botShot)
                            {
                                PhotonNetwork.RaiseEvent(7, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                            }
                        }
                    }
                }
            }

            Destroy(gameObject, liveTime);
        }

        public void Explode(bool doDamage, bool botShot, int idWhoShot, string cause)
        {
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();

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
                                    player.LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, adm.transform.position) / radius), cause, transform.position, adm.transform.position - transform.position, ragdollForce, transform.position, adm.ragdollId, botShot, idWhoShot);

                                    if (!botShot)
                                    {
                                        PhotonNetwork.RaiseEvent(7, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                                    }
                                }
                            }
                        }
                    }
                    else if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>() != null)
                    {
                        if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, affectedByExplosion[i].transform.position) / radius), 0, transform.position, affectedByExplosion[i].transform.position - transform.position, ragdollForce, transform.position, botShot, idWhoShot))
                        {

                            if (!botShot)
                            {
                                PhotonNetwork.RaiseEvent(7, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                            }
                        }
                    }
                }
            }

            Destroy(gameObject, liveTime);
        }
    }
}