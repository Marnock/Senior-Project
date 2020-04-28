using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_PlayerHUDBase : MonoBehaviour
    {
   
        public abstract void SetVisibility(bool visible);

     
        public abstract void DisplayLeavingBattlefield(float timeLeft);


        public abstract void SetWaitingStatus(bool isWaiting);


        public abstract void PlayerStart(Kit_PlayerBehaviour pb);


        public abstract void PlayerUpdate(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Called from the player when he dies
        /// </summary>
        /// <param name="pb"></param>
        public abstract void PlayerEnd(Kit_PlayerBehaviour pb);

   
        public abstract void DisplayHitmarker();

 
        public abstract void DisplayHitmarkerSpawnProtected();

  
        public abstract void DisplayHealth(float hp);


        public abstract void DisplayAmmo(int bl, int bltr, bool show = true);

        public abstract void DisplayCrosshair(float size);

 
        public abstract void MoveCrosshairTo(Vector3 pos);


        public abstract void DisplayWeaponsAndQuickUses(Kit_PlayerBehaviour pb, Weapons.WeaponManagerControllerRuntimeData weaponControllerRuntimeData);

        public abstract void DisplayHurtState(float state);

        /// <summary>
        /// Called when we were shot
        /// </summary>
        /// <param name="from"></param>
        public abstract void DisplayShot(Vector3 from);

   
        public abstract void DisplayBlind(float time);


        public abstract void DisplayWeaponPickup(bool displayed, int weapon = -1);


        public abstract void DisplayInteraction(bool display, string txt = "");


        public abstract void DisplayStamina(float stamina);

        public abstract int GetUnusedPlayerMarker();


        public abstract void ReleasePlayerMarker(int id);

        /// <summary>
        /// Positions a player marker at worldPos (on screen) with state and the given name
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="worldPos"></param>
        /// <param name="playerName"></param>
        public abstract void UpdatePlayerMarker(int id, PlayerNameState state, Vector3 worldPos, string playerName);


        public abstract void UpdateSpawnProtection(bool isActive, float timeLeft);
    }
}
