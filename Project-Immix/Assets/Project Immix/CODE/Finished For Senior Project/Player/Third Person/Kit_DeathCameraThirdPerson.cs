using UnityEngine;
using UnityEngine.Rendering;

namespace ImmixKit
{
    public class Kit_DeathCameraThirdPerson :Kit_DeathCameraBase
    {
    
        public Transform lookAtTransform;
        /// <summary>
        /// How quickly does the camera react
        /// </summary>
        public float lookAtSmooth = 5f;
         public float distanceFovReference = 30f;
    
        public float smallestFov = 40f;

        private Kit_IngameMain main;
        /// <summary>
        /// Where we died
        /// </summary>
        private Vector3 deathPos;
    
        private bool wasSetup;

        private void Update()
        {
            if (lookAtTransform && main && !main.myPlayer && main.mainCamera.transform.parent == null)
            {
                main.mainCamera.transform.position = deathPos;
                //main.mainCamera.transform.forward = Vector3.Slerp(main.mainCamera.transform.forward, lookAtTransform.position - main.mainCamera.transform.position, Time.deltaTime * lookAtSmooth);
                main.mainCamera.transform.rotation = Quaternion.Slerp(main.mainCamera.transform.rotation, Quaternion.LookRotation(lookAtTransform.position - main.mainCamera.transform.position), Time.deltaTime * lookAtSmooth);

                main.mainCamera.fieldOfView = Mathf.Lerp(main.mainCamera.fieldOfView, Mathf.Lerp(Kit_GameSettings.baseFov, smallestFov, Vector3.Distance(main.mainCamera.transform.position, lookAtTransform.position) / distanceFovReference), Time.deltaTime * lookAtSmooth);
            }

            if (main)
            {
                if (main.myPlayer)
                {
                    enabled = false;
                    main.isCameraFovOverridden = false;
                    wasSetup = false;
                }
            }
        }

        public override void SetupDeathCamera(Kit_ThirdPersonPlayerModel model)
        {
            Kit_ThirdPersonModernPlayerModel modernModel = model as Kit_ThirdPersonModernPlayerModel;
            modernModel.kpb.main.activeCameraTransform = null;
            main = modernModel.kpb.main;
            main.isCameraFovOverridden = true;
            deathPos = modernModel.kpb.playerCameraTransform.position;
            main.mainCamera.transform.position = modernModel.kpb.playerCameraTransform.position;
            main.mainCamera.transform.rotation = modernModel.kpb.playerCameraTransform.rotation;
            //Show
            for (int i = 0; i < modernModel.fpShadowOnlyRenderers.Length; i++)
            {
                modernModel.fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            }
            //Set bool
            wasSetup = true;
            //Make sure its enabled
            enabled = true;
        }

        private void OnDestroy()
        {
            if (wasSetup)
            {
                if (main)
                {
                    main.isCameraFovOverridden = false;
                    if (!main.myPlayer)
                    {
                        main.activeCameraTransform = main.spawnCameraPosition;
                    }
                }
            }
        }
    }
}