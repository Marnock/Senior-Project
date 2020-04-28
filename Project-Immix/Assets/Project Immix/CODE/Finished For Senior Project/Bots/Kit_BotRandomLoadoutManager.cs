using ImmixKit.Weapons;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ImmixKit
{
    /// <summary>
    /// Creates random loadouts for bots for when more weapons are available. Bots only use 1 gun now
    /// </summary>

    public class Kit_BotRandomLoadoutManager : Kit_BotLoadoutManager
    {
        public override Loadout GetBotLoadout(Kit_IngameMain main)
        {
            Loadout toReturn = new Loadout();
            //Find a primary
            Kit_WeaponBase[] primaries = Array.FindAll(main.gameInformation.allWeapons, x => x.weaponType == "Primary");
            Kit_WeaponBase primary = primaries[Random.Range(0, primaries.Length)];
            int primaryIndex = Array.IndexOf(main.gameInformation.allWeapons, primary);
            //Find a secondary
            Kit_WeaponBase[] secondaries = Array.FindAll(main.gameInformation.allWeapons, x => x.weaponType == "Secondary");
            Kit_WeaponBase secondary = secondaries[Random.Range(0, secondaries.Length)];
            int secondaryIndex = Array.IndexOf(main.gameInformation.allWeapons, secondary);

            toReturn.loadoutWeapons = new LoadoutWeapon[2];
            toReturn.loadoutWeapons[0] = new LoadoutWeapon();
            toReturn.loadoutWeapons[1] = new LoadoutWeapon();

            toReturn.loadoutWeapons[0].goesToSlot = 0;
            toReturn.loadoutWeapons[0].weaponID = primaryIndex;
            if (primary.firstPersonPrefab.GetComponent<Kit_WeaponRenderer>())
            {
                toReturn.loadoutWeapons[0].attachments = new int[primary.firstPersonPrefab.GetComponent<Kit_WeaponRenderer>().attachmentSlots.Length];
            }
            else
            {
                toReturn.loadoutWeapons[0].attachments = new int[0];
            }

            toReturn.loadoutWeapons[1].goesToSlot = 1;
            toReturn.loadoutWeapons[1].weaponID = secondaryIndex;
            if (secondary.firstPersonPrefab.GetComponent<Kit_WeaponRenderer>())
            {
                toReturn.loadoutWeapons[1].attachments = new int[secondary.firstPersonPrefab.GetComponent<Kit_WeaponRenderer>().attachmentSlots.Length];
            }
            else
            {
                toReturn.loadoutWeapons[1].attachments = new int[0];
            }
            toReturn.teamOnePlayerModelID = Random.Range(0, main.gameInformation.allTeamOnePlayerModels.Length);
            toReturn.teamOnePlayerModelCustomizations = new int[main.gameInformation.allTeamOnePlayerModels[toReturn.teamOnePlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];

            toReturn.teamTwoPlayerModelID = Random.Range(0, main.gameInformation.allTeamTwoPlayerModels.Length);
            toReturn.teamTwoPlayerModelCustomizations = new int[main.gameInformation.allTeamTwoPlayerModels[toReturn.teamTwoPlayerModelID].prefab.GetComponent<Kit_ThirdPersonPlayerModel>().customizationSlots.Length];

            return toReturn;
        }
    }
}
