using ImmixKit.Weapons;
using System.Linq;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class ModernBulletSetupData
        {
   
            public float damage = 30f;
   
            public float gravityMultiplier = 1f;
      
            public float speed = 200f;
    
            public bool penetrationEnabled = true;
  
            public int penetrationValue = 4;

            /// <summary>
            /// Direction of the bullet (spread already applied.)
            /// </summary>
            public Vector3 direction;
            /// <summary>
            /// Force to apply to ragdoll (if hit)
            /// </summary>
            public float ragdollForce = 500f;
            /// <summary>
            /// ID of the gun this bullet was fired with
            /// </summary>
            public int gameGunID;
            /// <summary>
            /// Lifetime of the bullet (in s)
            /// </summary>
            public float bulletLifeTime = 10f;
            /// <summary>
            /// Should the bullet parent itself to its hit thing and stay alive? Useful for things like nails and arrows.
            /// </summary>
            public bool staysAliveAfterDeath = false;
            /// <summary>
            /// If bullet stays alive after death, this is how long
            /// </summary>
            public float staysAliveAfterDeathTime = 10f;
            /// <summary>
            /// Mask of things we can hit
            /// </summary>
            public LayerMask mask;
    
            public Vector3 shotFromPosition;
            /// <summary>
            /// Was this fired locally (should apply damage)
            /// </summary>
            public bool isLocal;
    
            public Kit_PlayerBehaviour localOwner;
            /// <summary>
            /// ID of owner
            /// </summary>
            public int ownerID;
            /// <summary>
            /// Is the owner a bot?
            /// </summary>
            public bool ownerIsBot;
        }

        public class Kit_ModernBullet : Kit_BulletBase
        {
            #region Runtime
            /// <summary>
            /// Settings received from weapon script
            /// </summary>
            private ModernBulletSetupData settings;
   
            private Kit_IngameMain main;
 
            private Vector3 velocity;
 
            private Vector3 newPosition;
     
            private Vector3 oldPosition;
      
            private Vector3 tempDir;
    
            private float tempDistance;
     
            public RaycastHit tempHit;
            private bool tempPenetrated;
         
            private bool tempDestroyNext;
           
            private int lastHitID;
           
            private int lastHitIDBack;
                     private float bulletExistTime;
            #endregion

            public override void Setup(Kit_IngameMain newMain, Kit_ModernWeaponScript data, Kit_PlayerBehaviour pb, Vector3 dir)
            {
                //Setup
                ModernBulletSetupData mbsd = new ModernBulletSetupData();
                //Setup data
                mbsd.damage = data.baseDamage;
                mbsd.gravityMultiplier = data.bulletGravityMultiplier;
                mbsd.speed = data.bulletSpeed;
                mbsd.penetrationEnabled = data.bulletsPenetrationEnabled;
                mbsd.penetrationValue = data.bulletsPenetrationForce + 1;
                mbsd.direction = dir;
                mbsd.ragdollForce = data.ragdollForce;
                mbsd.gameGunID = data.gameGunID;
                mbsd.bulletLifeTime = data.bulletLifeTime;
                mbsd.staysAliveAfterDeath = data.bulletStaysAliveAfterDeath;
                mbsd.staysAliveAfterDeathTime = data.bulletStaysAliveAfterDeathTime;
                mbsd.mask = pb.weaponHitLayers;
                mbsd.shotFromPosition = pb.transform.position;
                mbsd.isLocal = pb.photonView.IsMine;
                mbsd.localOwner = pb;
                mbsd.ownerIsBot = pb.isBot;
                if (pb.isBot)
                    mbsd.ownerID = pb.botId;
                else
                    mbsd.ownerID = pb.photonView.Owner.ActorNumber;

                //Set main
                main = newMain;
                //Get settings
                settings = mbsd;
                //Set position default
                newPosition = transform.position;
                oldPosition = transform.position;
                velocity = mbsd.speed * transform.forward;
            }

            #region Unity Calls
            void Update()
            {
                //Advance
                newPosition += (velocity + settings.direction + (Physics.gravity * settings.gravityMultiplier)) * Time.deltaTime;
                //Calculate direction
                tempDir = newPosition - oldPosition;
                //Calculate travelled distance
                tempDistance = tempDir.magnitude;
                //Divide
                tempDir /= tempDistance;
                //Check if we actually travelled
                if (tempDistance > 0f)
                {
                    RaycastHit[] hits = Physics.RaycastAll(oldPosition, tempDir, tempDistance, settings.mask);
                    hits = hits.OrderBy(h => h.distance).ToArray();
                    for (int i = 0; i < hits.Length; i++)
                    {
                        //Check if we hit ourselves
                        if (settings.localOwner && hits[i].transform.root != settings.localOwner.transform.root)
                        {
                            //Check if we hit last object again
                            if (hits[i].collider.GetInstanceID() != lastHitID)
                            {
                                //Set new position to hit position
                                newPosition = hits[i].point;
                                //Call function
                                OnHit(hits[i], tempDir);
                                break;
                            }
                        }
                    }

                    if (tempPenetrated)
                    {
                        //There must be something back hit
                        if (Physics.Raycast(newPosition, -tempDir, out tempHit, tempDistance, settings.mask))
                        {
                            //Check if we hit ourselves
                            if (settings.localOwner && tempHit.transform.root != settings.localOwner.transform.root)
                            {
                                //Check if we hit same object again
                                if (lastHitIDBack != tempHit.collider.GetInstanceID())
                                {
                                    OnHit(tempHit);
                                }
                            }
                        }
                    }
                }

                //Check if bullet penetration is over
                if (settings.penetrationValue <= 0)
                {
                    if (settings.staysAliveAfterDeath)
                    {
                        enabled = false;
                        if (settings.staysAliveAfterDeathTime > 0f)
                        {
                            Destroy(gameObject, settings.staysAliveAfterDeathTime);
                        }
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }

                if (tempDestroyNext)
                {
                    if (settings.staysAliveAfterDeath)
                    {
                        enabled = false;
                        if (settings.staysAliveAfterDeathTime > 0f)
                        {
                            Destroy(gameObject, settings.staysAliveAfterDeathTime);
                        }
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }

                //Check if we should destroy
                bulletExistTime += Time.deltaTime;
                if (bulletExistTime > settings.bulletLifeTime && enabled) Destroy(gameObject);
            }

            void LateUpdate()
            {
                //Set position
                oldPosition = transform.position;
                //Move
                transform.position = newPosition;
            }
            #endregion

            #region Custom Calls
            void OnHit(RaycastHit hit, Vector3 dir)
            {
                if (settings.penetrationEnabled)
                {
                    //Check if we can penetrate
                    Kit_PenetrateableObject penetration = hit.collider.GetComponent<Kit_PenetrateableObject>();
                    if (penetration)
                    {
                        //Can we penetrate?
                        if (settings.penetrationValue >= penetration.cost)
                        {
                            //Subtract
                            settings.penetrationValue -= penetration.cost;
                            tempPenetrated = true;

                            if (settings.penetrationValue <= 0)
                            {
                                if (settings.staysAliveAfterDeath)
                                {
                                    //Parent
                                    transform.SetParent(hit.collider.transform, true);
                                    transform.position = hit.point;
                                }
                            }
                        }
                        else
                        {
                            //We can't. Destroy next
                            tempDestroyNext = true;

                            if (settings.staysAliveAfterDeath)
                            {
                                //Parent
                                transform.SetParent(hit.collider.transform, true);
                                transform.position = hit.point;
                            }
                        }
                    }
                    else
                    {
                        //Nothing to penetrate. Destroy next
                        tempDestroyNext = true;

                        if (settings.staysAliveAfterDeath)
                        {
                            //Parent
                            transform.SetParent(hit.collider.transform, true);
                            transform.position = hit.point;
                        }
                    }
                }
                else
                {
                    //Nothing to penetrate. Destroy next
                    tempDestroyNext = true;

                    if (settings.staysAliveAfterDeath)
                    {
                        //Parent
                        transform.SetParent(hit.collider.transform, true);
                        transform.position = hit.point;
                    }
                }

                //Check if we hit a player
                if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>())
                {
                    Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                    if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                    {
                        if (settings.isLocal)
                        {
                            Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                            //First check if we can actually damage that player
                            if ((settings.localOwner && main.currentGameModeBehaviour.ArePlayersEnemies(settings.localOwner, hitPb)) || (!settings.localOwner && main.currentGameModeBehaviour.ArePlayersEnemies(main, settings.ownerID, settings.ownerIsBot, hitPb, false)))
                            {
                                //Check if he has spawn protection
                                if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                {
                                    //Apply local damage, sample damage dropoff via distance
                                    hitPb.LocalDamage(settings.damage * pdm.damageMultiplier, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, hit.point, pdm.ragdollId, settings.ownerIsBot, settings.ownerID);
                                    if (!settings.ownerIsBot)
                                    {
                                        //Since we hit a player, show the hitmarker
                                        main.hud.DisplayHitmarker();
                                    }
                                }
                                else if (!settings.ownerIsBot)
                                {
                                    //We hit a player but his spawn protection is active
                                    main.hud.DisplayHitmarkerSpawnProtected();
                                }
                            }
                        }
                        //Send to hit processor
                        main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                    }
                }
                else
                {
                    //Proceed to hit processor
                    if (hit.collider.CompareTag("Dirt")) //Check for dirt
                    {
                        //Call
                        main.impactProcessor.ProcessImpact(hit.point, hit.normal, 1, hit.transform);
                    }
                    else if (hit.collider.CompareTag("Metal")) //Check for metal
                    {
                        //Call
                        main.impactProcessor.ProcessImpact(hit.point, hit.normal, 2, hit.transform);
                    }
                    else if (hit.collider.CompareTag("Wood")) //Check for wood
                    {
                        //Call
                        main.impactProcessor.ProcessImpact(hit.point, hit.normal, 3, hit.transform);
                    }
                    else if (hit.collider.CompareTag("Blood")) //Check for blood
                    {
                        //Call
                        main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                    }
                    else //Else use concrete
                    {
                        //Call
                        main.impactProcessor.ProcessImpact(hit.point, hit.normal, 0, hit.transform);
                    }

                    if (hit.collider.GetComponentInParent<IKitDamageable>() != null)
                    {
                        if (hit.collider.GetComponentInParent<IKitDamageable>().LocalDamage(settings.damage, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, hit.point, settings.ownerIsBot, settings.ownerID))
                        {

                            if (!settings.ownerIsBot)
                            {
                                //Since we hit a player, show the hitmarker
                                main.hud.DisplayHitmarker();
                            }
                        }
                    }
                }

                //Assign ID
                lastHitID = hit.collider.GetInstanceID();
            }

            void OnHit(RaycastHit hit)
            {
                //Proceed to hit processor
                if (hit.collider.CompareTag("Dirt")) //Check for dirt
                {
                    //Call
                    main.impactProcessor.ProcessImpact(hit.point, hit.normal, 1, hit.transform);
                }
                else if (hit.collider.CompareTag("Metal")) //Check for metal
                {
                    //Call
                    main.impactProcessor.ProcessImpact(hit.point, hit.normal, 2, hit.transform);
                }
                else if (hit.collider.CompareTag("Wood")) //Check for wood
                {
                    //Call
                    main.impactProcessor.ProcessImpact(hit.point, hit.normal, 3, hit.transform);
                }
                else if (hit.collider.CompareTag("Blood")) //Check for blood
                {
                    //Call
                    main.impactProcessor.ProcessEnemyImpact(hit.point, hit.normal);
                }
                else //Else use concrete
                {
                    //Call
                    main.impactProcessor.ProcessImpact(hit.point, hit.normal, 0, hit.transform);
                }

                //Assign ID
                lastHitIDBack = hit.collider.GetInstanceID();
            }
            #endregion
        }
    }
}