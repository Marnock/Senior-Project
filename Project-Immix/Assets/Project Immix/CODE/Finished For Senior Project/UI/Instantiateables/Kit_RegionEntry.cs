using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    namespace UI
    {
        public class Kit_RegionEntry : MonoBehaviour
        {
            public Text regionName; //Display
            public Text regionServer; //Display
            private Kit_MainMenu mm; //Our current main menu
            private int myId; //The region ID of this entry

 
            public void Setup(Kit_MainMenu curMM, int curId)
            {
                //Copy Main Menu
                mm = curMM;
                //Copy ID
                myId = curId;
                //Assign text
                regionName.text = curMM.gameInformation.allRegions[curId].regionName;
                regionServer.text = curMM.gameInformation.allRegions[curId].serverLocation;
                //Reset scale (Because otherwise scale would be offset)
                transform.localScale = Vector3.one;
            }

            //Called by the button on this prefab
            public void OnClick()
            {
                if (mm)
                {
                    mm.ChangeRegion(myId);
                }
            }
        }
    }
}
