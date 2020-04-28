using UnityEngine;
using System.Collections;

namespace ImmixKit
{

    /// <summary>
    /// This object contains a Region that is displayed to the user if assigned in <see cref="Kit_GameInformation"/>
    /// </summary>
    public class Kit_RegionInformation : ScriptableObject
    {
        public string regionName; 
        public string serverLocation; 
        public string token; //The token to use for Photon
    }
}
