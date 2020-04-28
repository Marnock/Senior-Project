using ImmixKit.Weapons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// A helper class to use coroutines from scriptable objects. The coroutines need to do checks if the instances supplied still exist though otherwise it might throw errors
    /// </summary>
    [DisallowMultipleComponent]
    public class Kit_ScriptableObjectCoroutineHelper : MonoBehaviour
    {
        public static Kit_ScriptableObjectCoroutineHelper instance;

        void Awake()
        {
            //The object should only exist once. Assign the instance
            instance = this;
        }

        public IEnumerator Kick(Transform trans, Vector3 target, float time)
        {
            Quaternion startRotation = trans.localRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(target);
            float rate = 1.0f / time;
            float t = 0.0f;
            while (trans && t < 1.0f)
            {
                //Advance
                t += Time.deltaTime * rate;
                //Slerp to it 
                trans.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
        }

        public IEnumerator WeaponApplyRecoil(Kit_ModernWeaponScript behaviour, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, Vector2 target, float time)
        {
            Quaternion startRotation = pb.recoilApplyRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(target.y, -target.x, 0f);
            float rate = 1.0f / time;
            float t = 0.0f;
            while (pb && behaviour && data != null && t < 1.0f)
            {
                //Advance
                t += Time.deltaTime * rate;
                //Slerp to it 
                pb.recoilApplyRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
        }

        public IEnumerator MeleeExecutePrimaryStab(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
            if (values.primaryAttackSettings.stabWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.primaryAttackSettings.stabWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.primaryAttackSettings.stabWindupAnimationName);
                        }
 
                    }
                }

                //Call network
                pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 0, 0);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 0);

                data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.stabWindupTime + values.primaryAttackSettings.stabHitTime + values.primaryAttackSettings.stabMissTime;

                //Wait
                yield return new WaitForSeconds(values.primaryAttackSettings.stabWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.primaryAttackSettings.stabReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.primaryAttackSettings.stabHalfExtents, dir, Quaternion.LookRotation(dir), values.primaryAttackSettings.stabReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.primaryAttackSettings.stabPenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(values.primaryAttackSettings.stabDamage * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.primaryAttackSettings.stabRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);
                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }

                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(values.primaryAttackSettings.stabDamage, values.gameGunID, pb.transform.position, dir, values.primaryAttackSettings.stabRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.primaryAttackSettings.stabAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.stabAnimationMissName);
                            }
 
                        }
                        //Play sound
                        if (values.primaryAttackSettings.stabMissSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.stabMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 1, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 1);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.stabMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.primaryAttackSettings.stabAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.stabAnimationHitName);
                            }
    
                        }
                        //Play sound
                        if (values.primaryAttackSettings.stabHitSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.stabHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 2, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 2);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.stabHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.primaryAttackSettings.stabAnimationHitObjectName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.stabAnimationHitObjectName);
                            }
 
                        }
                        //Play sound
                        if (values.primaryAttackSettings.stabHitObjectSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.stabHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 3, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 3);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.stabHitObjectTime;
                }

                yield return new WaitForSeconds(values.primaryAttackSettings.stabHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }

        public IEnumerator MeleeExecutePrimaryCharge(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {

            if (values.primaryAttackSettings.chargeWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.primaryAttackSettings.chargeWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.primaryAttackSettings.chargeWindupAnimationName);
                        }
       
                    }
                }

                data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.chargeWindupTime + values.primaryAttackSettings.chargeHitTime + values.primaryAttackSettings.chargeMissTime;

                //Call network
                pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 1, 0);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 1);

                //Wait
                yield return new WaitForSeconds(values.primaryAttackSettings.chargeWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.primaryAttackSettings.chargeReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.primaryAttackSettings.chargeHalfExtents, dir, Quaternion.LookRotation(dir), values.primaryAttackSettings.chargeReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.primaryAttackSettings.chargePenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(Mathf.Lerp(values.primaryAttackSettings.chargeDamageStart, values.primaryAttackSettings.chargeDamageCharged, data.chargingProgress) * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.primaryAttackSettings.chargeRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);
                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }

                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {

                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.Lerp(values.primaryAttackSettings.chargeDamageStart, values.primaryAttackSettings.chargeDamageCharged, data.chargingProgress), values.gameGunID, pb.transform.position, dir, values.primaryAttackSettings.chargeRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.primaryAttackSettings.chargeAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.chargeAnimationMissName);
                            }

                        }
                        //Play sound
                        if (values.primaryAttackSettings.chargeMissSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.chargeMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 2, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 2);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.chargeMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.primaryAttackSettings.chargeAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.chargeAnimationHitName);
                            }
   
                        }
                        //Play sound
                        if (values.primaryAttackSettings.chargeHitSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.chargeHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 3, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 3);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.chargeHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        if (values.primaryAttackSettings.chargeAnimationHitObjectName != "")
                        {
                            //Play animation
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.primaryAttackSettings.chargeAnimationHitObjectName);
                            }
      
                        }
                        //Play sound
                        if (values.primaryAttackSettings.chargeHitObjectSound)
                        {
                            data.sounds.clip = values.primaryAttackSettings.chargeHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 4, 0);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 4);

                    data.nextActionPossibleAt = Time.time + values.primaryAttackSettings.chargeHitObjectTime;
                }

                data.chargingProgress = 0f;

                yield return new WaitForSeconds(values.primaryAttackSettings.chargeHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }

        public IEnumerator MeleeExecuteSecondaryStab(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
    
            if (values.secondaryAttackSettings.stabWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.secondaryAttackSettings.stabWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.secondaryAttackSettings.stabWindupAnimationName);
                        }
     
                    }
                }

                //Call network
                pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 0, 1);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 0);

                data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.stabWindupTime + values.secondaryAttackSettings.stabHitTime + values.secondaryAttackSettings.stabMissTime;

                //Wait
                yield return new WaitForSeconds(values.secondaryAttackSettings.stabWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.secondaryAttackSettings.stabReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.secondaryAttackSettings.stabHalfExtents, dir, Quaternion.LookRotation(dir), values.secondaryAttackSettings.stabReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.secondaryAttackSettings.stabPenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {

                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(values.secondaryAttackSettings.stabDamage * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.secondaryAttackSettings.stabRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);
                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                                //Call
                                pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                //Tell other players we hits[i] something
                                pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);


                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(values.secondaryAttackSettings.stabDamage, values.gameGunID, pb.transform.position, dir, values.secondaryAttackSettings.stabRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.stabAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.stabAnimationMissName);
                            }
  
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.stabMissSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.stabMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 1, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 1);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.stabMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.stabAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.stabAnimationHitName);
                            }
        
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.stabHitSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.stabHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 2, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 2);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.stabHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.stabAnimationHitObjectName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.stabAnimationHitObjectName);
                            }
 
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.stabHitObjectSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.stabHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 3, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 3);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.stabHitObjectTime;
                }

                yield return new WaitForSeconds(values.secondaryAttackSettings.stabHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }

        public IEnumerator MeleeExecuteSecondaryCharge(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {


            if (values.secondaryAttackSettings.chargeWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.secondaryAttackSettings.chargeWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.secondaryAttackSettings.chargeWindupAnimationName);
                        }
     
                    }
                }

                data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.chargeWindupTime + values.secondaryAttackSettings.chargeHitTime + values.secondaryAttackSettings.chargeMissTime;

                //Call network
                pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 1, 1);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 1);

                //Wait
                yield return new WaitForSeconds(values.secondaryAttackSettings.chargeWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.secondaryAttackSettings.chargeReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.secondaryAttackSettings.chargeHalfExtents, dir, Quaternion.LookRotation(dir), values.secondaryAttackSettings.chargeReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.secondaryAttackSettings.chargePenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(Mathf.Lerp(values.secondaryAttackSettings.chargeDamageStart, values.secondaryAttackSettings.chargeDamageCharged, data.chargingProgress) * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.secondaryAttackSettings.chargeRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                                //Call
                                pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                //Tell other players we hits[i] something
                                pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);

                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.Lerp(values.secondaryAttackSettings.chargeDamageStart, values.secondaryAttackSettings.chargeDamageCharged, data.chargingProgress), values.gameGunID, pb.transform.position, dir, values.secondaryAttackSettings.chargeRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.chargeAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.chargeAnimationMissName);
                            }
    
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.chargeMissSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.chargeMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 2, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 2);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.chargeMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.chargeAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.chargeAnimationHitName);
                            }
     
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.chargeHitSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.chargeHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 3, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 3);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.chargeHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.secondaryAttackSettings.chargeAnimationHitObjectName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.secondaryAttackSettings.chargeAnimationHitObjectName);
                            }
     
                        }
                        //Play sound
                        if (values.secondaryAttackSettings.chargeHitObjectSound)
                        {
                            data.sounds.clip = values.secondaryAttackSettings.chargeHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 4, 1);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 4);

                    data.nextActionPossibleAt = Time.time + values.secondaryAttackSettings.chargeHitObjectTime;
                }

                data.chargingProgress = 0f;

                yield return new WaitForSeconds(values.secondaryAttackSettings.chargeHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }

        public IEnumerator MeleeExecuteQuickStab(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
            if (values.quickAttackSettings.stabWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.quickAttackSettings.stabWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.quickAttackSettings.stabWindupAnimationName);
                        }
 
                    }
                }

                //Call network
                pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 0, 2);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 0);

                data.nextActionPossibleAt = Time.time + values.quickAttackSettings.stabWindupTime + values.quickAttackSettings.stabHitTime + values.quickAttackSettings.stabMissTime;

                //Wait
                yield return new WaitForSeconds(values.quickAttackSettings.stabWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.quickAttackSettings.stabReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.quickAttackSettings.stabHalfExtents, dir, Quaternion.LookRotation(dir), values.quickAttackSettings.stabReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.quickAttackSettings.stabPenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(values.quickAttackSettings.stabDamage * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.quickAttackSettings.stabRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);
                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                                //Call
                                pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                //Tell other players we hits[i] something
                                pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);


                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(values.quickAttackSettings.stabDamage, values.gameGunID, pb.transform.position, dir, values.quickAttackSettings.stabRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.stabAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.stabAnimationMissName);
                            }
 
                        }
                        //Play sound
                        if (values.quickAttackSettings.stabMissSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.stabMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 1, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 1);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.stabMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.stabAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.stabAnimationHitName);
                            }
  
                        }
                        //Play sound
                        if (values.quickAttackSettings.stabHitSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.stabHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 2, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 2);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.stabHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.stabAnimationHitObjectName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.stabAnimationHitObjectName);
                            }
 
                        }
                        //Play sound
                        if (values.quickAttackSettings.stabHitObjectSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.stabHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeStabNetwork", Photon.Pun.RpcTarget.Others, 3, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(0, 3);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.stabHitObjectTime;
                }

                yield return new WaitForSeconds(values.quickAttackSettings.stabHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }

        public IEnumerator MeleeExecuteQuickCharge(Kit_ModernMeleeScript values, MeleeControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
            if (values.quickAttackSettings.chargeWindupAnimationName != "")
            {
                if (!pb.isBot)
                {
                    if (values.quickAttackSettings.chargeWindupAnimationName != "")
                    {
                        if (data.meleeRenderer.anim)
                        {
                            data.meleeRenderer.anim.Play(values.quickAttackSettings.chargeWindupAnimationName);
                        }
     
                    }
                }

                data.nextActionPossibleAt = Time.time + values.quickAttackSettings.chargeWindupTime + values.quickAttackSettings.chargeHitTime + values.quickAttackSettings.chargeMissTime;

                //Call network
                pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 1, 2);
                //Play third person reload anim
                pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 1);

                //Wait
                yield return new WaitForSeconds(values.quickAttackSettings.chargeWindupTime);
            }

            if (pb)
            {
                Vector3 center = pb.playerCameraTransform.position - (pb.playerCameraTransform.forward * (values.quickAttackSettings.chargeReach / 2f));
                Vector3 dir = pb.playerCameraTransform.forward;

                RaycastHit[] hits = Physics.BoxCastAll(center, values.quickAttackSettings.chargeHalfExtents, dir, Quaternion.LookRotation(dir), values.quickAttackSettings.chargeReach, pb.weaponHitLayers.value, QueryTriggerInteraction.Collide).OrderBy(h => Vector3.Distance(pb.playerCameraTransform.position, h.point)).ToArray();

                int penetrationPowerLeft = values.quickAttackSettings.chargePenetrationPower;
                //After penetration, only test for damage.
                bool penetratedOnce = false;

                //0 = Miss
                //1 = Hit Player
                //2 = Hit Object
                int result = 0;

                //Loop through all
                for (int i = 0; i < hits.Length; i++)
                {
                    //Check if we hits[i] ourselves
                    if (hits[i].transform.root != pb.transform.root)
                    {
                        //Check if we hits[i] a player
                        if (hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>())
                        {
                            Kit_PlayerDamageMultiplier pdm = hits[i].transform.GetComponent<Kit_PlayerDamageMultiplier>();
                            if (hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                            {
                                Kit_PlayerBehaviour hitPb = hits[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if (pb.main.currentGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage, sample damage dropoff via distance
                                        hitPb.LocalDamage(Mathf.Lerp(values.quickAttackSettings.chargeDamageStart, values.quickAttackSettings.chargeDamageCharged, (Time.time - data.quickChargeStartedAt) / values.quickAttackSettings.chargeChargeTime) * pdm.damageMultiplier, values.gameGunID, pb.transform.position, dir, values.quickAttackSettings.chargeRagdollForce, hits[i].point, pdm.ragdollId, pb.isBot, pb.id);
                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!pb.isBot)
                                    {
                                        //We hits[i] a player but his spawn protection is active
                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                                //Call
                                pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                //Tell other players we hits[i] something
                                pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);

                                if (!penetratedOnce)
                                    result = 1;

                                if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                                {
                                    Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                    if (penetrationPowerLeft >= penetration.cost)
                                    {
                                        penetrationPowerLeft -= penetration.cost;
                                        penetratedOnce = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //Just end
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!penetratedOnce)
                            {
                                //Proceed to hits[i] processor
                                if (hits[i].collider.CompareTag("Dirt")) //Check for dirt
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 1, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 1);
                                }
                                else if (hits[i].collider.CompareTag("Metal")) //Check for metal
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 2, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 2);
                                }
                                else if (hits[i].collider.CompareTag("Wood")) //Check for wood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 3, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 3);
                                }
                                else if (hits[i].collider.CompareTag("Blood")) //Check for blood
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessEnemyImpact(hits[i].point, hits[i].normal);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, -1);
                                }
                                else //Else use concrete
                                {
                                    //Call
                                    pb.main.impactProcessor.ProcessImpact(hits[i].point, hits[i].normal, 0, hits[i].transform);
                                    //Tell other players we hits[i] something
                                    pb.photonView.RPC("WeaponRaycastHit", Photon.Pun.RpcTarget.Others, hits[i].point, hits[i].normal, 0);
                                }

                                if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                {
                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.Lerp(values.quickAttackSettings.chargeDamageStart, values.quickAttackSettings.chargeDamageCharged, (Time.time - data.quickChargeStartedAt) / values.quickAttackSettings.chargeChargeTime), values.gameGunID, pb.transform.position, dir, values.quickAttackSettings.chargeRagdollForce, hits[i].point, pb.isBot, pb.id))
                                    {

                                        if (!pb.isBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            pb.main.hud.DisplayHitmarker();
                                        }
                                    }
                                }
                            }

                            if (!penetratedOnce)
                                result = 2;

                            if (hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>())
                            {
                                Kit_MeleePenetrateableObject penetration = hits[i].transform.GetComponent<Kit_MeleePenetrateableObject>();
                                if (penetrationPowerLeft >= penetration.cost)
                                {
                                    penetrationPowerLeft -= penetration.cost;
                                    penetratedOnce = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                //Just end
                                break;
                            }
                        }
                    }
                }

                if (result == 0)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.chargeAnimationMissName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.chargeAnimationMissName);
                            }

                        }
                        //Play sound
                        if (values.quickAttackSettings.chargeMissSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.chargeMissSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 2, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 2);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.chargeMissTime;
                }
                else if (result == 1)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.chargeAnimationHitName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.chargeAnimationHitName);
                            }
 
                        }
                        //Play sound
                        if (values.quickAttackSettings.chargeHitSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.chargeHitSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 3, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 3);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.chargeHitTime;
                }
                else if (result == 2)
                {
                    if (!pb.isBot)
                    {
                        //Play animation
                        if (values.quickAttackSettings.chargeAnimationHitObjectName != "")
                        {
                            if (data.meleeRenderer.anim)
                            {
                                data.meleeRenderer.anim.Play(values.quickAttackSettings.chargeAnimationHitObjectName);
                            }

                        }
                        //Play sound
                        if (values.quickAttackSettings.chargeHitObjectSound)
                        {
                            data.sounds.clip = values.quickAttackSettings.chargeHitObjectSound;
                            data.sounds.Play();
                        }
                    }

                    //Call network
                    pb.photonView.RPC("MeleeChargeNetwork", Photon.Pun.RpcTarget.Others, 4, 2);
                    //Play third person reload anim
                    pb.thirdPersonPlayerModel.PlayMeleeAnimation(1, 4);

                    data.nextActionPossibleAt = Time.time + values.quickAttackSettings.chargeHitObjectTime;
                }

                yield return new WaitForSeconds(values.quickAttackSettings.chargeHitObjectTime);

                if (pb && !pb.isBot)
                    data.startedRunAnimation = false;
            }
        }


        public IEnumerator WeaponBurstFire(Kit_ModernWeaponScript values, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < values.burstBulletsPerShot && data.bulletsLeft > 0)
            {
                //Fire
                bulletsFired++;

                //Subtract bullets
                if (!pb.main.gameInformation.debugEnableUnlimitedBullets)
                    data.bulletsLeft--;

                if (!pb.isBot)
                {
                    //Play sound
                    data.soundFire.PlayOneShot(data.weaponRenderer.cachedFireSound);

                    //Apply recoil using coroutine helper
                    instance.StartCoroutine(instance.WeaponApplyRecoil(values, data, pb, RandomExtensions.RandomBetweenVector2(values.recoilPerShotMin, values.recoilPerShotMax), values.recoilApplyTime));

                    //Play fire animation
                    if (data.bulletsLeft == 1)
                    {
                        //Last fire
                        if (data.weaponRenderer.anim)
                        {
                            //Play animation
                            data.weaponRenderer.anim.Play("Fire Last", 0, 0f);
                        }
 
                    }
                    else
                    {
                        if (data.isAiming)
                        {
                            //Normal fire (in aiming mode)
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire Aim", 0, 0f);
                            }

                        }
                        else
                        {
                            //Normal fire
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire", 0, 0f);
                            }
    
                        }
                    }
                    //Play third person fire animation
                    pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);
                }
                else
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                    //Play third person fire animation
                    pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                    //Play Muzzle Flash Particle System, if assigned
                    if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.tpWeaponRenderer.muzzleFlash.Play(true);
                    }
                }

                //Set firerate
                data.lastFire = Time.time;


                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Play Muzzle Flash Particle System, if assigned
                    if (!pb.isBot && data.weaponRenderer.muzzleFlash && data.weaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.weaponRenderer.muzzleFlash.Play(true);
                    }
                }
                else
                {
                    if (!pb.isBot && data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.tpWeaponRenderer.muzzleFlash.Play(true);
                    }
                }

                //Simple fire
                if (values.fireTypeMode == FireTypeMode.Simple)
                {
                    if (values.bulletsMode == BulletMode.Raycast)
                    {
                        //Fire Raycast
                        values.FireRaycast(pb, data);
                    }
                    else if (values.bulletsMode == BulletMode.Physical)
                    {
                        //Fire Physical Bullet
                        values.FirePhysicalBullet(pb, data);
                    }
                }
                //Pellet fire
                else if (values.fireTypeMode == FireTypeMode.Pellets)
                {
                    //Count how many have been shot
                    int pelletsShot = 0;
                    while (pelletsShot < values.amountOfPellets)
                    {
                        //Increase amount of shot ones
                        pelletsShot++;
                        if (values.bulletsMode == BulletMode.Raycast)
                        {
                            //Fire Raycast
                            values.FireRaycast(pb, data);
                        }
                        else if (values.bulletsMode == BulletMode.Physical)
                        {
                            //Fire Physical Bullet
                            values.FirePhysicalBullet(pb, data);
                        }
                    }
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator WeaponBurstFireOthers(Kit_ModernWeaponScript values, WeaponControllerOthersRuntimeData data, Kit_PlayerBehaviour pb, int burstLength)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < burstLength)
            {
                //Fire
                bulletsFired++;

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator WeaponBurstFireOthers(Kit_ModernWeaponScript values, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, int burstLength)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < burstLength)
            {
                //Fire
                bulletsFired++;

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }
                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator NetworkReplaceWeaponWait(Kit_PlayerBehaviour pb, int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
        {
            while (!pb || pb.customWeaponManagerData == null) yield return null;

            //Get runtime data
            if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
            {
                WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                while (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData == null) yield return null;
                if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    //Get old data
                    WeaponControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as WeaponControllerRuntimeData;
                    //Clean Up
                    for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                    {
                        Destroy(oldWcrd.instantiatedObjects[i]);
                    }
                }
                else if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                {
                    //Get old data
                    MeleeControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as MeleeControllerRuntimeData;
                    //Clean Up
                    for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                    {
                        Destroy(oldWcrd.instantiatedObjects[i]);
                    }
                }
                else if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                {
                    //Get old data
                    GrenadeControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as GrenadeControllerRuntimeData;
                    //Clean Up
                    for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                    {
                        Destroy(oldWcrd.instantiatedObjects[i]);
                    }
                }

                //Hide crosshair
                pb.main.hud.DisplayCrosshair(0f);
                //Get their behaviour modules
                Kit_WeaponBase newWeaponBehaviour = pb.gameInformation.allWeapons[weapon];
                //Setup new
                newWeaponBehaviour.SetupValues(weapon); //This sets up values in the object itself, nothing else
                if (newWeaponBehaviour is Kit_ModernWeaponScript)
                {
                    object newRuntimeData = newWeaponBehaviour.SetupThirdPersonOthers(pb, newWeaponBehaviour as Kit_ModernWeaponScript, attachments); //This creates the first person objects
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]] = new WeaponReference();
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].behaviour = newWeaponBehaviour;
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData = newRuntimeData;
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].attachments = attachments;
                    if (runtimeData.currentWeapon[0] == slot[0] && runtimeData.currentWeapon[1] == slot[1])
                    {
                        //Select current weapon
                        newWeaponBehaviour.DrawWeaponOthers(pb, newRuntimeData);
                        //Set current weapon
                        runtimeData.currentWeapon[0] = slot[0];
                        runtimeData.currentWeapon[1] = slot[1];
                    }
                }
                else
                {
                    object newRuntimeData = newWeaponBehaviour.SetupThirdPersonOthers(pb, null, attachments); //This creates the first person objects
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]] = new WeaponReference();
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].behaviour = newWeaponBehaviour;
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData = newRuntimeData;
                    runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].attachments = attachments;
                    if (runtimeData.currentWeapon[0] == slot[0] && runtimeData.currentWeapon[1] == slot[1])
                    {
                        //Select current weapon
                        newWeaponBehaviour.DrawWeaponOthers(pb, newRuntimeData);
                        //Set current weapon
                        runtimeData.currentWeapon[0] = slot[0];
                        runtimeData.currentWeapon[1] = slot[1];
                    }
                }
            }
        }
    }
}
