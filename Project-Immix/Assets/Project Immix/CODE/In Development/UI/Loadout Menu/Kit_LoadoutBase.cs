 using Photon.Pun;
using UnityEngine;

namespace ImmixKit
{
    [System.Serializable]

    public class Loadout
    {
        public LoadoutWeapon[] loadoutWeapons;
    
        public int teamOnePlayerModelID;
             public int[] teamOnePlayerModelCustomizations = new int[0];
     
        public int teamTwoPlayerModelID;
     
        public int[] teamTwoPlayerModelCustomizations = new int[0];
    }

    [System.Serializable]
    public class LoadoutWeapon
    {
 
        public int goesToSlot;
          public int weaponID;
   
        public int[] attachments;
    }

      public abstract class Kit_LoadoutBase : MonoBehaviour
    {
   
        public abstract Loadout GetCurrentLoadout();


        public abstract void Initialize();

        public abstract void Open();

    
        public abstract void ForceClose();

  
        public bool isOpen;

    
        public virtual void TeamChanged(int newTeam)
        {

        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }
    }
}
