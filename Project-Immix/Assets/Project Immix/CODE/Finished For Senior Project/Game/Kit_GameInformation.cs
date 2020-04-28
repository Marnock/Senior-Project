using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ImmixKit.Weapons;

namespace ImmixKit
{


    [System.Serializable]
    public class AnimatorSetInformation
    {
      
        public string prefix;
     
        public int type;
    }

    /// <summary>
    /// This Object contains all of the game info
    /// </summary>

    public class Kit_GameInformation : ScriptableObject
    {
        public enum PerspectiveMode { FirstPersonOnly, ThirdPersonOnly, Both }
        public enum Perspective { FirstPerson, ThirdPerson }
        public enum ThirdPersonAiming { OverShoulder, GoIntoFirstPerson }

        
        public string gameVersion = "1";
     
        public int sendRate = 40;
    
        public string defaultRegion = "eu";
        public Kit_RegionInformation[] allRegions;
      
        public Kit_GameModeBase[] allGameModes;
      
        public string[] allWeaponCategories = new string[5] { "Primary", "Secondary", "Melee", "Lethal", "NonLethal" };
      
        public int[] defaultWeaponsInSlot = new int[5];
      
        public Kit_WeaponBase[] allWeapons;
      
        public Kit_PlayerModelInformation[] allTeamOnePlayerModels;
    
        public Kit_PlayerModelInformation[] allTeamTwoPlayerModels;
     
        public AnimatorSetInformation[] allAnimatorAnimationSets;

        [Header("Settings")]
    
        public PerspectiveMode perspectiveMode;
     
        public Perspective defaultPerspective;
     
        public ThirdPersonAiming thirdPersonAiming;
        /// <summary>
        /// If this is enabled, bullets will be fired from third person camera in third person mode
        /// </summary>
        public bool thirdPersonCameraShooting;
        public bool enableDropWeaponOnSceneSpawnedWeapons = false;
        public bool enableAutoReload;
        [Header("Debug")]
        public bool debugEnableUnlimitedBullets;
     
        public bool debugEnableUnlimitedReloads;

        [Header("Modules")]
    
        public GameObject mainCameraOverride;
    
        public Kit_Plugin[] plugins;

        [Header("Points")]
        public int pointsPerKill = 100;

        public int GetCurrentLevel()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            for (int i = 0; i < allGameModes.Length; i++)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    for (int o = 0; o < allGameModes[i].traditionalMaps.Length; o++)
                    {
                        if (allGameModes[i].traditionalMaps[o].sceneName == currentScene.name)
                        {
                            return o;
                        }
                    }
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    for (int o = 0; o < allGameModes[i].lobbyMaps.Length; o++)
                    {
                        if (allGameModes[i].lobbyMaps[o].sceneName == currentScene.name)
                        {
                            return o;
                        }
                    }
                }
            }

            return -1;
        }

        public Kit_MapInformation GetMapInformationFromSceneName(string scene)
        {
            for (int i = 0; i < allGameModes.Length; i++)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    for (int o = 0; o < allGameModes[i].traditionalMaps.Length; o++)
                    {
                        if (allGameModes[i].traditionalMaps[o].sceneName == scene)
                        {
                            return allGameModes[i].traditionalMaps[o];
                        }
                    }
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    for (int o = 0; o < allGameModes[i].lobbyMaps.Length; o++)
                    {
                        if (allGameModes[i].lobbyMaps[o].sceneName == scene)
                        {
                            return allGameModes[i].lobbyMaps[o];
                        }
                    }
                }
            }
            return null;
        }

    }
}
