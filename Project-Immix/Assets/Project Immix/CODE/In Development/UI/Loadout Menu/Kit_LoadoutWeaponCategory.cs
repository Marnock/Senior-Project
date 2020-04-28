using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_LoadoutWeaponCategory : MonoBehaviour
    {
        public EventTrigger eventTrigger;
     
        public Image currentWeaponImage;
        
        public Dropdown weaponsInDropdown;
    
        public List<int> dropdownLocalToGlobal = new List<int>();
      
        public Button customizeWeaponButton;
           public Text weaponCategoryName;
    
        public GameObject statsRoot;
     
        public Image statsDamageFill;
  
        public Image statsFireRateFill;
      
        public Image statsRecoilFill;
    
        public Image statsReachFill;
    }
}