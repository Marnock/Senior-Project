using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// Contains information for custimization slots
    /// </summary>
    [System.Serializable]
    public class CustomizationSlot
    {
   
        public string name;

    
        public Transform uiPosition;

        public PlayerModelCustomization[] customizations;
    }
    [System.Serializable]
    public class PlayerModelCustomization
    {
        public string name;

        public Kit_PlayerModelCustomizationBehaviour[] customizationBehaviours;
    }


    public abstract class Kit_PlayerModelCustomizationBehaviour : MonoBehaviour
    {
        public abstract void Selected(Kit_PlayerBehaviour pb);
        public abstract void Unselected(Kit_PlayerBehaviour pb);
    }
}