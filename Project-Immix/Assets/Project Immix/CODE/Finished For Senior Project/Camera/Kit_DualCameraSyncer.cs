using UnityEngine;

namespace ImmixKit
{
    public class Kit_DualCameraSyncer : MonoBehaviour
    {
     
        public Camera mainCamera;
        
        public Camera weaponCamera;
       
        public bool copyFov;
              public Vector3 lerpFovReference = new Vector3(30f, 60f, 90f);
      
        public Vector3 lerpFovCopy = new Vector3(30f, 50f, 65f);

     
        void LateUpdate()
        {
            if (copyFov)
            {
                weaponCamera.fieldOfView = mainCamera.fieldOfView;
            }
            else
            {
                if (mainCamera.fieldOfView > lerpFovReference.y)
                {
                    weaponCamera.fieldOfView = Mathf.Lerp(lerpFovCopy.y, lerpFovCopy.z, Mathf.InverseLerp(lerpFovReference.y, lerpFovReference.z, mainCamera.fieldOfView));
                }
                else
                {
                    weaponCamera.fieldOfView = Mathf.Lerp(lerpFovCopy.x, lerpFovCopy.y, Mathf.InverseLerp(lerpFovReference.x, lerpFovReference.y, mainCamera.fieldOfView));
                }
            }
        }
    }
}