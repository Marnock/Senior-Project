using Photon.Pun;
using System;
using UnityEngine;

namespace ImmixKit
{
    public class BasicMouseLookRuntimeData
    {
        public float mouseX; //Rotation on Unity Y-Axis (Player Object)
        public float mouseY; //Rotation on Unity X-Axis (Camera/Weapons)

        public float recoilMouseX; //Recoil on x axis
        public float recoilMouseY; //Recoil on y axis

        public float finalMouseX; //Rotation on Unity Y-Axis with recoil applied
        public float finalMouseY; //Rotation on Unity X-Axis with recoil applied

        public bool lastThirdPersonButton;

        public bool wasAimingLast;

        /// <summary>
        /// 0 = First person pos
        /// 1 = Third person pos
        /// </summary>
        public float firstPersonThirdPersonBlend;

        /// <summary>
        /// Hit for perspective
        /// </summary>
        public RaycastHit perspectiveClippingAvoidmentHit;

        /// <summary>
        /// Hit for crosshair
        /// </summary>
        public RaycastHit worldPositionCrosshair;
    }

    public class BasicMouseLookOthersRuntimeData
    {
        public float mouseY; //Rotation on Unity X-Axis (Camera/Weapons)
        public float leaningState;
    }

 

    public class Kit_BasicMouseLook : Kit_MouseLookBase
    {
        #region Perspective Persistent Data
 
        public static Kit_GameInformation.Perspective desiredPerspective;

  
        public static Kit_GameInformation.Perspective currentPerspective;

  
        public static bool wasPerspectiveSetInitially;
        #endregion

        [Header("Basic Sensitivity")]
        public float basicSensitivityX = 1f;
        public float basicSensitivityY = 1f;

        [Header("Limits")]
        public float minY = -85f; //Minimum for y looking
        public float maxY = 85f; //Maximum for y looking

      
     
        [Header("Perspective")]
        public Vector3 cameraFirstPersonPosition;
    
        public Vector3 cameraThirdPersonPosition;

     
        public float firstThirdPersonChangeSpeed = 4f;

     
        [Header("Clipping Avoidment")]
        public bool enableCameraClippingAvoidment = true;
   
        public LayerMask clippingAvoidmentMask;
    
        public float clippingAvoidmentCorrection = 0.01f;

   
        [Header("World Position Crosshair")]
        public bool enableWorldPositionCrosshair = true;
    
        public LayerMask worldPositionCrosshairMask;
     
        public float worldPositionCrosshairMaxDistance = 100f;

        public override void StartLocalPlayer(Kit_PlayerBehaviour pb)
        {
            //Assign
            pb.customMouseLookData = new BasicMouseLookRuntimeData();
            //Get our custom data
            BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
            //Set initial look pos
            data.mouseX = pb.transform.localEulerAngles.y;
            if (!wasPerspectiveSetInitially)
            {
                //Set default
                if (pb.main.gameInformation.perspectiveMode == Kit_GameInformation.PerspectiveMode.FirstPersonOnly || pb.main.gameInformation.perspectiveMode == Kit_GameInformation.PerspectiveMode.Both && pb.main.gameInformation.defaultPerspective == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Set desired
                    desiredPerspective = Kit_GameInformation.Perspective.FirstPerson;
                    //Set current
                    currentPerspective = Kit_GameInformation.Perspective.FirstPerson;
                    //Set Blend
                    data.firstPersonThirdPersonBlend = 0f;
                }
                else if (pb.main.gameInformation.perspectiveMode == Kit_GameInformation.PerspectiveMode.ThirdPersonOnly || pb.main.gameInformation.perspectiveMode == Kit_GameInformation.PerspectiveMode.Both && pb.main.gameInformation.defaultPerspective == Kit_GameInformation.Perspective.ThirdPerson)
                {
                    //Set desired
                    desiredPerspective = Kit_GameInformation.Perspective.ThirdPerson;
                    //Set current
                    currentPerspective = Kit_GameInformation.Perspective.ThirdPerson;
                    //Set Blend
                    data.firstPersonThirdPersonBlend = 1f;
                }

                wasPerspectiveSetInitially = true;
            }
            else
            {
                if (currentPerspective == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Set Blend
                    data.firstPersonThirdPersonBlend = 0f;
                }
                else
                {
                    //Set Blend
                    data.firstPersonThirdPersonBlend = 1f;
                }
            }
            //Update
            UpdatePerspectiveScripts(pb, currentPerspective);
        }

        public override void CalculateLookUpdate(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData == null || pb.customMouseLookData.GetType() != typeof(BasicMouseLookRuntimeData))
            {
                pb.customMouseLookData = new BasicMouseLookRuntimeData();
            }

            //Get our custom data
            BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;

            //Get Input, if the cursor is locked
            if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
            {
                data.mouseY += pb.input.mouseY * basicSensitivityY * pb.weaponManager.CurrentSensitivity(pb);
                data.mouseX += pb.input.mouseX * basicSensitivityX * pb.weaponManager.CurrentSensitivity(pb);
            }
            //Calculate recoil
            if (pb.recoilApplyRotation.eulerAngles.x < 90)
            {
                data.recoilMouseY = pb.recoilApplyRotation.eulerAngles.x;
            }
            else
            {
                data.recoilMouseY = pb.recoilApplyRotation.eulerAngles.x - 360;
            }

