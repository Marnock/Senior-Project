using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_SimpleChat : Kit_ChatBase
    {
        public Kit_IngameMain main;

        //Runtime info
        private int messageType = 0;
        private bool isChatOpen;
        private float lastMessageReceived = -100f;
        private float lastChatOpen = -100;
        private List<GameObject> activeChatEntries = new List<GameObject>();
        //End

   
        public CanvasGroup chatAlpha;

        public CanvasGroup chatScrollAlpha;

        public CanvasGroup messageAlpha;

    
        public Text chatPlaceholder;
        public InputField chatInput;

     
        public GameObject chatEntryPrefab;
 
        public RectTransform chatEntryGo;

   
        public ScrollRect chatScroll;

     
        public float fadeOutTimeNormal = 4f;
    
        public float fadeOutTimeMessages = 8f;

   
        public int maxNumberOfActiveChatEntries = 32;

    
        public Color serverMessageColor;

    
        public Color teamOneColor = Color.blue;

      
        public Color teamTwoColor = Color.red;

      
        public Color teamOnlyColor = Color.yellow;

        public override void DisplayChatMessage(Photon.Realtime.Player sender, string message, int type)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Determine Color
            Color finalCol = Color.white;
            //Color on team basis, if we are playing a team game mode
            if (main.currentGameModeBehaviour.isTeamGameMode)
            {
                //Set if the player has a team set
                if (sender.CustomProperties["team"] != null)
                {
                    //Team only message
                    if (type == 1)
                    {
                        finalCol = teamOnlyColor;
                    }
                    else
                    {
                        //Get team
                        int team = (int)sender.CustomProperties["team"];
                        if (team == 0)
                        {
                            //Set color
                            finalCol = teamOneColor;
                        }
                        else if (team == 1)
                        {
                            //Set color
                            finalCol = teamTwoColor;
                        }
                    }

                }
            }
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(finalCol) + ">" + sender.NickName + "</color>: " + message);
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void MasterClientSwitched(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " is the new master client");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void PlayerJoined(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " joined");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void PlayerLeft(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " left");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void BotJoined(string botName)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + botName + " joined");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void BotLeft(string botName)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + botName + " left");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }


        public void SendMessageButton()
        {
            //Check if we can send
            if (!chatInput.text.IsNullOrWhiteSpace())
            {
                //Send input
                SendChatMessage(chatInput.text, messageType);
                //Reset
                chatInput.text = "";
            }
            lastChatOpen = Time.time; //Set time
            lastMessageReceived = Time.time; //Also set this time
            //Close chat
            isChatOpen = false;
            RedrawChat(); //Redraw
        }

        public override void PauseMenuOpened()
        {
            //If chat is open
            if (isChatOpen)
            {
                //Set times
                lastChatOpen = Time.time;
                lastMessageReceived = Time.time;
            }

            //Make sure chat is closed
            isChatOpen = false;
            RedrawChat();
        }

        public override void PauseMenuClosed()
        {
            //If chat is open
            if (isChatOpen)
            {
                //Set times
                lastChatOpen = Time.time;
                lastMessageReceived = Time.time;
            }

            //Make sure chat is closed
            isChatOpen = false;
            RedrawChat();
        }

        void RedrawChat()
        {
            if (isChatOpen)
            {
                //If chat is open, select it
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject, null);
                chatInput.OnPointerClick(new PointerEventData(EventSystem.current));
                //And unlock cursor
                LockCursor.lockCursor = false;
            }
            else
            {
                if (main.myPlayer && !Kit_IngameMain.isPauseMenuOpen)
                {
                    //If we have a player and the pause menu is not open, lock the cursor again
                    LockCursor.lockCursor = true;
                }
                else
                {
                    LockCursor.lockCursor = false;
                }
            }
        }

        #region Unity Calls
        void Update()
        {
            #region Input
            //Only check for chat input if the pause menu isnt open
            if (!Kit_IngameMain.isPauseMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (!isChatOpen)
                    {
                        messageType = 0; //Message for everyone
                        chatPlaceholder.text = "Say to all"; //Display correct placeholder
                        chatInput.text = ""; //Make sure text is empty
                        isChatOpen = true; //Open
                        RedrawChat(); //Redraw
                        //Auto Spawn System
                        if (main.autoSpawnSystem)
                        {
                            main.autoSpawnSystem.Interruption();
                        }
                    }
                    else
                    {
                        //Check if we can send
                        if (!chatInput.text.IsNullOrWhiteSpace())
                        {
                            //Send input
                            SendChatMessage(chatInput.text, messageType);
                            //Reset
                            chatInput.text = "";
                        }
                        lastChatOpen = Time.time; //Set time
                        lastMessageReceived = Time.time; //Also set this time
                        //Close chat
                        isChatOpen = false;
                        EventSystem.current.SetSelectedGameObject(null);
                        RedrawChat(); //Redraw
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    if (!isChatOpen)
                    {
                        messageType = 1; //Message for team only
                        //Check if we should display "team only"
                        if (main.currentGameModeBehaviour.isTeamGameMode)
                        {
                            chatPlaceholder.text = "Say to team"; //Display correct placeholder
                        }
                        else
                        {
                            chatPlaceholder.text = "Say to all"; //Display correct placeholder
                        }
                        chatInput.text = ""; //Make sure text is empty
                        isChatOpen = true;
                        RedrawChat(); //Redraw
                                      //Auto Spawn System
                        if (main.autoSpawnSystem)
                        {
                            main.autoSpawnSystem.Interruption();
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (isChatOpen)
                    {
                        lastChatOpen = Time.time; //Set time
                        lastMessageReceived = Time.time; //Also set this time
                        isChatOpen = false; //Close
                        RedrawChat(); //Redraw
                    }
                }
            }
            #endregion

            #region UI
            if (isChatOpen)
            {
                chatAlpha.alpha = 1f;
                messageAlpha.alpha = 1f;
                chatScrollAlpha.alpha = 1f;
            }
            else
            {
                //Set alpha according to times
                chatAlpha.alpha = Mathf.Clamp01((lastChatOpen + fadeOutTimeNormal) - Time.time);
                messageAlpha.alpha = Mathf.Clamp01((lastMessageReceived + fadeOutTimeMessages) - Time.time);
                chatScrollAlpha.alpha = chatAlpha.alpha; //Just copy
            }
            #endregion
        }
        #endregion
    }
}