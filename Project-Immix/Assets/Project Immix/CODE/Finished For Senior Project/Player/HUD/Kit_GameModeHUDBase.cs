using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_GameModeHUDBase : MonoBehaviour
    {

        public virtual void HUDInitialize(Kit_IngameMain main)
        {

        }
        public abstract void HUDUpdate(Kit_IngameMain main);
    }
}
