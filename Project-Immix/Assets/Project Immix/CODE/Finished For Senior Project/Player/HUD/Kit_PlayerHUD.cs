using System;
using System.Collections.Generic;
using ImmixKit.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_PlayerHUD : Kit_PlayerHUDBase
    {
       
        public GameObject root;

      
        public Kit_IngameMain main;

              public Canvas canvas;

      
        [Header("Health")]
        public GameObject healthRoot;
    
        public Text healthText;

    
        [Header("Ammo")]
        public GameObject bulletsRoot;
     
        public Text bulletsLeft;
      
        public Text bulletsLeftToReload; 

        [Header("Crosshair")]
      
        public GameObject crosshairRoot;
   
        public Image crosshairLeft;
      
        public Image crosshairRight;
   
        public Image crosshairUp;
    
        public Image crosshairDown;
     
        public RectTransform crosshairMoveRoot;

        [Header("Bloody Screen")]
     
        public Image bloodyScreen;

        [Header("Hitmarker")]
        public Image hitmarkerImage;
    
        public float hitmarkerTime;
     
        public AudioClip hitmarkerSound;
      
        public AudioSource hitmarkerAudioSource;
   
        private float hitmarkerLastDisplay;
       
        private Color hitmarkerColor;

        [Header("Hitmarker Spawn Protected")]
        public Image hitmarkerSpawnProtectionImage;
        /// <summary>
        /// How long is a hitmarker going to be displayed?
        /// </summary>
        public float hitmarkerSpawnProtectionTime;
        /// <summary>
        /// Sound that is going to be played when we hit someone
        /// </summary>
        public AudioClip hitmarkerSpawnProtectionSound;
        /// <summary>
        /// Audio source for <see cref="hitmarkerSound"/>
        /// </summary>
        public AudioSource hitmarkerSpawnProtectionAudioSource;
        /// <summary>
        /// At which <see cref="Time.time"/> is the hitmarker going to be completely invisible
        /// </summary>
        private float hitmarkerSpawnProtectionLastDisplay;
   
        private Color hitmarkerSpawnProtectionColor;

        [Header("Damage Indicator")]
        /// <summary>
        /// The transform which is going to be rotated on the UI
        /// </summary>
        public RectTransform indicatorRotate;
   
        public Image indicatorImage;
        /// <summary>
        /// An object which the player's position is going to be copied to. Parent of the helper.
        /// </summary>
        public Transform indicatorHelperRoot;
        /// <summary>
        /// A helper transform which looks at the last direction we were shot from
        /// </summary>
        public Transform indicatorHelper;
    
        public float indicatorVisibleTime = 5f;
      
        private float indicatorAlpha;
     
        private Vector3 indicatorLastPos;

     
        [Header("Waiting for Players")]
     
        public GameObject waitingForPlayersRoot;

        [Header("Player Name Markers")]
        public List<Kit_PlayerMarker> allPlayerMarkers = new List<Kit_PlayerMarker>();
    
        public GameObject playerMarkerPrefab;
     
        public RectTransform playerMarkerGo;
      
        public Color friendlyMarkerColor = Color.white;
     
        public Color enemyMarkerColor = Color.red;

        [Header("Spawn Protection")]
      
        public GameObject spRoot;
  
        public Text spText;

        [Header("Weapon Pickup")]
     
        public GameObject weaponPickupRoot;
     
        public Text weaponPickupText;

        [Header("Interaction")]
     
        public GameObject interactionRoot;
    
        public Text interactionText;

     
        [Header("Stamina Bar")]
        public CanvasGroup staminaGroup;
        /// <summary>
        /// Bar to fill with stamina
        /// </summary>
        public Image staminaProgress;
        /// <summary>
        /// How fast will stamina fade in / out
        /// </summary>
        public float staminaAlphaFadeSpeed = 2f;

    
        [Header("Flashbang Blind")]
        public Image flashbangWhite;
    
        public RawImage flashbangScreenshot;
      
        private float flashbangTimeLeft;
      
        public AudioSource flashbangSource;

      
        [Header("Weapon Display")]
        public GameObject weaponDisplayPrefab;
        /// <summary>
        /// Where they go
        /// </summary>
        public RectTransform weaponDisplayGo;
       
        public List<Image> weaponDisplayActives = new List<Image>();
        
        public Color weaponDisplaySelectedColor = Color.black;
        
        public Color weaponDisplayUnselectedColor = Color.white;

        /// <summary>
        /// Prefab for weapon display
        /// </summary>
        [Header("Weapon Quick Use Display")]
        public GameObject weaponQuickUseDisplayPrefab;
      
        public RectTransform weaponQuickUseDisplayGo;
    
        public List<Image> weaponQuickUseDisplayActives = new List<Image>();


        [Header("Leaving Battlefield")]
        public Text leavingBattlefieldText;

        #region Unity Calls
        void Awake()
        {
            //Cache color
            hitmarkerColor = hitmarkerImage.color;
            //SpawnProtection
            hitmarkerSpawnProtectionColor = hitmarkerSpawnProtectionImage.color;
        }

        void Update()
        {
            //Update hitmarker alpha
            hitmarkerColor.a = Mathf.Clamp01(hitmarkerLastDisplay - Time.time);
            //Set the color
            hitmarkerImage.color = hitmarkerColor;

            //Update hitmarker SP alpha
            hitmarkerSpawnProtectionColor.a = Mathf.Clamp01(hitmarkerSpawnProtectionLastDisplay - Time.time);
            //Set the color
            hitmarkerSpawnProtectionImage.color = hitmarkerSpawnProtectionColor;

            //Check if stamina shall be displayed
            if (!Mathf.Approximately(staminaProgress.fillAmount, 1f))
            {
                if (staminaGroup.alpha < 1f)
                {
                    //Increase alpha
                    staminaGroup.alpha += Time.deltaTime * staminaAlphaFadeSpeed;
                }
            }
            else
            {
                if (staminaGroup.alpha > 0f)
                {
                    //Decrase alpha
                    staminaGroup.alpha -= Time.deltaTime * staminaAlphaFadeSpeed;
                }
            }
        }
        #endregion

        #region Custom Calls
        /// <summary>
        /// Shows or hides the HUD. Some parts (such as the hitmarker) will always be visible.
        /// </summary>
        /// <param name="visible"></param>
        public override void SetVisibility(bool visible)
        {
            //Update the active state of root, but only if it doesn't have it already.
            if (root)
            {
                if (visible)
                {
                    if (!root.activeSelf) root.SetActive(true);
                }
                else
                {
                    if (root.activeSelf) root.SetActive(false);
                    //Hide spawn protection too
                    if (spRoot.activeSelf) spRoot.SetActive(false);
                    //Hide Battlefield
                    DisplayLeavingBattlefield(-1);
                }
            }
        }

        public override void DisplayLeavingBattlefield(float timeLeft)
        {
            if (timeLeft < 0)
            {
                leavingBattlefieldText.enabled = false;
            }
            else
            {
                leavingBattlefieldText.text = "YOU ARE LEAVING THE BATTLEFIELD. YOU WILL DIE IN " + timeLeft.ToString("F1");
                leavingBattlefieldText.enabled = true;
            }
        }

 
        public override void SetWaitingStatus(bool isWaiting)
        {
            if (waitingForPlayersRoot.activeSelf != isWaiting)
            {
                //Set to the required state
                waitingForPlayersRoot.SetActive(isWaiting);
            }
        }

        public override void PlayerStart(Kit_PlayerBehaviour pb)
        {
            indicatorAlpha = 0f;
            //Update state
            //Set state accordingly
            staminaGroup.alpha = 0f;
            flashbangTimeLeft = 0f;
            flashbangScreenshot.color = new Color(1, 1, 1, 0f);
            flashbangWhite.color = new Color(1, 1, 1, 0f);
            //Start sound
            flashbangSource.volume = 0f;
            flashbangSource.loop = true;
            flashbangSource.Play();
        }

        public override void PlayerEnd(Kit_PlayerBehaviour pb)
        {
            if (flashbangSource)
            {
                //Set sound to 0
                flashbangSource.volume = 0f;
                flashbangSource.Stop();
            }
        }

        public override void PlayerUpdate(Kit_PlayerBehaviour pb)
        {
            //Position damage indicator
            indicatorHelperRoot.position = pb.transform.position;
            indicatorHelperRoot.rotation = pb.transform.rotation;
            //Look at
            indicatorHelper.LookAt(indicatorLastPos);
            //Decrease alpha
            if (indicatorAlpha > 0f) indicatorAlpha -= Time.deltaTime;
            //Set alpha
            indicatorImage.color = new Color(1f, 1f, 1f, indicatorAlpha);
            //Set rotation 
            indicatorRotate.localRotation = Quaternion.Euler(0f, 0f, -indicatorHelper.localEulerAngles.y);

            if (flashbangTimeLeft >= 0)
            {
                //Set Color
                flashbangScreenshot.color = new Color(1, 1, 1, flashbangTimeLeft / 2f);
                flashbangWhite.color = new Color(1, 1, 1, Mathf.Clamp(flashbangTimeLeft / 3f, 0, 0.6f));
                flashbangSource.volume = flashbangTimeLeft;

                flashbangTimeLeft -= Time.deltaTime;
            }
            else
            {
                flashbangScreenshot.color = new Color(1, 1, 1, 0f);
                flashbangWhite.color = new Color(1, 1, 1, 0f);
                flashbangSource.volume = 0f;
            }
        }

        /// <summary>
        /// Displays the hitmarker for <see cref="hitmarkerTime"/> seconds
        /// </summary>
        public override void DisplayHitmarker()
        {
            hitmarkerLastDisplay = Time.time + hitmarkerTime; //Set time of the hitmarker, which makes it visible automatically
            //Play sound
            if (hitmarkerSound)
            {
                hitmarkerAudioSource.clip = hitmarkerSound;
                hitmarkerAudioSource.PlayOneShot(hitmarkerSound);
            }
        }

        public override void DisplayHitmarkerSpawnProtected()
        {
            hitmarkerSpawnProtectionLastDisplay = Time.time + hitmarkerSpawnProtectionTime; //Set time of the hitmarker, which makes it visible automatically
            //Play sound
            if (hitmarkerSpawnProtectionSound)
            {
                hitmarkerSpawnProtectionAudioSource.clip = hitmarkerSpawnProtectionSound;
                hitmarkerSpawnProtectionAudioSource.PlayOneShot(hitmarkerSpawnProtectionSound);
            }
        }

        /// <summary>
        /// Display hit points in the HUD
        /// </summary>
        /// <param name="hp">Amount of hitpoints</param>
        public override void DisplayHealth(float hp)
        {
            if (hp >= 0f)
            {
                if (!healthRoot.activeSelf) healthRoot.SetActive(true);
                //Display the HP
                healthText.text = hp.ToString("F0"); //If you want decimals, change it to F1, F2, etc...
            }
            else
            {
                if (healthRoot.activeSelf) healthRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Display ammo count in the HUD
        /// </summary>
        /// <param name="bl">Bullets left (On the left side)</param>
        /// <param name="bltr">Bullets left to reload (On the right side)</param>
        public override void DisplayAmmo(int bl, int bltr, bool show = true)
        {
            if (show)
            {
                if (bl >= 0)
                {
                    //Set text for bullets left
                    bulletsLeft.text = bl.ToString("F0");
                }
                else
                {
                    bulletsLeft.text = "";
                }
                if (bltr >= 0)
                {
                    //Set text for bullets left to reload
                    bulletsLeftToReload.text = bltr.ToString("F0");
                }
                else
                {
                    bulletsLeftToReload.text = "";
                }

                if (!bulletsRoot.activeSelf) bulletsRoot.SetActive(true);
            }
            else
            {
                if (bulletsRoot.activeSelf) bulletsRoot.SetActive(false);
            }
        }

        public override void DisplayCrosshair(float size)
        {
            //For zero or smaller,
            if (size <= 0f && !Kit_GameSettings.isThirdPersonActive)
            {
                //Hide it
                crosshairLeft.enabled = false;
                crosshairRight.enabled = false;
                crosshairUp.enabled = false;
                crosshairDown.enabled = false;
            }
            else
            {
                //Show it
                crosshairLeft.enabled = true;
                crosshairRight.enabled = true;
                crosshairUp.enabled = true;
                crosshairDown.enabled = true;

                //Position all crosshair parts accordingly
                crosshairLeft.rectTransform.anchoredPosition = new Vector2 { x = size };
                crosshairRight.rectTransform.anchoredPosition = new Vector2 { x = -size };
                crosshairUp.rectTransform.anchoredPosition = new Vector2 { y = size };
                crosshairDown.rectTransform.anchoredPosition = new Vector2 { y = -size };
            }
        }

        public override void MoveCrosshairTo(Vector3 pos)
        {
            crosshairMoveRoot.anchoredPosition3D = pos;
        }

        public override void DisplayWeaponsAndQuickUses(Kit_PlayerBehaviour pb, WeaponManagerControllerRuntimeData runtimeData)
        {
            List<WeaponDisplayData> weaponDisplayData = new List<WeaponDisplayData>();
            List<WeaponQuickUseDisplayData> weaponQuickUseDisplayData = new List<WeaponQuickUseDisplayData>();

            //Get Data from Weapon Manager!
            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
            {
                for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                {
                    //Get from weapons!
                    WeaponDisplayData wdd = runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.GetWeaponDisplayData(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData);
                    WeaponQuickUseDisplayData wqudd = runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.GetWeaponQuickUseDisplayData(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData);

                    //Add if weapon supports it!
                    if (wdd != null)
                    {
                        //Check if this weapon is selected atm!
                        if (runtimeData.currentWeapon[0] == i && runtimeData.currentWeapon[1] == o)
                        {
                            wdd.selected = true;
                        }
                        else
                        {
                            wdd.selected = false;
                        }
                        weaponDisplayData.Add(wdd);
                    }

                    //Add if weapon supports it!
                    if (wqudd != null)
                    {
                        weaponQuickUseDisplayData.Add(wqudd);
                    }
                }
            }

            //Make sure list length if correct!
            if (weaponDisplayData.Count != weaponDisplayActives.Count)
            {
                while (weaponDisplayData.Count != weaponDisplayActives.Count)
                {
                    if (weaponDisplayActives.Count > weaponDisplayData.Count)
                    {
                        Destroy(weaponDisplayActives[weaponDisplayActives.Count - 1].gameObject);
                        //Remove
                        weaponDisplayActives.RemoveAt(weaponDisplayActives.Count - 1);
                    }
                    else if (weaponDisplayActives.Count < weaponDisplayData.Count)
                    {
                        //Add new
                        GameObject go = Instantiate(weaponDisplayPrefab, weaponDisplayGo, false);
                        //Get
                        Image img = go.GetComponent<Image>();
                        //Add
                        weaponDisplayActives.Add(img);
                    }
                }
            }

            //Now length is correct, redraw!
            for (int i = 0; i < weaponDisplayData.Count; i++)
            {
                weaponDisplayActives[i].sprite = weaponDisplayData[i].sprite;
                //Set correct color
                if (weaponDisplayData[i].selected)
                {
                    weaponDisplayActives[i].color = weaponDisplaySelectedColor;
                }
                else
                {
                    weaponDisplayActives[i].color = weaponDisplayUnselectedColor;
                }
            }

            int totalQuickUseDisplayLength = 0;

            for (int i = 0; i < weaponQuickUseDisplayData.Count; i++)
            {
                totalQuickUseDisplayLength += weaponQuickUseDisplayData[i].amount;
            }

            //Make sure list length if correct!
            if (totalQuickUseDisplayLength != weaponQuickUseDisplayActives.Count)
            {
                while (totalQuickUseDisplayLength != weaponQuickUseDisplayActives.Count)
                {
                    if (weaponQuickUseDisplayActives.Count > totalQuickUseDisplayLength)
                    {
                        Destroy(weaponQuickUseDisplayActives[weaponQuickUseDisplayActives.Count - 1].gameObject);
                        //Remove
                        weaponQuickUseDisplayActives.RemoveAt(weaponQuickUseDisplayActives.Count - 1);
                    }
                    else if (weaponQuickUseDisplayActives.Count < totalQuickUseDisplayLength)
                    {
                        //Add new
                        GameObject go = Instantiate(weaponQuickUseDisplayPrefab, weaponQuickUseDisplayGo, false);
                        //Get
                        Image img = go.GetComponent<Image>();
                        //Add
                        weaponQuickUseDisplayActives.Add(img);
                    }
                }
            }

            int currentIndex = 0;

            //Now length is correct, redraw!
            for (int i = 0; i < weaponQuickUseDisplayData.Count; i++)
            {
                for (int o = 0; o < weaponQuickUseDisplayData[i].amount; o++)
                {
                    weaponQuickUseDisplayActives[currentIndex].sprite = weaponQuickUseDisplayData[i].sprite;
                    currentIndex++;
                }
            }
        }

        public override void DisplayHurtState(float state)
        {
            //Update bloody screen
            bloodyScreen.color = new Color(1, 1, 1, state);
        }

        public override void DisplayShot(Vector3 from)
        {
            //Set pos
            indicatorLastPos = from;
            //Set alpha
            indicatorAlpha = indicatorVisibleTime;
        }

        /// <summary>
        /// Should we grab the screen for flashbang?
        /// </summary>
        bool grab = false;

        float flashbangTimeForGrab;

        public void OnEnable()
        {
            // register the callback when enabling object
            Camera.onPostRender += FlashbangPostRender;
        }

        public void OnDisable()
        {
            // remove the callback when disabling object
            Camera.onPostRender -= FlashbangPostRender;
        }

        private void FlashbangPostRender(Camera cam)
        {
            //Check if its the main camera
            if (cam.CompareTag("MainCamera"))
            {
                if (grab)
                {
                    Texture2D tex = new Texture2D(Screen.width, Screen.height);
                    tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    tex.Apply();
                    flashbangScreenshot.texture = tex;
                    //Reset the grab state
                    grab = false;
                    //Set time, this needs to be here otherwise the screenshot will be white too!
                    flashbangTimeLeft = flashbangTimeForGrab;
                }
            }
        }

        public override void DisplayBlind(float time)
        {
            //Set time
            flashbangTimeForGrab = time;
            grab = true;

            //Play if not
            flashbangSource.loop = true;
            flashbangSource.Play();
        }


        public override void DisplayWeaponPickup(bool displayed, int weapon = -1)
        {
            if (displayed)
            {
                if (!weaponPickupRoot.activeSelf)
                    weaponPickupRoot.SetActive(true);
                if (weapon >= 0)
                {
                    //Set name
                    weaponPickupText.text = "Press [F] to pickup: " + main.gameInformation.allWeapons[weapon].weaponName;
                }
            }
            else
            {
                if (weaponPickupRoot.activeSelf)
                    weaponPickupRoot.SetActive(false);
            }
        }

        public override void DisplayInteraction(bool display, string txt = "")
        {
            if (display)
            {
                if (!interactionRoot.activeSelf) interactionRoot.SetActive(true);
                //Set
                interactionText.text = "Press [F] to: " + txt;
            }
            else
            {
                if (interactionRoot.activeSelf) interactionRoot.SetActive(false);
            }
        }

        public override void DisplayStamina(float stamina)
        {
            //Set progress
            staminaProgress.fillAmount = (stamina / 100f);
        }

        public override int GetUnusedPlayerMarker()
        {
            for (int i = 0; i < allPlayerMarkers.Count; i++)
            {
                //Check if its not used
                if (!allPlayerMarkers[i].used)
                {
                    //If its not, set it to used
                    allPlayerMarkers[i].used = true;
                    //Activate its root
                    allPlayerMarkers[i].markerRoot.gameObject.SetActive(true);
                    //And return its id
                    return i;
                }
            }
            //If not, add a new one and return that one
            GameObject newMarker = Instantiate(playerMarkerPrefab, playerMarkerGo, false);
            //Reset scale
            newMarker.transform.localScale = Vector3.one;
            //Add
            allPlayerMarkers.Add(newMarker.GetComponent<Kit_PlayerMarker>());
            allPlayerMarkers[allPlayerMarkers.Count - 1].used = true;
            allPlayerMarkers[allPlayerMarkers.Count - 1].markerRoot.gameObject.SetActive(true);
            return allPlayerMarkers.Count - 1;
        }

        public override void ReleasePlayerMarker(int id)
        {
            if (allPlayerMarkers[id].markerRoot)
            {
                //Deactivate its root
                allPlayerMarkers[id].markerRoot.gameObject.SetActive(false);
            }
            //And set it to unused
            allPlayerMarkers[id].used = false;
        }

        public override void UpdatePlayerMarker(int id, PlayerNameState state, Vector3 worldPos, string playerName)
        {
            //Get screen pos
            Vector3 canvasPos = canvas.WorldToCanvas(worldPos, main.mainCamera);
            //Set
            allPlayerMarkers[id].markerRoot.anchoredPosition3D = canvasPos;
            //Check if it is visible at all
            if (canvasPos.z > 0)
            {
                //Check the state
                if (state == PlayerNameState.friendlyClose)
                {
                    //Set name
                    allPlayerMarkers[id].markerText.text = playerName;
                    //Set color
                    allPlayerMarkers[id].markerText.color = friendlyMarkerColor;
                    //Display name
                    allPlayerMarkers[id].markerText.enabled = true;
                    //Dont display marker
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
                else if (state == PlayerNameState.friendlyFar)
                {
                    //Display marker
                    allPlayerMarkers[id].markerArrow.enabled = true;
                    //Dont display name
                    allPlayerMarkers[id].markerText.enabled = false;
                }
                else if (state == PlayerNameState.enemy)
                {
                    //Set name
                    allPlayerMarkers[id].markerText.text = playerName;
                    //Set color
                    allPlayerMarkers[id].markerText.color = enemyMarkerColor;
                    //Display name
                    allPlayerMarkers[id].markerText.enabled = true;
                    //Dont display marker
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
                else
                {
                    //Hide all
                    allPlayerMarkers[id].markerText.enabled = false;
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
            }
            //If its not...
            else
            {
                //...hide all
                allPlayerMarkers[id].markerText.enabled = false;
                allPlayerMarkers[id].markerArrow.enabled = false;
            }
        }

        public override void UpdateSpawnProtection(bool isActive, float timeLeft)
        {
            if (isActive)
            {
                //Activate root
                if (!spRoot.activeSelf) spRoot.SetActive(true);
                //Set time
                spText.text = timeLeft.ToString("F1");
            }
            else
            {
                //Deactivate root
                if (spRoot.activeSelf) spRoot.SetActive(false);
            }
        }
        #endregion
    }
}
