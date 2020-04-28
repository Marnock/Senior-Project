using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_DeathCameraBase : MonoBehaviour
    {
        public abstract void SetupDeathCamera(Kit_ThirdPersonPlayerModel model);
    }
}