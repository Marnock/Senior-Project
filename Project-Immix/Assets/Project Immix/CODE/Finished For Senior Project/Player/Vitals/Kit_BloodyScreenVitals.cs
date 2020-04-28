using System;
using UnityEngine;

namespace ImmixKit
{
    public class BloodyScreenVitalsRuntimeData
    {
        public float hitPoints;
        public float lastHit;
        /// <summary>
        /// For displaying the bloody screen
        /// </summary>
        public float hitAlpha;
    }


    public class Kit_BloodyScreenVitals : Kit_VitalsBase
    {
        public float simpleBloodyScreenTime = 3f;
    
        public float timeUntilHealthIsRegenerated = 5f;
        public float healthRegenerationSpeed = 25f;

 
        public bool hitReactionEnabled = true;
   
        public float hitReactionsIntensity = 1.2f;
      
        public float hitReactionsReturnSpeed = 5f;
    
        public bool displayHealthLikeSimple;

 
        public int fallDamageSoundCatID;
       
        public int outOfMapSoundCatID;

        public override void ApplyHeal(Kit_PlayerBehaviour pb, float heal)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints + heal, 0, 100f);
            }
        }

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotFrom)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        //Hit reactions
                        if (hitReactionEnabled)
                        {
                            Vector3 dir = (pb.playerCameraTransform.InverseTransformDirection(Vector3.Cross(pb.playerCameraTransform.forward, pb.transform.position - shotFrom))).normalized * hitReactionsIntensity;
                            dir *= Mathf.Clamp(dmg / 30f, 0.3f, 1f);

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraHitReactionsTransform, dir, 0.1f));

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.weaponsHitReactions, dir * 2f, 0.1f));
                        }
                        if (!pb.isBot)
                        {
                            //Set damage effect
                            vrd.hitAlpha = 2f;
                        }
                        //Check for death
                        if (vrd.hitPoints <= 0)
                        {
                            //Call the die function on pb
                            pb.Die(botShot, idWhoShot, gunID);
                        }
                    }
                }
            }
        }

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, string deathCause, Vector3 shotFrom)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        //Hit reactions
                        if (hitReactionEnabled)
                        {
                            Vector3 dir = (pb.playerCameraTransform.InverseTransformDirection(Vector3.Cross(pb.playerCameraTransform.forward, pb.transform.position - shotFrom))).normalized * hitReactionsIntensity;
                            dir *= Mathf.Clamp(dmg / 30f, 0.3f, 1f);

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraHitReactionsTransform, dir, 0.1f));

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.weaponsHitReactions, dir * 2f, 0.1f));
                        }
                        if (!pb.isBot)
                        {
                            //Set damage effect
                            vrd.hitAlpha = 2f;
                        }
                        //Check for death
                        if (vrd.hitPoints <= 0)
                        {
                            //Call the die function on pb
                            pb.Die(botShot, idWhoShot, deathCause);
                        }
                    }
                }
            }
        }

        public override void ApplyFallDamage(Kit_PlayerBehaviour pb, float dmg)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    pb.deathSoundCategory = fallDamageSoundCatID;
                    if (!pb.isBot)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                        //Set damage effect
                        vrd.hitAlpha = 2f;
                    }
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Check for death
                    if (vrd.hitPoints <= 0)
                    {
                        //Reset player force
                        pb.ragdollForce = 0f;
                        //Call the die function on pb
                        pb.Die(-1);
                    }
                }
            }
        }

        public override void ApplyEnvironmentalDamage(Kit_PlayerBehaviour pb, float dmg, int deathSoundCategory)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    pb.deathSoundCategory = deathSoundCategory;
                    if (!pb.isBot)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                        //Set damage effect
                        vrd.hitAlpha = 2f;
                    }
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Check for death
                    if (vrd.hitPoints <= 0)
                    {
                        //Reset player force
                        pb.ragdollForce = 0f;
                        //Call the die function on pb
                        pb.Die(-3);
                    }
                }
            }
        }

        public override void Suicide(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                //Reset player force
                pb.ragdollForce = 0f;
                //Call the die function on pb
                pb.Die(-3);
            }
        }

        public override void CustomUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Clamp
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints, 0f, 100f);

                //Decrease hit alpha
                if (vrd.hitAlpha > 0)
                {
                    vrd.hitAlpha -= (Time.deltaTime * 2) / simpleBloodyScreenTime;
                }

                if (!pb.isBot)
                {
                    if (displayHealthLikeSimple)
                    {
                        //Update hud
                        pb.main.hud.DisplayHealth(vrd.hitPoints);
                        pb.main.hud.DisplayHurtState(vrd.hitAlpha);
                    }
                    else
                    {
                        //Update hud with negative values, so its hidden
                        pb.main.hud.DisplayHealth(-1);
                        //Negative values should hide it
                        pb.main.hud.DisplayHurtState(1 - vrd.hitPoints / 100f);
                    }
                }

                //Return hit reactions
                if (hitReactionEnabled)
                {
                    pb.playerCameraHitReactionsTransform.localRotation = Quaternion.Slerp(pb.playerCameraHitReactionsTransform.localRotation, Quaternion.identity, Time.deltaTime * hitReactionsReturnSpeed);
                    pb.weaponsHitReactions.localRotation = Quaternion.Slerp(pb.weaponsHitReactions.localRotation, Quaternion.identity, Time.deltaTime * hitReactionsReturnSpeed);
                }

                if (vrd.hitPoints < 100f)
                {
                    //Check for hp regeneration
                    if (Time.time > vrd.lastHit + timeUntilHealthIsRegenerated)
                    {
                        vrd.hitPoints += Time.deltaTime * healthRegenerationSpeed;
                    }
                }

                //Check if we are lower than death threshold
                if (pb.transform.position.y <= pb.main.mapDeathThreshold)
                {
                    pb.deathSoundCategory = outOfMapSoundCatID;
                    pb.Die(-1);
                }
            }
        }

        public override void Setup(Kit_PlayerBehaviour pb)
        {
            //Create runtime data
            BloodyScreenVitalsRuntimeData vrd = new BloodyScreenVitalsRuntimeData();
            //Set standard values
            vrd.hitPoints = 100f;
            //Assign it
            pb.customVitalsData = vrd;
        }
    }
}
