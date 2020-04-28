using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        /// <summary>
        /// Input that is allowed for the given weapon slot
        /// </summary>
        [System.Serializable]
        public class WeaponManagerSlot
        {
            /// <summary>
            /// Can weapons in this slot be equipped?
            /// </summary>
            public bool enableEquipping = true;
            /// <summary>
            /// ID of <see cref="Kit_PlayerInput.weaponSlotUses"/>
            /// </summary>
            public int equippingInputID;
            /// <summary>
            /// Can weapons in this slot be quick used (e.g. quick grenades, quick knife)
            /// </summary>
            public bool enableQuickUse;
            /// <summary>
            /// ID of <see cref="Kit_PlayerInput.weaponSlotUses"/>
            /// </summary>
            public int quickUseInputID;
            /// <summary>
            /// When <see cref="maxAmountOfWeaponsInSlot"/> is > 1 and <see cref="enableQuickUse"/> is set to true, this key can be used to iterate through them
            /// </summary>
            public int quickUseIterationKey;
        }

        /// <summary>
        /// This will store runtime data for the controlling player
        /// </summary>
        public class WeaponManagerControllerRuntimeData
        {
            /// <summary>
            /// Our currently selected weapon; [0] = slot; [1] = weapon In Slot
            /// </summary>
            public int[] currentWeapon = new int[2];

            /// <summary>
            /// The weapon we want to select
            /// </summary>
            public int[] desiredWeapon = new int[2];

            /// <summary>
            /// Desired weapon is locked (by plugin?)
            /// </summary>
            public bool isDesiredWeaponLocked;

            /// <summary>
            /// Is a quick use in progress?
            /// </summary>
            public bool quickUseInProgress;
            /// <summary>
            /// Quick use that we want to do!
            /// </summary>
            public int[] desiredQuickUse = new int[2];
            /// <summary>
            /// Current state of quick use.
            /// </summary>
            public int quickUseState;
            /// <summary>
            /// When is the next quick use state over?
            /// </summary>
            public float quickUseOverAt;
            /// <summary>
            /// Sync!
            /// </summary>
            public bool quickUseSyncButtonWaitOver;

            /// <summary>
            /// The data of our two weapons that are in use. None of these should ever be null.
            /// </summary>
            public WeaponSlotReference[] weaponsInUse = new WeaponSlotReference[2];

            /// <summary>
            /// Last states for the slot buttons!
            /// </summary>
            public bool[] lastInputIDs;
            /// <summary>
            /// Last state for the drop weapon
            /// </summary>
            public bool lastDropWeapon;

            /// <summary>
            /// Are we currently switching weapons?
            /// </summary>
            public bool switchInProgress;
            /// <summary>
            /// When is the next switching phase over?
            /// </summary>
            public float switchNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of switching
            /// </summary>
            public int switchPhase;
            /// <summary>
            /// Raycast hit for the pickup process
            /// </summary>
            public RaycastHit hit;

            #region IK
            /// <summary>
            /// Weight of the left hand IK
            /// </summary>
            public float leftHandIKWeight;
            #endregion
        }

        /// <summary>
        /// This contains the reference to a generic weapon.
        /// </summary>
        public class WeaponSlotReference
        {
            public int selectedSlot;
            public int selectedQuickUse;
            public WeaponReference[] weaponsInSlot;
            /// <summary>
            /// If this is true, those were injected from a plugin and cannot be manually selected.
            /// </summary>
            public bool isInjectedFromPlugin;
        }

        public class WeaponReference
        {
            public Kit_WeaponBase behaviour;
            public object runtimeData;
            public int[] attachments;
        }

        /// <summary>
        /// Other players will keep this runtime data, to replicate the behaviour based on what this player tells them
        /// </summary>
        public class WeaponManagerControllerOthersRuntimeData
        {
            //Our currently selected weapon
            public int[] currentWeapon = new int[2];

            //The weapon we want to select
            public int[] desiredWeapon = new int[2];

            /// <summary>
            /// Is a quick use in progress?
            /// </summary>
            public bool quickUseInProgress;
            /// <summary>
            /// Quick use that we want to do!
            /// </summary>
            public int[] desiredQuickUse = new int[2];
            /// <summary>
            /// Current state of quick use.
            /// </summary>
            public int quickUseState;
            /// <summary>
            /// When is the next quick use state over?
            /// </summary>
            public float quickUseOverAt;
            /// <summary>
            /// Sync!
            /// </summary>
            public bool quickUseSyncButtonWaitOver;

            //The data of our two weapons that are in use. None of these should ever be null.
            public WeaponSlotReference[] weaponsInUse = new WeaponSlotReference[2];

            /// <summary>
            /// Are we currently switching weapons?
            /// </summary>
            public bool switchInProgress;
            /// <summary>
            /// When is the next switching phase over?
            /// </summary>
            public float switchNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of switching
            /// </summary>
            public int switchPhase;

            #region IK
            /// <summary>
            /// Weight of the left hand IK
            /// </summary>
            public float leftHandIKWeight;
            #endregion
        }

        public enum DeadDrop { None, Selected, All }

        public class Kit_ModernWeaponManager : Kit_WeaponManagerBase
        {
            public WeaponManagerSlot[] slotConfiguration;
      
            public GameObject dropPrefab;
        
            public LayerMask pickupLayers;
       
            public float pickupDistance = 3f;
   
            public DeadDrop uponDeathDrop;
   
            public float weaponPositionChangeSpeed = 5f;
            public bool allowSwitchingWhileRunning;
            public bool allowQuickUseWhileRunning;

            public override void SetupManager(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerRuntimeData runtimeData = new WeaponManagerControllerRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                //Hide crosshair
                pb.main.hud.DisplayCrosshair(0f);

                //Setup input IDs
                runtimeData.lastInputIDs = new bool[pb.input.weaponSlotUses.Length];

                int amountOfWeapons = (int)instantiationData[1];
                Debug.Log("[Weapon Manager] Setup Local Begins With " + amountOfWeapons + " Weapons");

                //Determine how many slots are going to be used!
                int highestSlot = 0;

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    if ((int)table["slot"] > highestSlot)
                    {
                        highestSlot = (int)table["slot"];
                    }
                }

                //Increase by one (is length, so highest slot is Length - 1)
                highestSlot++;

                //PLUGIN INJECTION
                //KEY = WEAPON; VALUE = ID OF PLUGIN
                List<WeaponsFromPlugin> weaponsFromPlugins = new List<WeaponsFromPlugin>();
                List<Kit_WeaponInjection> pluginWeaponsCallback = new List<Kit_WeaponInjection>();

                for (int i = 0; i < pb.main.gameInformation.plugins.Length; i++)
                {
                    WeaponsFromPlugin weapons = pb.main.gameInformation.plugins[i].WeaponsToInjectIntoWeaponManager(pb);
                    if (weapons.weaponsInSlot.Length > 0)
                    {
                        int id = i;
                        pluginWeaponsCallback.Add(pb.main.gameInformation.plugins[id]);
                        weaponsFromPlugins.Add(weapons);
                    }
                }

                WeaponsFromPlugin movementWeapons = pb.movement.WeaponsToInjectIntoWeaponManager(pb);
                if (movementWeapons != null && movementWeapons.weaponsInSlot.Length > 0)
                {
                    pluginWeaponsCallback.Add(pb.movement);
                    weaponsFromPlugins.Add(movementWeapons);
                }

                runtimeData.weaponsInUse = new WeaponSlotReference[highestSlot + weaponsFromPlugins.Count];
                //PLUGIN INJECTION END

                //Setup
                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    runtimeData.weaponsInUse[i] = new WeaponSlotReference();
                }

                //Now determine how many weapons go in each slot
                int[] weaponsInEachSlot = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    //Add!
                    weaponsInEachSlot[slot]++;
                }

                //Setup length of slots
                for (int i = 0; i < highestSlot; i++)
                {
                    runtimeData.weaponsInUse[i].weaponsInSlot = new WeaponReference[weaponsInEachSlot[i]];
                }

                //Now, setup weapons

                int[] slotsUsed = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    int id = (int)table["id"];
                    int[] attachments = (int[])table["attachments"];

                    //Get their behaviour modules
                    Kit_WeaponBase weaponBehaviour = pb.gameInformation.allWeapons[id];
                    //Setup values
                    weaponBehaviour.SetupValues(id);
                    //Setup Reference
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]] = new WeaponReference();
                    //Assign Behaviour
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].behaviour = weaponBehaviour;
                    //Setup FP
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData = weaponBehaviour.SetupFirstPerson(pb, attachments);
                    //Assign attachments
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].attachments = attachments;
                    if (weaponBehaviour is Kit_ModernWeaponScript)
                    {
                        //Setup TP
                        weaponBehaviour.SetupThirdPerson(pb, weaponBehaviour as Kit_ModernWeaponScript, runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData, attachments);
                    }
                    else
                    {
                        //Setup TP
                        weaponBehaviour.SetupThirdPerson(pb, null, runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData, attachments);
                    }
                    //Increase Slot!
                    slotsUsed[slot]++;
                }

                //PLUGIN INJECTION
                for (int i = 0; i < weaponsFromPlugins.Count; i++)
                {
                    int slot = highestSlot + i;
                    runtimeData.weaponsInUse[slot].isInjectedFromPlugin = true;

                    runtimeData.weaponsInUse[slot].weaponsInSlot = new WeaponReference[weaponsFromPlugins[i].weaponsInSlot.Length];

                    for (int p = 0; p < weaponsFromPlugins[i].weaponsInSlot.Length; p++)
                    {
                        Kit_WeaponBase weaponBehaviour = weaponsFromPlugins[i].weaponsInSlot[p].weapon;
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p] = new WeaponReference();
                        //Assign Behaviour
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].behaviour = weaponBehaviour;
                        //Setup FP
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData = weaponBehaviour.SetupFirstPerson(pb, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        //Assign attachments
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].attachments = weaponsFromPlugins[i].weaponsInSlot[p].attachments;
                        if (weaponBehaviour is Kit_ModernWeaponScript)
                        {
                            //Setup TP
                            weaponBehaviour.SetupThirdPerson(pb, weaponBehaviour as Kit_ModernWeaponScript, runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                        else
                        {
                            //Setup TP
                            weaponBehaviour.SetupThirdPerson(pb, null, runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                    }

                    //Call plugin
                    pluginWeaponsCallback[i].ReportSlotOfInjectedWeapons(pb, slot);
                }
                //END

                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    if (slotConfiguration[i].enableEquipping)
                    {
                        //Select current weapon
                        runtimeData.weaponsInUse[i].weaponsInSlot[i].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[i].weaponsInSlot[0].runtimeData);
                        //Set current weapon
                        runtimeData.currentWeapon[0] = i;
                        runtimeData.currentWeapon[1] = 0;
                        break;
                    }
                }
                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            public override void SetupManagerBot(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerRuntimeData runtimeData = new WeaponManagerControllerRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                //Setup input IDs
                runtimeData.lastInputIDs = new bool[pb.input.weaponSlotUses.Length];

                int amountOfWeapons = (int)instantiationData[1];
                Debug.Log("[Weapon Manager] Setup Local (BOT) Begins With " + amountOfWeapons + " Weapons");

                //Determine how many slots are going to be used!
                int highestSlot = 0;

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    if ((int)table["slot"] > highestSlot)
                    {
                        highestSlot = (int)table["slot"];
                    }
                }

                //Increase by one (is length, so highest slot is Length - 1)
                highestSlot++;
                //PLUGIN INJECTION
                List<WeaponsFromPlugin> weaponsFromPlugins = new List<WeaponsFromPlugin>();
                List<Kit_WeaponInjection> pluginWeaponsCallback = new List<Kit_WeaponInjection>();

                for (int i = 0; i < pb.main.gameInformation.plugins.Length; i++)
                {
                    WeaponsFromPlugin weapons = pb.main.gameInformation.plugins[i].WeaponsToInjectIntoWeaponManager(pb);
                    if (weapons.weaponsInSlot.Length > 0)
                    {
                        int id = i;
                        pluginWeaponsCallback.Add(pb.main.gameInformation.plugins[id]);
                        weaponsFromPlugins.Add(weapons);
                    }
                }

                WeaponsFromPlugin movementWeapons = pb.movement.WeaponsToInjectIntoWeaponManager(pb);
                if (movementWeapons != null && movementWeapons.weaponsInSlot.Length > 0)
                {
                    pluginWeaponsCallback.Add(pb.movement);
                    weaponsFromPlugins.Add(movementWeapons);
                }

                //Setup Slot Length!
                runtimeData.weaponsInUse = new WeaponSlotReference[highestSlot + weaponsFromPlugins.Count];
                //PLUGIN INJECTION END

                //Setup
                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    runtimeData.weaponsInUse[i] = new WeaponSlotReference();
                }

                //Now determine how many weapons go in each slot
                int[] weaponsInEachSlot = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    //Add!
                    weaponsInEachSlot[slot]++;
                }

                //Setup length of slots!
                for (int i = 0; i < highestSlot; i++)
                {
                    runtimeData.weaponsInUse[i].weaponsInSlot = new WeaponReference[weaponsInEachSlot[i]];
                }

                //Now, setup weapons!

                int[] slotsUsed = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    int id = (int)table["id"];
                    int[] attachments = (int[])table["attachments"];

                    //Get their behaviour modules
                    Kit_WeaponBase weaponBehaviour = pb.gameInformation.allWeapons[id];
                    //Setup values
                    weaponBehaviour.SetupValues(id);
                    //Setup Reference
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]] = new WeaponReference();
                    //Assign Behaviour
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].behaviour = weaponBehaviour;
                    //Setup FP
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData = weaponBehaviour.SetupFirstPerson(pb, attachments);
                    //Assign attachments
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].attachments = attachments;
                    if (weaponBehaviour is Kit_ModernWeaponScript)
                    {
                        //Setup TP
                        weaponBehaviour.SetupThirdPerson(pb, weaponBehaviour as Kit_ModernWeaponScript, runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData, attachments);
                    }
                    else
                    {
                        //Setup TP
                        weaponBehaviour.SetupThirdPerson(pb, null, runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData, attachments);
                    }
                    //Increase Slot!
                    slotsUsed[slot]++;
                }

                //PLUGIN INJECTION
                for (int i = 0; i < weaponsFromPlugins.Count; i++)
                {
                    int slot = highestSlot + i;
                    runtimeData.weaponsInUse[slot].isInjectedFromPlugin = true;

                    runtimeData.weaponsInUse[slot].weaponsInSlot = new WeaponReference[weaponsFromPlugins[i].weaponsInSlot.Length];

                    for (int p = 0; p < weaponsFromPlugins[i].weaponsInSlot.Length; p++)
                    {
                        Kit_WeaponBase weaponBehaviour = weaponsFromPlugins[i].weaponsInSlot[p].weapon;
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p] = new WeaponReference();
                        //Assign Behaviour
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].behaviour = weaponBehaviour;
                        //Setup FP
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData = weaponBehaviour.SetupFirstPerson(pb, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        //Assign attachments
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].attachments = weaponsFromPlugins[i].weaponsInSlot[p].attachments;
                        if (weaponBehaviour is Kit_ModernWeaponScript)
                        {
                            //Setup TP
                            weaponBehaviour.SetupThirdPerson(pb, weaponBehaviour as Kit_ModernWeaponScript, runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                        else
                        {
                            //Setup TP
                            weaponBehaviour.SetupThirdPerson(pb, null, runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        //Call plugin
                        pluginWeaponsCallback[i].ReportSlotOfInjectedWeapons(pb, slot);
                    }
                }
                //END

                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    if (slotConfiguration[i].enableEquipping)
                    {
                        //Select current weapon
                        runtimeData.weaponsInUse[i].weaponsInSlot[i].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[i].weaponsInSlot[0].runtimeData);
                        //Set current weapon
                        runtimeData.currentWeapon[0] = i;
                        runtimeData.currentWeapon[1] = 0;
                        break;
                    }
                }
                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            public override void SetupManagerOthers(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerOthersRuntimeData runtimeData = new WeaponManagerControllerOthersRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                int amountOfWeapons = (int)instantiationData[1];
                Debug.Log("[Weapon Manager] Setup Others Begins With " + amountOfWeapons + " Weapons");

                //Determine how many slots are going to be used!
                int highestSlot = 0;

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    if ((int)table["slot"] > highestSlot)
                    {
                        highestSlot = (int)table["slot"];
                    }
                }

                //Increase by one (is length, so highest slot is Length - 1)
                highestSlot++;

                //PLUGIN INJECTION
                List<WeaponsFromPlugin> weaponsFromPlugins = new List<WeaponsFromPlugin>();

                for (int i = 0; i < pb.main.gameInformation.plugins.Length; i++)
                {
                    WeaponsFromPlugin weapons = pb.main.gameInformation.plugins[i].WeaponsToInjectIntoWeaponManager(pb);
                    if (weapons.weaponsInSlot.Length > 0)
                    {
                        weaponsFromPlugins.Add(weapons);
                    }
                }

                WeaponsFromPlugin movementWeapons = pb.movement.WeaponsToInjectIntoWeaponManager(pb);
                if (movementWeapons != null && movementWeapons.weaponsInSlot.Length > 0)
                {
                    weaponsFromPlugins.Add(movementWeapons);
                }

                //Setup Slot Length!
                runtimeData.weaponsInUse = new WeaponSlotReference[highestSlot + weaponsFromPlugins.Count];
                //PLUGIN INJECTION END

                //Setup
                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    runtimeData.weaponsInUse[i] = new WeaponSlotReference();
                }

                //Now determine how many weapons go in each slot
                int[] weaponsInEachSlot = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    //Add!
                    weaponsInEachSlot[slot]++;
                }

                //Setup length of slots!
                for (int i = 0; i < highestSlot; i++)
                {
                    runtimeData.weaponsInUse[i].weaponsInSlot = new WeaponReference[weaponsInEachSlot[i]];
                }

                //Now, setup weapons!

                int[] slotsUsed = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    int id = (int)table["id"];
                    int[] attachments = (int[])table["attachments"];

                    //Get their behaviour modules
                    Kit_WeaponBase weaponBehaviour = pb.gameInformation.allWeapons[id];
                    //Setup values
                    weaponBehaviour.SetupValues(id);
                    //Setup Reference
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]] = new WeaponReference();
                    //Assign Behaviour
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].behaviour = weaponBehaviour;
                    if (weaponBehaviour is Kit_ModernWeaponScript)
                    {
                        //Setup TP
                        runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData = weaponBehaviour.SetupThirdPersonOthers(pb, weaponBehaviour as Kit_ModernWeaponScript, attachments);
                    }
                    else
                    {
                        //Setup TP
                        runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData = weaponBehaviour.SetupThirdPersonOthers(pb, null, attachments);
                    }
                    //Assign attachments
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].attachments = attachments;
                    //Increase Slot!
                    slotsUsed[slot]++;
                }

                //PLUGIN INJECTION
                for (int i = 0; i < weaponsFromPlugins.Count; i++)
                {
                    int slot = highestSlot + i;
                    runtimeData.weaponsInUse[slot].isInjectedFromPlugin = true;

                    runtimeData.weaponsInUse[slot].weaponsInSlot = new WeaponReference[weaponsFromPlugins[i].weaponsInSlot.Length];

                    for (int p = 0; p < weaponsFromPlugins[i].weaponsInSlot.Length; p++)
                    {
                        Kit_WeaponBase weaponBehaviour = weaponsFromPlugins[i].weaponsInSlot[p].weapon;
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p] = new WeaponReference();
                        //Assign Behaviour
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].behaviour = weaponBehaviour;
                        //Assign attachments
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].attachments = weaponsFromPlugins[i].weaponsInSlot[p].attachments;
                        if (weaponBehaviour is Kit_ModernWeaponScript)
                        {
                            //Setup TP
                            runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData = weaponBehaviour.SetupThirdPersonOthers(pb, weaponBehaviour as Kit_ModernWeaponScript, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                        else
                        {
                            //Setup TP
                            runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData = weaponBehaviour.SetupThirdPersonOthers(pb, null, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        }
                    }
                }
                //END

                //Select current weapon
                runtimeData.weaponsInUse[0].weaponsInSlot[0].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[0].weaponsInSlot[0].runtimeData);
                //Set current weapon
                runtimeData.currentWeapon[0] = 0;
                runtimeData.currentWeapon[1] = 0;
                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            public override void ForceUnselectCurrentWeapon(Kit_PlayerBehaviour pb)
            {
                Debug.Log("[Weapon Manager] Forcing unselect of current weapon!");
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Try to find next weapon
                    int[] next = new int[2] { -1, -1 };
                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                            {
                                //Check if this one works!
                                if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                                {
                                    int id = i;
                                    int idTwo = o;
                                    next[0] = id;
                                    next[1] = idTwo;
                                    //We found one
                                    break;
                                }
                            }
                        }
                    }

                    //This should ALWAYS be true
                    if (next[0] >= 0 && next[1] >= 0)
                    {
                        runtimeData.desiredWeapon[0] = next[0];
                        runtimeData.desiredWeapon[1] = next[1];
                        //Begin switch and skip putaway
                        runtimeData.switchInProgress = true;
                        //Set time (Because here we cannot use a coroutine)
                        runtimeData.switchNextEnd = 0f;
                        //Set phase
                        runtimeData.switchPhase = 0;
                        if (!pb.isBot)
                        {
                            //Hide crosshair
                            pb.main.hud.DisplayCrosshair(0f);
                        }
                        //Set current one too!
                        runtimeData.currentWeapon[0] = next[0];
                        runtimeData.currentWeapon[1] = next[1];
                    }
                    else
                    {
                        Debug.LogError("Could not find next weapon! This is not allowed!");
                    }
                }
            }

            public override void CustomUpdate(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    if ((LockCursor.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        if (!runtimeData.isDesiredWeaponLocked)
                        {
                            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                            {
                                if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                {
                                    if (slotConfiguration[i].enableEquipping && slotConfiguration[i].equippingInputID >= 0 && !runtimeData.quickUseInProgress)
                                    {
                                        if (runtimeData.lastInputIDs[slotConfiguration[i].equippingInputID] != pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID])
                                        {
                                            runtimeData.lastInputIDs[slotConfiguration[i].equippingInputID] = pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID];
                                            //Check for input
                                            if (pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID] && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                            {
                                                int id = i;
                                                if (runtimeData.desiredWeapon[0] != id)
                                                {
                                                    if (runtimeData.weaponsInUse[i].weaponsInSlot[0].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[0].runtimeData))
                                                    {
                                                        runtimeData.desiredWeapon[0] = id;
                                                        runtimeData.desiredWeapon[1] = 0;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    int next = runtimeData.desiredWeapon[1] + 1;
                                                    if (next >= runtimeData.weaponsInUse[id].weaponsInSlot.Length)
                                                    {
                                                        next = 0;
                                                    }
                                                    if (runtimeData.weaponsInUse[i].weaponsInSlot[next].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[next].runtimeData))
                                                    {
                                                        runtimeData.desiredWeapon[1] = next;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Check if we can do a quick use!
                            if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0] && runtimeData.currentWeapon[1] == runtimeData.currentWeapon[1] && !runtimeData.quickUseInProgress && !runtimeData.switchInProgress)
                            {
                                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                                {
                                    if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                    {
                                        int slot = i;
                                        if (slotConfiguration[slot].enableQuickUse)
                                        {
                                            if (slotConfiguration[slot].quickUseIterationKey >= 0)
                                            {
                                                if (runtimeData.lastInputIDs[slotConfiguration[slot].quickUseIterationKey] != pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey])
                                                {
                                                    runtimeData.lastInputIDs[slotConfiguration[slot].quickUseIterationKey] = pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey];

                                                    int id = i;
                                                    if (pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey])
                                                    {
                                                        runtimeData.weaponsInUse[id].selectedQuickUse++;
                                                        if (runtimeData.weaponsInUse[id].selectedQuickUse >= runtimeData.weaponsInUse[id].weaponsInSlot.Length)
                                                        {
                                                            runtimeData.weaponsInUse[id].selectedQuickUse = 0;
                                                        }
                                                    }
                                                }
                                            }

                                            if (slotConfiguration[slot].quickUseInputID >= 0)
                                            {
                                                if (runtimeData.lastInputIDs[slotConfiguration[slot].quickUseInputID] != pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID])
                                                {
                                                    runtimeData.lastInputIDs[slotConfiguration[slot].quickUseInputID] = pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID];
                                                    //Check for input
                                                    if (pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID] && (allowQuickUseWhileRunning || !pb.movement.IsRunning(pb)))
                                                    {
                                                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[runtimeData.weaponsInUse[slot].selectedQuickUse].behaviour.SupportsQuickUse(pb, runtimeData.weaponsInUse[slot].weaponsInSlot[runtimeData.weaponsInUse[slot].selectedQuickUse].runtimeData))
                                                        {
                                                            runtimeData.desiredQuickUse[0] = slot;
                                                            runtimeData.desiredQuickUse[1] = runtimeData.weaponsInUse[slot].selectedQuickUse;
                                                            runtimeData.quickUseInProgress = true;
                                                            //Also reset these!
                                                            runtimeData.quickUseState = 0;
                                                            runtimeData.quickUseOverAt = Time.time;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Physics.Raycast(pb.playerCameraTransform.position, pb.playerCameraTransform.forward, out runtimeData.hit, pickupDistance, pickupLayers.value))
                        {
                            if (runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>())
                            {
                                Kit_DropBehaviour drop = runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>();
                                if (!pb.isBot)
                                {
                                    pb.main.hud.DisplayWeaponPickup(true, drop.weaponID);
                                    pb.main.hud.DisplayInteraction(false);
                                }
                                if (runtimeData.lastDropWeapon != pb.input.dropWeapon)
                                {
                                    runtimeData.lastDropWeapon = pb.input.dropWeapon;
                                    if (pb.input.dropWeapon && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                    {
                                        int[] slots = new int[2];

                                        if (pb.main.gameInformation.allWeapons[drop.weaponID].canFitIntoSlots.Contains(runtimeData.currentWeapon[0]))
                                        {
                                            slots[0] = runtimeData.currentWeapon[0];
                                            slots[1] = runtimeData.currentWeapon[1];
                                        }
                                        else
                                        {
                                            slots[0] = pb.main.gameInformation.allWeapons[drop.weaponID].canFitIntoSlots[0];
                                            slots[1] = 0;
                                        }

                                        //Check if we can drop
                                        if (!drop.isSceneOwned || drop.isSceneOwned && pb.main.gameInformation.enableDropWeaponOnSceneSpawnedWeapons)
                                        {
                                            //First drop our weapon
                                            DropWeapon(pb, slots[0], slots[1], drop.transform);
                                        }

                                        //Pickup new weapon
                                        pb.photonView.RPC("ReplaceWeapon", RpcTarget.AllBuffered, slots, drop.weaponID, drop.bulletsLeft, drop.bulletsLeftToReload, drop.attachments);
                                        //First hide
                                        drop.rendererRoot.SetActive(false);
                                        if (drop.isSceneOwned)
                                        {
                                            //Delete object
                                            drop.photonView.RPC("PickedUp", PhotonNetwork.MasterClient);
                                        }
                                        else
                                        {
                                            //Delete object
                                            drop.photonView.RPC("PickedUp", drop.photonView.Owner);
                                        }
                                    }
                                }
                            }
                            else if (!pb.isBot)
                            {
                                pb.main.hud.DisplayWeaponPickup(false);
                                pb.main.hud.DisplayInteraction(false);
                            }
                        }
                        else if (!pb.isBot)
                        {
                            pb.main.hud.DisplayWeaponPickup(false);
                            pb.main.hud.DisplayInteraction(false);
                        }
                    }

                    //Update weapon animation
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.AnimateWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, pb.movement.GetCurrentWeaponMoveAnimation(pb), pb.movement.GetCurrentWalkAnimationSpeed(pb));

                    //Move weapons transform
                    pb.weaponsGo.localPosition = Vector3.Lerp(pb.weaponsGo.localPosition, Vector3.zero + pb.looking.GetWeaponOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    //Move weapons transform
                    pb.weaponsGo.localRotation = Quaternion.Slerp(pb.weaponsGo.localRotation, pb.looking.GetWeaponRotationOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    //Quick use has priority!
                    if (runtimeData.quickUseInProgress)
                    {
                        if (Time.time >= runtimeData.quickUseOverAt)
                        {
                            //First, put away current weapon!
                            if (runtimeData.quickUseState == 0)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.QuickUseSkipsPutaway(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData))
                                {
                                    //Set time (Because here we cannot use a coroutine)
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                    //Start putaway
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    if (!pb.isBot)
                                    {
                                        //Hide crosshair
                                        pb.main.hud.DisplayCrosshair(0f);
                                    }
                                }
                                else
                                {
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                    if (!pb.isBot)
                                    {
                                        //Hide crosshair
                                        pb.main.hud.DisplayCrosshair(0f);
                                    }
                                }
                            }
                            else if (runtimeData.quickUseState == 1)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                //Set state
                                runtimeData.quickUseState = 2;

                                //Begin quick use....
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.BeginQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else if (runtimeData.quickUseState == 2)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.WaitForQuickUseButtonRelease() || !pb.input.weaponSlotUses[slotConfiguration[runtimeData.desiredQuickUse[0]].quickUseInputID])
                                {
                                    runtimeData.quickUseSyncButtonWaitOver = true;
                                    //Set State
                                    runtimeData.quickUseState = 3;
                                    //End quick use...
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                }
                            }
                            else if (runtimeData.quickUseState == 3)
                            {
                                //Hide Quick Use!
                                runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUseAfter(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                //Check if currently selected  weapon is valid.
                                if (runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData))
                                {
                                    //Set weapon
                                    if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0])
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];
                                    }
                                    else
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = 0;
                                    }
                                }
                                else
                                {
                                    //Its not, find a new one
                                    int[] next = new int[2] { -1, -1 };
                                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                                    {
                                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                        {
                                            //Check if this one works!
                                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                                            {
                                                int id = i;
                                                int idTwo = o;
                                                next[0] = id;
                                                next[1] = idTwo;
                                                //We found one
                                                break;
                                            }
                                        }
                                    }

                                    //This should ALWAYS be true!
                                    if (next[0] >= 0 && next[1] >= 0)
                                    {
                                        runtimeData.desiredWeapon[0] = next[0];
                                        runtimeData.desiredWeapon[1] = next[1];
                                        //Set current one too!
                                        runtimeData.currentWeapon[0] = next[0];
                                        runtimeData.currentWeapon[1] = next[1];
                                    }
                                    else
                                    {
                                        Debug.LogError("Could not find next weapon! This is not allowed!");
                                    }
                                }
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                //Set phase
                                runtimeData.quickUseState = 4;
                                //Set time
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.quickUseState == 4)
                            {
                                //End quick use
                                runtimeData.quickUseInProgress = false;
                                runtimeData.desiredQuickUse[0] = -1;
                                runtimeData.desiredQuickUse[1] = -1;
                                runtimeData.quickUseSyncButtonWaitOver = false;

                                //Also reset switching just to be sure!
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                    }
                    else
                    {
                        if (!runtimeData.switchInProgress)
                        {
                            //If we aren't switching weapons, update weapon behaviour
                            runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CalculateWeaponUpdate(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                            //Check if we want to select a different weapon
                            if (runtimeData.desiredWeapon[0] != runtimeData.currentWeapon[0] || runtimeData.desiredWeapon[0] == runtimeData.currentWeapon[0] && runtimeData.desiredWeapon[1] != runtimeData.currentWeapon[1])
                            {
                                //If not, start to switch
                                runtimeData.switchInProgress = true;
                                //Set time (Because here we cannot use a coroutine)
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                //Set phase
                                runtimeData.switchPhase = 0;
                                //Start putaway
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                if (!pb.isBot)
                                {
                                    //Hide crosshair
                                    pb.main.hud.DisplayCrosshair(0f);
                                }
                            }
                        }
                        else
                        {
                            //Switching, courtine less
                            #region Switching
                            //Check for time
                            if (Time.time >= runtimeData.switchNextEnd)
                            {
                                //Time is over, check which phase is next
                                if (runtimeData.switchPhase == 0)
                                {
                                    //Weapon has been put away, hide weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                    runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                    runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];

                                    //Draw that weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Set phase
                                    runtimeData.switchPhase = 1;
                                    //Set time
                                    runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                    //Done, now wait
                                }
                                else if (runtimeData.switchPhase == 1)
                                {
                                    //Switching is over
                                    runtimeData.switchPhase = 0;
                                    runtimeData.switchNextEnd = 0f;
                                    runtimeData.switchInProgress = false;
                                }
                            }
                            #endregion
                        }
                    }

                    if (!pb.isBot)
                    {
                        pb.main.hud.DisplayWeaponsAndQuickUses(pb, runtimeData);
                    }
                }
            }

            public override void CustomUpdateOthers(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Quick use has priority!
                    if (runtimeData.quickUseInProgress)
                    {
                        if (Time.time >= runtimeData.quickUseOverAt)
                        {
                            //First, put away current weapon!
                            if (runtimeData.quickUseState == 0)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.QuickUseSkipsPutaway(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData))
                                {
                                    //Set time (Because here we cannot use a coroutine)
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                    //Start putaway
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                }
                                else
                                {
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                }
                            }
                            else if (runtimeData.quickUseState == 1)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHideOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                //Set state
                                runtimeData.quickUseState = 2;

                                //Begin quick use....
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.BeginQuickUseOthers(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else if (runtimeData.quickUseState == 2)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.WaitForQuickUseButtonRelease() || runtimeData.quickUseSyncButtonWaitOver)
                                {
                                    //Set State
                                    runtimeData.quickUseState = 3;
                                    //End quick use...
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUseOthers(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                }
                            }
                            else if (runtimeData.quickUseState == 3)
                            {
                                //Hide Quick Use!
                                runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUseAfterOthers(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                //Set phase
                                runtimeData.quickUseState = 4;
                                //Set time
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.quickUseState == 4)
                            {
                                //End quick use
                                runtimeData.quickUseInProgress = false;
                                runtimeData.desiredQuickUse[0] = -1;
                                runtimeData.desiredQuickUse[1] = -1;

                                //Also reset switching just to be sure!
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                    }
                    else
                    {
                        //Reset quick use
                        runtimeData.quickUseState = 0;
                        if (!runtimeData.switchInProgress)
                        {
                            //If we aren't switching weapons, update weapon behaviour
                            runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CalculateWeaponUpdateOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                            //Check if we want to select a different weapon
                            if (runtimeData.desiredWeapon[0] != runtimeData.currentWeapon[0] || runtimeData.desiredWeapon[0] == runtimeData.currentWeapon[0] && runtimeData.desiredWeapon[1] != runtimeData.currentWeapon[1])
                            {
                                //If not, start to switchz
                                runtimeData.switchInProgress = true;
                                //Set time (Because here we cannot use a coroutine)
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                //Set phase
                                runtimeData.switchPhase = 0;
                                //Start putaway
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }
                        }
                        else
                        {
                            //Switching, courtine less
                            #region Switching
                            //Check for time
                            if (Time.time >= runtimeData.switchNextEnd)
                            {
                                //Time is over, check which phase is next
                                if (runtimeData.switchPhase == 0)
                                {
                                    //Weapon has been put away, hide weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHideOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Set weapon
                                    if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0])
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];
                                    }
                                    else
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = 0;
                                    }
                                    //Draw that weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Set phase
                                    runtimeData.switchPhase = 1;
                                    //Set time
                                    runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                    //Done, now wait
                                }
                                else if (runtimeData.switchPhase == 1)
                                {
                                    //Switching is over
                                    runtimeData.switchPhase = 0;
                                    runtimeData.switchNextEnd = 0f;
                                    runtimeData.switchInProgress = false;
                                }
                            }
                            #endregion
                        }
                    }
                }
                else //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Quick use has priority!
                    if (runtimeData.quickUseInProgress)
                    {
                        if (Time.time >= runtimeData.quickUseOverAt)
                        {
                            //First, put away current weapon!
                            if (runtimeData.quickUseState == 0)
                            {
                                //Set time (Because here we cannot use a coroutine)
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                //Set phase
                                runtimeData.quickUseState = 1;
                                //Start putaway
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                if (!pb.isBot)
                                {
                                    //Hide crosshair
                                    pb.main.hud.DisplayCrosshair(0f);
                                }
                            }
                            else if (runtimeData.quickUseState == 1)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                //Set state
                                runtimeData.quickUseState = 2;

                                //Begin quick use....
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.BeginQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else if (runtimeData.quickUseState == 2)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.WaitForQuickUseButtonRelease() || !pb.input.weaponSlotUses[slotConfiguration[runtimeData.desiredQuickUse[0]].quickUseInputID])
                                {
                                    //Set State
                                    runtimeData.quickUseState = 3;
                                    //End quick use...
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                }
                            }
                            else if (runtimeData.quickUseState == 3)
                            {
                                //Hide Quick Use!
                                runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUseAfter(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                //Check if currently selected  weapon is valid.
                                if (runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData))
                                {
                                    //Set weapon
                                    if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0])
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];
                                    }
                                    else
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = 0;
                                    }
                                }
                                else
                                {
                                    //Its not, find a new one
                                    int[] next = new int[2] { -1, -1 };
                                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                                    {
                                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                        {
                                            //Check if this one works!
                                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                                            {
                                                int id = i;
                                                int idTwo = o;
                                                next[0] = id;
                                                next[1] = idTwo;
                                                //We found one
                                                break;
                                            }
                                        }
                                    }

                                    //This should ALWAYS be true!
                                    if (next[0] >= 0 && next[1] >= 0)
                                    {
                                        runtimeData.desiredWeapon[0] = next[0];
                                        runtimeData.desiredWeapon[1] = next[1];
                                        //Set current one too!
                                        runtimeData.currentWeapon[0] = next[0];
                                        runtimeData.currentWeapon[1] = next[1];
                                    }
                                    else
                                    {
                                        Debug.LogError("Could not find next weapon! This is not allowed!");
                                    }
                                }
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                //Set phase
                                runtimeData.quickUseState = 4;
                                //Set time
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.quickUseState == 4)
                            {
                                //End quick use
                                runtimeData.quickUseInProgress = false;
                                runtimeData.desiredQuickUse[0] = -1;
                                runtimeData.desiredQuickUse[1] = -1;

                                //Also reset switching just to be sure!
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                    }
                    else
                    {
                        if (!runtimeData.switchInProgress)
                        {
                            //If we aren't switching weapons, update weapon behaviour
                            runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CalculateWeaponUpdateOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                            //Check if we want to select a different weapon
                            if (runtimeData.desiredWeapon[0] != runtimeData.currentWeapon[0] || runtimeData.desiredWeapon[0] == runtimeData.currentWeapon[0] && runtimeData.desiredWeapon[1] != runtimeData.currentWeapon[1])
                            {
                                //If not, start to switchz
                                runtimeData.switchInProgress = true;
                                //Set time (Because here we cannot use a coroutine)
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                //Set phase
                                runtimeData.switchPhase = 0;
                                //Start putaway
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }
                        }
                        else
                        {
                            //Switching, courtine less
                            #region Switching
                            //Check for time
                            if (Time.time >= runtimeData.switchNextEnd)
                            {
                                //Time is over, check which phase is next
                                if (runtimeData.switchPhase == 0)
                                {
                                    //Weapon has been put away, hide weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHideOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                    runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                    runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[0];
                                    //Draw that weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Set phase
                                    runtimeData.switchPhase = 1;
                                    //Set time
                                    runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                    //Done, now wait
                                }
                                else if (runtimeData.switchPhase == 1)
                                {
                                    //Switching is over
                                    runtimeData.switchPhase = 0;
                                    runtimeData.switchNextEnd = 0f;
                                    runtimeData.switchInProgress = false;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }

            public override void PlayerDead(Kit_PlayerBehaviour pb)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (uponDeathDrop == DeadDrop.Selected)
                        {
                            DropWeaponDead(pb, runtimeData.currentWeapon[0], runtimeData.currentWeapon[1]);
                        }
                        else if (uponDeathDrop == DeadDrop.All)
                        {
                            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                            {
                                if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                {
                                    for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                    {
                                        DropWeaponDead(pb, i, o);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override void OnAnimatorIKCallback(Kit_PlayerBehaviour pb, Animator anim)
            {
                //Get runtime data
                if (pb.isController)
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                    else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                    {
                        WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                }
                else
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                    else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                    {
                        WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, bool wasFallDamageApplied)
            {
                if (pb.isBot) return;
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.FallDownEffect(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, wasFallDamageApplied);
                }
            }

            public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnControllerColliderHitCallback(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, hit);
                }
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
            {
                if (stream.IsWriting)
                {
                    //Get runtime data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Send runtime data
                        stream.SendNext(runtimeData.desiredWeapon[0]);
                        stream.SendNext(runtimeData.desiredWeapon[1]);

                        stream.SendNext(runtimeData.desiredQuickUse[0]);
                        stream.SendNext(runtimeData.desiredQuickUse[1]);
                        stream.SendNext(runtimeData.quickUseInProgress);
                        stream.SendNext(runtimeData.quickUseSyncButtonWaitOver);

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                    //Send dummy data
                    else
                    {
                        stream.SendNext(0);
                        stream.SendNext(0);

                        stream.SendNext(0);
                        stream.SendNext(0);
                        stream.SendNext(false);
                        stream.SendNext(false);
                    }
                }
                else
                {
                    //Read data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                    {
                        WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                        runtimeData.desiredWeapon[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredWeapon[1] = (int)stream.ReceiveNext();

                        runtimeData.desiredQuickUse[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredQuickUse[1] = (int)stream.ReceiveNext();
                        runtimeData.quickUseInProgress = (bool)stream.ReceiveNext();
                        runtimeData.quickUseSyncButtonWaitOver = (bool)stream.ReceiveNext();

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                    else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        runtimeData.desiredWeapon[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredWeapon[1] = (int)stream.ReceiveNext();

                        runtimeData.desiredQuickUse[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredQuickUse[1] = (int)stream.ReceiveNext();
                        runtimeData.quickUseInProgress = (bool)stream.ReceiveNext();
                        runtimeData.quickUseSyncButtonWaitOver = (bool)stream.ReceiveNext();

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                    else
                    {
                        //Dummy reading
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                    }
                }
            }

            public override void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkSemiRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkSemiRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, int state)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBoltActionRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBoltActionRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state);
                }
            }

            public override void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, int burstLength)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBurstRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, burstLength);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBurstRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, burstLength);
                }
            }

            public override void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkReloadRPCReceived(pb, isEmpty, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkReloadRPCReceived(pb, isEmpty, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkProceduralReloadRPCReceived(pb, stage, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkProceduralReloadRPCReceived(pb, stage, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkMeleeChargeRPCReceived(Kit_PlayerBehaviour pb, int state, int slot)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeChargeRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeChargeRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
            }

            public override void NetworkMeleeStabRPCReceived(Kit_PlayerBehaviour pb, int state, int slot)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeStabRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeStabRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
            }

            public override void NetworkGrenadePullPinRPCReceived(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadePullPinRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadePullPinRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkGrenadeThrowRPCReceived(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadeThrowRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadeThrowRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override bool IsAiming(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.IsWeaponAiming(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return false;
            }

            public override float AimInTime(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.AimInTime(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 0.5f;
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.ForceIntoFirstPerson(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return false;
            }

            public override bool CanRun(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (allowSwitchingWhileRunning) return true;
                    else return !runtimeData.switchInProgress;
                }
                return true;
            }

            public override float CurrentMovementMultiplier(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.SpeedMultiplier(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override float CurrentSensitivity(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.Sensitivity(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override void NetworkReplaceWeapon(Kit_PlayerBehaviour pb, int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
            {
                if (pb.photonView.IsMine)
                {
                    //Get runtime data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get old data
                            WeaponControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as WeaponControllerRuntimeData;
                            //Clean Up
                            for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                            {
                                Destroy(oldWcrd.instantiatedObjects[i]);
                            }
                        }
                        else if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                        {
                            //Get old data
                            MeleeControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as MeleeControllerRuntimeData;
                            //Clean Up
                            for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                            {
                                Destroy(oldWcrd.instantiatedObjects[i]);
                            }
                        }
                        else if (runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Get old data
                            GrenadeControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData as GrenadeControllerRuntimeData;
                            //Clean Up
                            for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                            {
                                Destroy(oldWcrd.instantiatedObjects[i]);
                            }
                        }
                        if (!pb.isBot)
                        {
                            //Hide crosshair
                            pb.main.hud.DisplayCrosshair(0f);
                        }
                        //Get their behaviour modules
                        Kit_WeaponBase newWeaponBehaviour = pb.gameInformation.allWeapons[weapon];
                        //Setup new
                        newWeaponBehaviour.SetupValues(weapon); //This sets up values in the object itself, nothing else
                        object newRuntimeData = newWeaponBehaviour.SetupFirstPerson(pb, attachments); //This creates the first person objects
                        if (newRuntimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Set data
                            WeaponControllerRuntimeData wcrd = newRuntimeData as WeaponControllerRuntimeData;
                            //Set data
                            wcrd.bulletsLeft = bulletsLeft;
                            wcrd.bulletsLeftToReload = bulletsLeftToReload;
                        }
                        else if (newRuntimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Set data
                            GrenadeControllerRuntimeData gcrd = newRuntimeData as GrenadeControllerRuntimeData;
                            gcrd.amountOfGrenadesLeft = bulletsLeft;
                        }
                        runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]] = new WeaponReference();
                        runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].behaviour = newWeaponBehaviour;
                        runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].runtimeData = newRuntimeData;
                        runtimeData.weaponsInUse[slot[0]].weaponsInSlot[slot[1]].attachments = attachments;
                        if (newWeaponBehaviour is Kit_ModernWeaponScript)
                        {
                            //Setup third person
                            newWeaponBehaviour.SetupThirdPerson(pb, newWeaponBehaviour as Kit_ModernWeaponScript, newRuntimeData, attachments);
                        }
                        else
                        {
                            //Setup third person
                            newWeaponBehaviour.SetupThirdPerson(pb, null, newRuntimeData, attachments);
                        }
                        if (runtimeData.currentWeapon[0] == slot[0] && runtimeData.currentWeapon[1] == slot[1])
                        {
                            //Select current weapon
                            newWeaponBehaviour.DrawWeapon(pb, newRuntimeData);
                            //Set current weapon
                            runtimeData.currentWeapon[0] = slot[0];
                            runtimeData.currentWeapon[1] = slot[1];
                            //Set time
                            runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                            //Set phase
                            runtimeData.switchPhase = 1;
                            //Set switching
                            runtimeData.switchInProgress = true;
                        }
                    }
                }
                else
                {
                    Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.NetworkReplaceWeaponWait(pb, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments));
                }
            }

            public override void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkPhysicalBulletFired(pb, pos, dir, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkPhysicalBulletFired(pb, pos, dir, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot, int weaponInSlot)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            //MeleeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as MeleeControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left (nothing)
                            instData[1] = 0;
                            //Bullets Left To Reload (nothing;
                            instData[2] = 0;
                            //Attachments length (melee doesnt support that yet but well do it anyway)
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                            if (wepData.amountOfGrenadesLeft <= 0) return;
                            //Get the Scriptable object
                            Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left (grenades left)
                            instData[1] = wepData.amountOfGrenadesLeft;
                            //Bullets Left To Reload (nothing;
                            instData[2] = 0;
                            //Attachments length (melee doesnt support that yet but well do it anyway)
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                    }
                }
            }

            /// <summary>
            /// Drops a weapon and applies the ragdoll force
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="slot"></param>
            public void DropWeaponDead(Kit_PlayerBehaviour pb, int slot, int weaponInSlot)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour.dropPrefab)
                        {
                            if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                            {
                                //Get the weapon's runtime data
                                WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                                //Get the Scriptable object
                                Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = wepData.bulletsLeft;
                                //Bullets Left To Reload
                                instData[2] = wepData.bulletsLeftToReload;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                            else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                            {
                                Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = 0;
                                //Bullets Left To Reload
                                instData[2] = 0;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                            else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                            {
                                //Get the weapon's runtime data
                                GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                                if (wepData.amountOfGrenadesLeft <= 0) return;
                                //Get the Scriptable object
                                Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = wepData.amountOfGrenadesLeft;
                                //Bullets Left To Reload
                                instData[2] = 0;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                        }
                    }
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot, int weaponInSlot, Transform replace)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour.dropPrefab)
                    {
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                        {
                            Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = 0;
                            //Bullets Left To Reload
                            instData[2] = 0;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                            if (wepData.amountOfGrenadesLeft <= 0) return;
                            //Get the Scriptable object
                            Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.amountOfGrenadesLeft;
                            //Bullets Left To Reload
                            instData[2] = 0;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                    }
                }
            }

            public override int WeaponState(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.WeaponState(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                return 0;
            }

            public override int WeaponType(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetWeaponType(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                return 0;
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Forward to currently selected weapon
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                    {
                        runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.FirstThirdPersonChanged(pb, perspective, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                    }
                    else
                    {
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.FirstThirdPersonChanged(pb, perspective, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                }
            }


            public override void PluginSelectWeapon(Kit_PlayerBehaviour pb, int slot, int id, bool locked = true)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Just set, thats all!
                    runtimeData.desiredWeapon[1] = id;
                    runtimeData.desiredWeapon[0] = slot;
                    runtimeData.isDesiredWeaponLocked = locked;
                }
            }

            public override int[] GetCurrentlyDesiredWeapon(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    int[] toReturn = new int[2];
                    toReturn[0] = runtimeData.desiredWeapon[0];
                    toReturn[1] = runtimeData.desiredWeapon[1];
                    return toReturn;
                }
                return null;
            }

            public override int[] GetCurrentlySelectedWeapon(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    int[] toReturn = new int[2];
                    toReturn[0] = runtimeData.currentWeapon[0];
                    toReturn[1] = runtimeData.currentWeapon[1];
                    return toReturn;
                }
                return null;
            }
        }
    }
}