            if (pb.recoilApplyRotation.eulerAngles.y < 90)
            {
                data.recoilMouseX = -pb.recoilApplyRotation.eulerAngles.y;
            }
            else
            {
                data.recoilMouseX = -(pb.recoilApplyRotation.eulerAngles.y - 360);
            }

            //Clamp y input
            data.mouseY = Mathf.Clamp(data.mouseY, minY, maxY);
            //Apply reocil
            data.finalMouseY = Mathf.Clamp(data.mouseY + data.recoilMouseY, minY, maxY);

            //Simplify x input
            data.mouseX %= 360;
            //Apply recoil
            data.finalMouseX = data.mouseX + data.recoilMouseX;

            if (!pb.isBot) //Bots cannot handle input like this, so they will need to assign it themselves.
            {
                //Apply rotation
                pb.transform.rotation = Quaternion.Euler(new Vector3(0, data.finalMouseX, 0f));
                pb.mouseLookObject.localRotation = Quaternion.Euler(-data.finalMouseY, 0, 0);
            }
            else
            {
                data.finalMouseY = -pb.mouseLookObject.localEulerAngles.x;
                if (data.finalMouseY < -180) data.finalMouseY += 360;
                data.mouseY = data.finalMouseY;
            }

            if (!pb.isBot)
            {
                #region Perspective Management
                if (pb.main.gameInformation.perspectiveMode == Kit_GameInformation.PerspectiveMode.Both)
                {
                    //Check if input changed
                    if (LockCursor.lockCursor && data.lastThirdPersonButton != pb.input.thirdPerson)
                    {
                        data.lastThirdPersonButton = pb.input.thirdPerson;

                        //Check if button is pressed
                        if (pb.input.thirdPerson)
                        {
                            //Change perspective
                            if (desiredPerspective == Kit_GameInformation.Perspective.FirstPerson) desiredPerspective = Kit_GameInformation.Perspective.ThirdPerson;
                            else desiredPerspective = Kit_GameInformation.Perspective.FirstPerson;
                        }
                    }
                }

                if (pb.main.gameInformation.thirdPersonAiming == Kit_GameInformation.ThirdPersonAiming.GoIntoFirstPerson && pb.weaponManager.IsAiming(pb) || pb.weaponManager.ForceIntoFirstPerson(pb))
                {
                    if (data.firstPersonThirdPersonBlend >= 0) data.firstPersonThirdPersonBlend -= Time.deltaTime / pb.weaponManager.AimInTime(pb);
                }
                else if (desiredPerspective == Kit_GameInformation.Perspective.FirstPerson)
                {
                    if (data.firstPersonThirdPersonBlend >= 0) data.firstPersonThirdPersonBlend -= Time.deltaTime * firstThirdPersonChangeSpeed;
                }
                else
                {
                    if (data.firstPersonThirdPersonBlend <= 1) data.firstPersonThirdPersonBlend += Time.deltaTime * firstThirdPersonChangeSpeed;
                }

                //Clamp
                data.firstPersonThirdPersonBlend = Mathf.Clamp(data.firstPersonThirdPersonBlend, 0f, 1f);

                if (Mathf.Approximately(data.firstPersonThirdPersonBlend, 0f) && currentPerspective != Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Set
                    currentPerspective = Kit_GameInformation.Perspective.FirstPerson;
                    //Update
                    UpdatePerspectiveScripts(pb, Kit_GameInformation.Perspective.FirstPerson);
                }

                if (!Mathf.Approximately(data.firstPersonThirdPersonBlend, 0f) && currentPerspective != Kit_GameInformation.Perspective.ThirdPerson)
                {
                    //Set
                    currentPerspective = Kit_GameInformation.Perspective.ThirdPerson;
                    //Update
                    UpdatePerspectiveScripts(pb, Kit_GameInformation.Perspective.ThirdPerson);
                }

                if (currentPerspective == Kit_GameInformation.Perspective.ThirdPerson) Kit_GameSettings.isThirdPersonActive = true;
                else Kit_GameSettings.isThirdPersonActive = false;

                if (enableCameraClippingAvoidment)
                {
                    if (Kit_GameSettings.isThirdPersonActive)
                    {
                        if (Physics.Linecast(pb.playerCameraTransform.transform.position, pb.playerCameraTransform.TransformPoint(cameraThirdPersonPosition), out data.perspectiveClippingAvoidmentHit, clippingAvoidmentMask.value, QueryTriggerInteraction.Ignore))
                        {
                            pb.main.mainCamera.transform.localPosition = Vector3.Lerp(cameraFirstPersonPosition, pb.main.mainCamera.transform.parent.InverseTransformPoint(data.perspectiveClippingAvoidmentHit.point + data.perspectiveClippingAvoidmentHit.normal * clippingAvoidmentCorrection), data.firstPersonThirdPersonBlend);
                        }
                        else
                        {
                            pb.main.mainCamera.transform.localPosition = Vector3.Lerp(cameraFirstPersonPosition, cameraThirdPersonPosition, data.firstPersonThirdPersonBlend);
                        }
                    }
                    else
                    {
                        pb.main.mainCamera.transform.localPosition = Vector3.Lerp(cameraFirstPersonPosition, cameraThirdPersonPosition, data.firstPersonThirdPersonBlend);
                    }
                }
                else
                {
                    pb.main.mainCamera.transform.localPosition = Vector3.Lerp(cameraFirstPersonPosition, cameraThirdPersonPosition, data.firstPersonThirdPersonBlend);
                }

                if (enableWorldPositionCrosshair)
                {
                    if (Kit_GameSettings.isThirdPersonActive)
                    {
                        if (pb.main.gameInformation.thirdPersonCameraShooting)
                        {
                            if (Physics.Raycast(pb.main.mainCamera.transform.position, pb.main.mainCamera.transform.forward, out data.worldPositionCrosshair, worldPositionCrosshairMaxDistance, worldPositionCrosshairMask.value))
                            {
                                //Get screen pos
                                Vector3 canvasPos = pb.main.canvas.WorldToCanvas(data.worldPositionCrosshair.point, pb.main.mainCamera);
                                pb.main.hud.MoveCrosshairTo(canvasPos);
                            }
                            else
                            {
                                //Get screen pos
                                Vector3 canvasPos = pb.main.canvas.WorldToCanvas(pb.main.mainCamera.transform.position + pb.main.mainCamera.transform.forward * 100f, pb.main.mainCamera);
                                pb.main.hud.MoveCrosshairTo(canvasPos);
                            }
                        }
                        else
                        {
                            if (Physics.Raycast(pb.playerCameraTransform.transform.position, pb.playerCameraTransform.transform.forward, out data.worldPositionCrosshair, worldPositionCrosshairMaxDistance, worldPositionCrosshairMask.value))
                            {
                                //Get screen pos
                                Vector3 canvasPos = pb.main.canvas.WorldToCanvas(data.worldPositionCrosshair.point, pb.main.mainCamera);
                                pb.main.hud.MoveCrosshairTo(canvasPos);
                            }
                            else
                            {
                                //Get screen pos
                                Vector3 canvasPos = pb.main.canvas.WorldToCanvas(pb.playerCameraTransform.transform.position + pb.playerCameraTransform.transform.forward * 100f, pb.main.mainCamera);
                                pb.main.hud.MoveCrosshairTo(canvasPos);
                            }
                        }
                    }
                    else
                    {
                        //Reset to center
                        pb.main.hud.MoveCrosshairTo(Vector3.zero);
                    }
                }
            }
            #endregion
        }

