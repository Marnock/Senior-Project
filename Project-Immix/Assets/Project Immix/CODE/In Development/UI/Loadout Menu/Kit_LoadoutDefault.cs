using ImmixKit.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace ImmixKit
{
    namespace UI
    {
        public class Kit_LoadoutDefault : Kit_LoadoutBase
        {
     
            public Kit_IngameMain mainIngame;

            public Kit_MainMenu mainMenu;

        
            public GameObject root;

       
            public GameObject weaponSelectionRoot;

       
            [Header("Weapon Categories")]
            public RectTransform weaponCategoriesRoot;
          
            public GameObject weaponCategoriesPrefab;
            /// <summary>
            /// Active weapon categories in correct order!
            /// </summary>
            public Kit_LoadoutWeaponCategory[] weaponCategories;

            [Header("Player Model")]
            /// <summary>
            /// Where are player models going to be positioned?
            /// </summary>
            public Transform playerModelGo;
            /// <summary>
            /// This list contains all elements instantiated by the player model
            /// </summary>
            private List<GameObject> playerModelObjects = new List<GameObject>();
            /// <summary>
            /// The currently displayed player model
            /// </summary>
            private Kit_ThirdPersonPlayerModel playerModelCurrent;
            /// <summary>
            /// Script to help with IK
            /// </summary>
            private Kit_LoadoutIKHelper playerModelIkHelper;

            /// <summary>
            /// Where the prefab will be instantiated
            /// </summary>
            [Header("Player Model Selection")]
            public RectTransform playerModelSelectionRoot;
            /// <summary>
            /// Prefab that displays the categories
            /// </summary>
            public GameObject playerModelSelectionPrefab;
            /// <summary>
            /// Active weapon categories in correct order!
            /// </summary>
            public Kit_LoadoutPlayerModelCategory[] playerModelSelectionCategories;
            /// <summary>
            /// Current Team
            /// </summary>
            private int playerModelCurrentTeamDisplayed = 0;

            [Header("Player Model Customization")]
            /// <summary>
            /// Player Model customization uses a different canvas that is set to camera overlay
            /// </summary>
            public Canvas playerModelCustomizationCanvas;
            /// <summary>
            /// Camera for Player Model customization
            /// </summary>
            public Camera playerModelCustomizationCamera;
            /// <summary>
            /// Root where you customize the attachments
            /// </summary>
            public GameObject playerModelCustomizationRoot;
            /// <summary>
            /// This is where the weapon prefab is going to be instantiated
            /// </summary>
            public Transform playerModelCustomizationPrefabGo;
            /// <summary>
            /// This list contains all elements instantiated in the customization menu!
            /// </summary>
            private List<GameObject> playerModelCustomizationObjects = new List<GameObject>();
            [HideInInspector]
            /// <summary>
            /// The renderer of the weapon we are currently customizing
            /// </summary>
            public Kit_ThirdPersonPlayerModel playerModelCustomizationCurrentRenderer;
            /// <summary>
            /// The prefab for the drop down UI element that will be displayed for each attachment slot
            /// </summary>
            public GameObject playerModelCustomizationDropdownPrefab;

            /// <summary>
            /// This list contains all elements instantiated by the weapons
            /// </summary>
            private List<GameObject> weaponObjects = new List<GameObject>();

            [Header("Weapon Customization")]
     
            public Canvas weaponCustomizationCanvas;
     
            public Camera weaponCustomizationCamera;
      
            public GameObject weaponCustomizationRoot;
      
            public Transform weaponCustomizationPrefabGo;
  
            public Transform weaponCustomizationRotationRoot;
     
            private List<GameObject> weaponCustomizationObjects = new List<GameObject>();
            [HideInInspector]
    
            public Weapons.Kit_WeaponRenderer weaponCustomizationCurrentRenderer;
     
            public GameObject weaponCustomizationDropdownPrefab;

            //RUNTIME DATA
            //[HideInInspector]
            public Loadout[] allLoadouts = new Loadout[5];
            [HideInInspector]
            public int currentLoadout;
      
            private int currentlyInHand;
     
            private bool isWeaponCustomizationRotationEnabled;
        

            public override void ForceClose()
            {
                //Just normally Close
                Close();
            }

            public override Loadout GetCurrentLoadout()
            {
                //Just return the currently selected loadout
                return allLoadouts[currentLoadout];
            }

            bool wasInitialized = false;

            public override void Initialize()
            {
                if (!wasInitialized)
                {
                    wasInitialized = true;
                    //Set initial values
                    root.SetActive(false);
                    isOpen = false;
                    currentLoadout = 0;
                    //Set Loadouts
                    allLoadouts = new Loadout[5];

                    Kit_GameInformation game = null;

                    if (mainIngame)
                    {
                        game = mainIngame.gameInformation;
                    }
                    else if (mainMenu)
                    {
                        game = mainMenu.gameInformation;
                    }

                    if (game)
                    {
                        weaponCategories = new Kit_LoadoutWeaponCategory[game.allWeaponCategories.Length];

                        for (int i = 0; i < game.allWeaponCategories.Length; i++)
                        {
                            //Instantiate
                            GameObject go = Instantiate(weaponCategoriesPrefab, weaponCategoriesRoot, false);
                            //Get
                            Kit_LoadoutWeaponCategory cat = go.GetComponent<Kit_LoadoutWeaponCategory>();
                            //Set
                            weaponCategories[i] = cat;
                            //Set values
                            cat.weaponCategoryName.text = game.allWeaponCategories[i];
                            cat.weaponsInDropdown.ClearOptions();
                            //Set callbacks
                            int slot = i;
                            cat.customizeWeaponButton.onClick.AddListener(delegate { CustomizeWeaponInSlot(slot); });
                            cat.weaponsInDropdown.onValueChanged.AddListener(delegate { WeaponCategoryDropdownChanged(slot); });
                            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
                            hoverEntry.eventID = EventTriggerType.PointerEnter;
                            hoverEntry.callback.AddListener(delegate { ChangeInHands(slot); });
                            cat.eventTrigger.triggers.Add(hoverEntry);
                        }

                        //Setup Dropdowns!
                        for (int i = 0; i < game.allWeapons.Length; i++)
                        {
                            int categoryId = Array.IndexOf(game.allWeaponCategories, game.allWeapons[i].weaponType);

                            if (categoryId >= 0)
                            {
                                int globalId = i;
                                weaponCategories[categoryId].dropdownLocalToGlobal.Add(globalId);

                                if (game.allWeapons[globalId].IsWeaponUnlocked(game))
                                {
                                    weaponCategories[categoryId].weaponsInDropdown.options.Add(new Dropdown.OptionData(game.allWeapons[globalId].name));
                                }
                                else
                                {
                                    //Add with locked color and level
                                    weaponCategories[categoryId].weaponsInDropdown.options.Add(new Dropdown.OptionData("<color=#800000ff>" + game.allWeapons[globalId].weaponName + " [" + game.allWeapons[globalId].levelToUnlockAt + "]</color>"));
                                }
                            }
                            else
                            {
                                Debug.LogError("Weapon with invalid weapon category: " + game.allWeapons[i].name);
                            }
                        }

                        playerModelSelectionCategories = new Kit_LoadoutPlayerModelCategory[2];


                        for (int i = 0; i < 2; i++)
                        {
                            //Instantiate
                            GameObject go = Instantiate(playerModelSelectionPrefab, playerModelSelectionRoot, false);
                            //Get
                            Kit_LoadoutPlayerModelCategory cat = go.GetComponent<Kit_LoadoutPlayerModelCategory>();
                            //Set
                            playerModelSelectionCategories[i] = cat;
                            //Set values
                            cat.teamText.text = "Team " + i;
                            cat.dropdown.ClearOptions();
                            //Set callbacks
                            int slot = i;
                            cat.customizeButton.onClick.AddListener(delegate { CustomizePlayerModel(slot); });
                            cat.dropdown.onValueChanged.AddListener(delegate { PlayerModelCategoryDropdownChanged(slot); });
                            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
                            hoverEntry.eventID = EventTriggerType.PointerEnter;
                            hoverEntry.callback.AddListener(delegate { PlayerModelChangeTeam(slot); });
                            cat.eventTrigger.triggers.Add(hoverEntry);

                            if (i == 0)
                            {
                                for (int p = 0; p < game.allTeamOnePlayerModels.Length; p++)
                                {
                                    playerModelSelectionCategories[i].dropdown.options.Add(new Dropdown.OptionData(game.allTeamOnePlayerModels[p].name));
                                }
                            }
                            else if (i == 1)
                            {
                                for (int p = 0; p < game.allTeamTwoPlayerModels.Length; p++)
                                {
                                    playerModelSelectionCategories[i].dropdown.options.Add(new Dropdown.OptionData(game.allTeamTwoPlayerModels[p].name));
                                }
                            }

                            playerModelSelectionCategories[i].dropdown.value = 0;
                            playerModelSelectionCategories[i].dropdown.RefreshShownValue();
                        }

                        for (int i = 0; i < allLoadouts.Length; i++)
                        {
                            currentLoadout = i;
                            allLoadouts[i] = new Loadout();
                            allLoadouts[i].loadoutWeapons = new LoadoutWeapon[game.allWeaponCategories.Length];
                            allLoadouts[i].teamOnePlayerModelID = 0;
                            allLoadouts[i].teamOnePlayerModelCustomizations = new int[game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];
                            allLoadouts[i].teamTwoPlayerModelID = 0;
                            allLoadouts[i].teamTwoPlayerModelCustomizations = new int[game.allTeamTwoPlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];

                            for (int o = 0; o < game.allWeaponCategories.Length; o++)
                            {
                                allLoadouts[i].loadoutWeapons[o] = new LoadoutWeapon();
                                int slotId = o;
                                allLoadouts[i].loadoutWeapons[o].goesToSlot = slotId;

                                SetWeaponInSlot(slotId, game.defaultWeaponsInSlot[slotId]);
                            }
                        }

                        for (int i = 0; i < weaponCategories.Length; i++)
                        {
                            weaponCategories[i].weaponsInDropdown.RefreshShownValue();
                        }
                    }

                    //Reset to class 0
                    currentLoadout = 0;

                    //Load
                    Load();

                    //Redraw all
                    RedrawWeaponIcons();
                }
            }

            #region Player Model Customization
            public void CustomizePlayerModel(int team)
            {
   
            }

            public void PlayerModelCategoryDropdownChanged(int team)
            {
   
            }

            public void PlayerModelChangeTeam(int team)
            {
                playerModelCurrentTeamDisplayed = team;
                FullRedraw();
            }

            void SetupPlayerModelCustomization(Kit_PlayerModelInformation inf, int[] customizations)
            {
           
            }

            void UpdatePlayerCustomization(int[] attachments)
            {
        
            }
            #endregion

            public void RedrawWeaponIcons()
            {
      
            }

            public override void Open()
            {
                //Set bool
                isOpen = true;
                //Enable Loadout menu
                root.SetActive(true);
                weaponSelectionRoot.SetActive(true);
                weaponCustomizationRoot.SetActive(false);
                if (mainIngame)
                {
                    //Disable normal UI
                    mainIngame.ui_root.SetActive(false);
                    //Disable camera
                    mainIngame.mainCamera.enabled = false;
                }
                else if (mainMenu)
                {
                    //Disable normal UI
                    //mainMenu.ui_Canvas.gameObject.SetActive(false);
                    //Disable camera
                    //mainMenu.mainCamera.enabled = false;
                }
                //Check if guns are unlocked
                CheckIfGunsAreUnlocked();
                //Make sure cursor is unlocked
                LockCursor.lockCursor = false;
                if (mainIngame)
                {
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
                else if (mainMenu)
                {
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
            }

            /// <summary>
            /// Closes the menu
            /// </summary>
            public void Close()
            {
                //Set bool
                isOpen = false;
                //Disable loadout
                root.SetActive(false);
                if (mainIngame)
                {
                    //Enable normal UI
                    mainIngame.ui_root.SetActive(true);
                    //Enable camera
                    mainIngame.mainCamera.enabled = true;
                }
                else if (mainMenu)
                {
                    //Enable normal UI
                    mainMenu.ui_Canvas.gameObject.SetActive(true);
                    //Enable camera
                    mainMenu.mainCamera.enabled = true;
                }
                //Save
                Save();
            }

            void CheckIfGunsAreUnlocked()
            {
       
            }

        
            bool loaded = false;

       
            void Save()
            {
                Kit_GameInformation game = null;
                if (mainIngame) game = mainIngame.gameInformation;
                else game = mainMenu.gameInformation;

                if (loaded)
                {
                    PlayerPrefs.SetInt("control", game.allWeapons.Length + game.allTeamOnePlayerModels.Length + game.allTeamTwoPlayerModels.Length);

                    for (int i = 0; i < allLoadouts.Length; i++)
                    {
                        PlayerPrefs.SetInt("loadout_" + i + "_amountOfWeapons", allLoadouts[i].loadoutWeapons.Length);
                        PlayerPrefs.SetInt("loadout_" + i + "_playerModelTeamOne", allLoadouts[i].teamOnePlayerModelID);
                        PlayerPrefsExtended.SetIntArray("loadout_" + i + "_playerModelTeamOneCustomization", allLoadouts[i].teamOnePlayerModelCustomizations);
                        PlayerPrefs.SetInt("loadout_" + i + "_playerModelTeamTwo", allLoadouts[i].teamTwoPlayerModelID);
                        PlayerPrefsExtended.SetIntArray("loadout_" + i + "_playerModelTeamTwoCustomization", allLoadouts[i].teamTwoPlayerModelCustomizations);

                        for (int o = 0; o < allLoadouts[i].loadoutWeapons.Length; o++)
                        {
                            PlayerPrefs.SetInt("loadout_" + i + "_weapon_" + o, allLoadouts[i].loadoutWeapons[o].weaponID);
                            PlayerPrefsExtended.SetIntArray("loadout_" + i + "_weapon_" + o + "_attachments", allLoadouts[i].loadoutWeapons[o].attachments);
                        }
                    }
                    Debug.Log("[Loadout] Saved!");
                }
            }

            void Load()
            {
                Kit_GameInformation game = null;
                if (mainIngame) game = mainIngame.gameInformation;
                else game = mainMenu.gameInformation;

                for (int i = 0; i < allLoadouts.Length; i++)
                {
                    int control = PlayerPrefs.GetInt("control");

                    if (control == game.allWeapons.Length + game.allTeamOnePlayerModels.Length + game.allTeamTwoPlayerModels.Length)
                    {

                        int amountOfWeapons = PlayerPrefs.GetInt("loadout_" + i + "_amountOfWeapons", allLoadouts[i].loadoutWeapons.Length);

                        for (int o = 0; o < amountOfWeapons; o++)
                        {
                            int globalID = PlayerPrefs.GetInt("loadout_" + i + "_weapon_" + o, -1);
                            int localID = weaponCategories[o].dropdownLocalToGlobal.IndexOf(globalID);

                            if (localID >= 0)
                            {
                                currentLoadout = i;
                                weaponCategories[o].weaponsInDropdown.value = localID;
                                allLoadouts[i].loadoutWeapons[o] = new LoadoutWeapon();
                                int slot = o;
                                allLoadouts[i].loadoutWeapons[o].goesToSlot = slot;
                                allLoadouts[i].loadoutWeapons[o].weaponID = globalID;
                                int defaultLength = 0;

                                if (game.allWeapons[globalID].firstPersonPrefab.GetComponent<Kit_WeaponRenderer>())
                                {
                                    Kit_WeaponRenderer render = game.allWeapons[globalID].firstPersonPrefab.GetComponent<Kit_WeaponRenderer>();
                                    defaultLength = render.attachmentSlots.Length;
                                }

                                allLoadouts[i].loadoutWeapons[o].attachments = PlayerPrefsExtended.GetIntArray("loadout_" + i + "_weapon_" + o + "_attachments", 0, defaultLength);
                            }
                        }

                        allLoadouts[i].teamOnePlayerModelID = PlayerPrefs.GetInt("loadout_" + i + "_playerModelTeamOne");
                        int[] customizationArray = PlayerPrefsExtended.GetIntArray("loadout_" + i + "_playerModelTeamOneCustomization", 0, game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length);

                        if (customizationArray.Length == game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length)
                        {
                            allLoadouts[i].teamOnePlayerModelCustomizations = customizationArray;
                        }
                        else
                        {
                            allLoadouts[i].teamOnePlayerModelCustomizations = new int[game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];
                        }
                        allLoadouts[i].teamTwoPlayerModelID = PlayerPrefs.GetInt("loadout_" + i + "_playerModelTeamTwo");
                        customizationArray = PlayerPrefsExtended.GetIntArray("loadout_" + i + "_playerModelTeamTwoCustomization", 0, game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length);
                        if (customizationArray.Length == game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length)
                        {
                            allLoadouts[i].teamTwoPlayerModelCustomizations = customizationArray;
                        }
                        else
                        {
                            allLoadouts[i].teamTwoPlayerModelCustomizations = new int[game.allTeamOnePlayerModels[allLoadouts[i].teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];
                        }

                    }
                }

                currentLoadout = 0;

                ChangeClass(0);

                for (int i = 0; i < weaponCategories.Length; i++)
                {
                    weaponCategories[i].weaponsInDropdown.RefreshShownValue();
                }

                Debug.Log("[Loadout] Loaded!");
                loaded = true;
            }

            public override void TeamChanged(int newTeam)
            {
                FullRedraw();
            }

            public void FullRedraw()
            {
                Kit_GameInformation game = null;
                if (mainIngame) game = mainIngame.gameInformation;
                else game = mainMenu.gameInformation;

                if (game)
                {
                    if (playerModelCurrentTeamDisplayed == 0)
                    {
                        Redraw(game.allTeamOnePlayerModels[allLoadouts[currentLoadout].teamOnePlayerModelID], allLoadouts[currentLoadout].teamOnePlayerModelCustomizations);
                    }
                    else
                    {
                        Redraw(game.allTeamTwoPlayerModels[allLoadouts[currentLoadout].teamTwoPlayerModelID], allLoadouts[currentLoadout].teamTwoPlayerModelCustomizations);
                    }
                }
            }

   
            void Redraw(Kit_PlayerModelInformation pm, int[] customizationOptions)
            {
                //First clean up
                for (int i = 0; i < playerModelObjects.Count; i++)
                {
                    Destroy(playerModelObjects[i]);
                }
                //Create new list
                playerModelObjects = new List<GameObject>();
                //Because it erases weapon objects too, also reset that list
                weaponObjects = new List<GameObject>();
                //Instantiate new model
                GameObject newPrefab = Instantiate(pm.prefab, playerModelGo, false);
                //Reset scale
                newPrefab.transform.localScale = Vector3.one;
                //Get player model
                playerModelCurrent = newPrefab.GetComponent<Kit_ThirdPersonPlayerModel>();
                playerModelCurrent.SetCustomizations(customizationOptions, null);
                //Setup IK helper
                if (playerModelCurrent.anim)
                {
                    playerModelIkHelper = playerModelCurrent.anim.gameObject.AddComponent<Kit_LoadoutIKHelper>();
                    playerModelIkHelper.anim = playerModelCurrent.anim;
                    playerModelIkHelper.applyIk = false;
                }
                //Set Layer
                newPrefab.transform.SetLayerRecursively(gameObject.layer);
                //Add to list
                playerModelObjects.Add(newPrefab);

                if (mainIngame)
                {
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
                else if (mainMenu)
                {
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
            }

            void Redraw(Kit_WeaponBase wep, int[] attachments)
            {
                //Clean up
                for (int i = 0; i < weaponObjects.Count; i++)
                {
                    Destroy(weaponObjects[i]);
                }

                weaponObjects = new List<GameObject>();

                //Check if we have a player model
                if (playerModelCurrent)
                {
                    GameObject newWep = Instantiate(wep.thirdPersonPrefab, playerModelCurrent.weaponsInHandsGo);
                    //Set Scale
                    newWep.transform.localScale = Vector3.one;
                    //Set layer
                    newWep.transform.SetLayerRecursively(gameObject.layer);
                    //Try
                    if (newWep.GetComponent<Weapons.Kit_ThirdPersonWeaponRenderer>())
                    {
                        Weapons.Kit_ThirdPersonWeaponRenderer tpw = newWep.GetComponent<Weapons.Kit_ThirdPersonWeaponRenderer>();
                        tpw.SetAttachments(attachments, wep as Weapons.Kit_ModernWeaponScript, null, null);
                        //Show it
                        tpw.visible = true;
                        //Check if we have ik
                        playerModelIkHelper.leftHandGoal = tpw.leftHandIK;
                        if (playerModelIkHelper.leftHandGoal) playerModelIkHelper.applyIk = true;
                        else playerModelIkHelper.applyIk = false;
                    }
                    //Else dont apply IK
                    else playerModelIkHelper.applyIk = false;
                    //Add to the list
                    weaponObjects.Add(newWep);
                    //Redraw Stats
                    RedrawStats();
                    //Set model anim type
                    if (playerModelCurrent.anim.gameObject.activeInHierarchy) //Only play if animator is active
                    {
                        //Tell it to not use transitions
                        playerModelCurrent.SetAnimType(wep.thirdPersonAnimType, true);
                    }
                }
            }

     
            void RedrawStats()
            {
                Kit_GameInformation game = null;
                if (mainIngame) game = mainIngame.gameInformation;
                else game = mainMenu.gameInformation;

                if (game)
                {
                    for (int i = 0; i < weaponCategories.Length; i++)
                    {
                        if (weaponCategories[i] && allLoadouts.Length > currentLoadout && allLoadouts[currentLoadout] != null)
                        {
                            if (allLoadouts[currentLoadout].loadoutWeapons[i] != null)
                            {
                                if (game.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[i].weaponID])
                                {
                                    //Check if stats are actually supported
                                    if (game.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[i].weaponID].SupportsStats())
                                    {
                                        //Get stats
                                        WeaponStats stats = game.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[i].weaponID].GetStats();
                                        if (stats != null)
                                        {
                                            //Set name
                                            weaponCategories[i].currentWeaponImage.sprite = game.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[i].weaponID].weaponPicture;
                                            weaponCategories[i].statsDamageFill.fillAmount = stats.damage / (float)GetHighestDamageStat();
                                            weaponCategories[i].statsFireRateFill.fillAmount = stats.fireRate / (float)GetHighestFireRateStat();
                                            weaponCategories[i].statsReachFill.fillAmount = stats.reach / (float)GetHighestReachStat();
                                            weaponCategories[i].statsRecoilFill.fillAmount = stats.recoil / (float)GetHighestRecoilStat();
                                            weaponCategories[i].statsRoot.SetActiveOptimized(true);
                                        }
                                    }
                                    else
                                    {
                                        weaponCategories[i].statsRoot.SetActiveOptimized(false);
                                    }

                                    //Check for customizatio support
                                    if (game.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[i].weaponID].SupportsCustomization())
                                    {
                                        weaponCategories[i].customizeWeaponButton.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        weaponCategories[i].customizeWeaponButton.gameObject.SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void ChangeInHands(int newInHands)
            {
                //Set
                currentlyInHand = newInHands;
                if (mainIngame)
                {
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
                else if (mainMenu)
                {
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].weaponID], allLoadouts[currentLoadout].loadoutWeapons[currentlyInHand].attachments);
                }
            }

            public void ChangeClass(int newClass)
            {
               
            }

          
            bool updateDropdowns = true;

            public void WeaponCategoryDropdownChanged(int slot)
            {
           
            }


            #region Helper functions
       
            public float GetHighestDamageStat()
            {
                float highest = 0.1f;
                if (mainIngame)
                {
                    for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainIngame.gameInformation.allWeapons[i].GetStats().damage > highest)
                        {
                            highest = mainIngame.gameInformation.allWeapons[i].GetStats().damage;
                        }
                    }
                }
                else if (mainMenu)
                {
                    for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainMenu.gameInformation.allWeapons[i].GetStats().damage > highest)
                        {
                            highest = mainMenu.gameInformation.allWeapons[i].GetStats().damage;
                        }
                    }
                }
                return highest;
            }

        
            public float GetHighestFireRateStat()
            {
                float highest = 0.1f;
                if (mainIngame)
                {
                    for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainIngame.gameInformation.allWeapons[i].GetStats().fireRate > highest)
                        {
                            highest = mainIngame.gameInformation.allWeapons[i].GetStats().fireRate;
                        }
                    }
                }
                else if (mainMenu)
                {
                    for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainMenu.gameInformation.allWeapons[i].GetStats().fireRate > highest)
                        {
                            highest = mainMenu.gameInformation.allWeapons[i].GetStats().fireRate;
                        }
                    }
                }
                return highest;
            }

            public float GetHighestRecoilStat()
            {
                float highest = 0.1f;
                if (mainIngame)
                {
                    for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainIngame.gameInformation.allWeapons[i].GetStats().recoil > highest)
                        {
                            highest = mainIngame.gameInformation.allWeapons[i].GetStats().recoil;
                        }
                    }
                }
                else if (mainMenu)
                {
                    for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainMenu.gameInformation.allWeapons[i].GetStats().recoil > highest)
                        {
                            highest = mainMenu.gameInformation.allWeapons[i].GetStats().recoil;
                        }
                    }
                }
                return highest;
            }

            public float GetHighestReachStat()
            {
                float highest = 0.1f;
                if (mainIngame)
                {
                    for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainIngame.gameInformation.allWeapons[i].GetStats().reach > highest)
                        {
                            highest = mainIngame.gameInformation.allWeapons[i].GetStats().reach;
                        }
                    }
                }
                else if (mainMenu)
                {
                    for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                    {
                        if (mainMenu.gameInformation.allWeapons[i].GetStats().reach > highest)
                        {
                            highest = mainMenu.gameInformation.allWeapons[i].GetStats().reach;
                        }
                    }
                }
                return highest;
            }
            #endregion

            void SetWeaponInSlot(int slot, int id)
            {
                Kit_GameInformation game = null;
                if (mainIngame) game = mainIngame.gameInformation;
                else game = mainMenu.gameInformation;
                if (game.allWeapons[id].IsWeaponUnlocked(game))
                {
                    allLoadouts[currentLoadout].loadoutWeapons[slot].weaponID = id;
                    //Update Dropdown
                    weaponCategories[slot].weaponsInDropdown.value = weaponCategories[slot].dropdownLocalToGlobal.IndexOf(id);
                    weaponCategories[slot].currentWeaponImage.sprite = game.allWeapons[id].weaponPicture;
                    Weapons.Kit_WeaponRenderer renderer = null;
                    if (mainIngame)
                    {
                        //Get Renderer
                        renderer = mainIngame.gameInformation.allWeapons[id].firstPersonPrefab.GetComponent<Kit_WeaponRenderer>();
                    }
                    else if (mainMenu)
                    {
                        //Get Renderer
                        renderer = mainMenu.gameInformation.allWeapons[id].firstPersonPrefab.GetComponent<Kit_WeaponRenderer>();
                    }

                    if (renderer)
                    {
                        allLoadouts[currentLoadout].loadoutWeapons[slot].attachments = new int[renderer.attachmentSlots.Length];
                    }
                    else
                    {
                        allLoadouts[currentLoadout].loadoutWeapons[slot].attachments = new int[0];
                    }
                }
                else
                {
                    //Set the dropdown back
                    weaponCategories[slot].weaponsInDropdown.value = weaponCategories[slot].dropdownLocalToGlobal.IndexOf(allLoadouts[currentLoadout].loadoutWeapons[slot].weaponID);
                }
            }

            public void CustomizeWeaponInSlot(int slot)
            {
      
            }

            void SetupCustomization(Kit_WeaponBase inf, int[] attachments)
            {
      
            }

            void UpdateCustomization(int[] attachments, Weapons.Kit_ModernWeaponScript wep)
            {
              
            }

            public void ProceedToCustomization(int id)
            {
        
            }

            public void BackToSelection()
            {
                //Enable Loadout menu
                root.SetActive(true);
                //Activate selection page
                weaponSelectionRoot.SetActive(true);
                weaponCustomizationRoot.SetActive(false);
                playerModelCustomizationRoot.SetActive(false);
                //Redraw team
                FullRedraw();
            }
        }
    }
}