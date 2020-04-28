using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_ModernAutoSpawnSystem : Kit_AutoSpawnSystemBase
    {
    
        [Header("Settings")]
        public float autoRespawnTime = 6f;
     
        public Kit_IngameMain main;

        [Header("UI")]
        //UI Root
        public GameObject spawnSystemRoot;
   
        public Text remainingTimeText;

        #region Runtime
     
        private bool isAutoSpawnSystemOpen;
     
        private float autoSpawnSystemActivatedAt;
        #endregion

        void Start()
        {
            //Disable
            //Close system
            isAutoSpawnSystemOpen = false;
            //Disable GUI
            spawnSystemRoot.SetActive(false);
        }

        void Update()
        {
            //Update ui if open
            if (isAutoSpawnSystemOpen)
            {
                //Update text
                remainingTimeText.text = "Auto respawn in " + ((autoSpawnSystemActivatedAt + autoRespawnTime) - Time.time).ToString("F2") +" seconds";
                //Check
                if (Time.time > (autoSpawnSystemActivatedAt + autoRespawnTime))
                {
                    //Spawn and close
                    main.Spawn();
                    //Close system
                    isAutoSpawnSystemOpen = false;
                    //Disable GUI
                    spawnSystemRoot.SetActive(false);
                }

                //Check for input
                if (Input.GetKeyDown(KeyCode.F))
                {
                    //Spawn and close
                    main.Spawn();
                    //Close system
                    isAutoSpawnSystemOpen = false;
                    //Disable GUI
                    spawnSystemRoot.SetActive(false);
                }
            }
        }

        public override void Interruption()
        {
            //Close system
            isAutoSpawnSystemOpen = false;
            //Disable GUI
            spawnSystemRoot.SetActive(false);
        }

        public override void LocalPlayerDied()
        {
            //Set time
            autoSpawnSystemActivatedAt = Time.time;
            //Activate system
            isAutoSpawnSystemOpen = true;
            //Activate GUI
            spawnSystemRoot.SetActive(true);
            //Close Pause Menu
            main.SetPauseMenuState(false, false);
        }

        public override void LocalPlayerSpawned()
        {
            //Close system
            isAutoSpawnSystemOpen = false;
            //Disable GUI
            spawnSystemRoot.SetActive(false);
        }
    }
}
