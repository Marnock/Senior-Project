namespace ImmixKit
{
    using ExitGames.Client.Photon;

    public enum KitNetworkingMode { Traditional, Lobby }

    /// <summary>
    /// Holds more unique info than Kit_GameInformation
    /// </summary>
    public static class Kit_GameSettings
    {
        public static string userName = "Unassigned"; //Our current username
        public static string selectedRegion;

        public static bool isAimingToggle = true;

     
        public static bool isCrouchToggle = true;

  
        public static int gameLength;

   
        public static float baseFov = 60f;

     
        public static float hipSensitivity = 1f;

     
        public static float aimSensitivity = 0.8f;

      
        public static float fullScreenAimSensitivity = 0.2f;

        #region Runtime
        public static bool isThirdPersonActive;
       
        public static KitNetworkingMode currentNetworkingMode = KitNetworkingMode.Traditional;
        #endregion
    }
}