        void UpdatePerspectiveScripts(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective)
        {
            //Tell weapon manager
            pb.weaponManager.FirstThirdPersonChanged(pb, perspective);
            //Tell player model
            pb.thirdPersonPlayerModel.FirstThirdPersonChanged(pb, perspective);
        }

        public override bool ReachedYMax(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;

                //Check if we reached min or max value on Y
                if (Mathf.Approximately(data.finalMouseY, minY)) return true;
                else if (Mathf.Approximately(data.finalMouseY, maxY)) return true;
            }

            return false;
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                    //Send y looking
                    stream.SendNext(data.finalMouseY);
                }
                else
                {
                    //Send dummy values
                    stream.SendNext(0f);
                    stream.SendNext(0f);
                }
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData == null || pb.customMouseLookData.GetType() != typeof(BasicMouseLookOthersRuntimeData))
                {
                    pb.customMouseLookData = new BasicMouseLookOthersRuntimeData();
                }

                //Get our custom data
                BasicMouseLookOthersRuntimeData data = pb.customMouseLookData as BasicMouseLookOthersRuntimeData;
                //Receive look value
                data.mouseY = (float)stream.ReceiveNext();
                //Receive leaning
                data.leaningState = (float)stream.ReceiveNext();
            }
        }

        public override float GetSpeedMultiplier(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;;
            }
            return 1f;
        }

        public override Vector3 GetCameraOffset(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
            }
            return base.GetCameraOffset(pb);
        }

        public override Quaternion GetCameraRotationOffset(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
            }
            return base.GetCameraRotationOffset(pb);
        }

        public override Vector3 GetWeaponOffset(Kit_PlayerBehaviour pb)
        {
            if (pb.weaponManager.IsAiming(pb))
            {
                return Vector3.zero;
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                }
            }
            return base.GetWeaponOffset(pb);
        }

        public override Quaternion GetWeaponRotationOffset(Kit_PlayerBehaviour pb)
        {
            if (pb.weaponManager.IsAiming(pb))
            {
                return Quaternion.identity;
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                }
            }
            return base.GetWeaponRotationOffset(pb);
        }

        public override Kit_GameInformation.Perspective GetPerspective(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                //Return whats saved
                return currentPerspective;
            }
            return Kit_GameInformation.Perspective.FirstPerson;
        }
    }
}
