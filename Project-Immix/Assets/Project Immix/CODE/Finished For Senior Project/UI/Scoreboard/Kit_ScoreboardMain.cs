using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;

namespace ImmixKit
{
    namespace Scoreboard
    {
      
        public class Kit_ScoreboardHelper
        {
      
            public string name;
    
            public int team;
      
            public int kills;
        
            public int deaths;
        
            public int ping;
          
            public bool used;
        }

        public class Kit_ScoreboardMain : Kit_ScoreboardBase
        {
            public Kit_IngameMain main;

        
            public GameObject scoreboardRoot;

            [Header("Team Game Mode")]
           
            public GameObject teamGameModeRoot;
          
            public RectTransform teamOneEntriesGo;
           
            public RectTransform teamTwoEntriesGo;
        
            public GameObject teamPrefab;

     
            private List<Kit_ScoreboardUIEntry> activeEntriesTeamOne = new List<Kit_ScoreboardUIEntry>();

       
            private List<Kit_ScoreboardUIEntry> activeEntriesTeamTwo = new List<Kit_ScoreboardUIEntry>();

            [Header("Non Team Game Mode")]
       
            public GameObject nonTeamGameModeRoot;
         
            public RectTransform entriesGo;
           
            public GameObject entryPrefab;

            
            private List<Kit_ScoreboardUIEntry> activeEntries = new List<Kit_ScoreboardUIEntry>();

            [Header("Settings")]
          
            public float redrawFrequency = 1f;

         
            private float lastRedraw;

            //[HideInInspector]
            public bool canUseScoreboard;

            #region Runtime
            private List<Kit_ScoreboardHelper> rt_ScoreboardEntries = new List<Kit_ScoreboardHelper>();
            #endregion

            void Update()
            {
                //Check if we can use the scoreboard
                if (canUseScoreboard)
                {
                    //Check for input
                    if (Input.GetKey(KeyCode.Tab))
                    {
                        //Check if its not already open
                        if (!isOpen)
                        {
                            isOpen = true;
                            //Redraw
                            Redraw();
                        }
                    }
                    else
                    {
                        //Check if it is open
                        if (isOpen)
                        {
                            isOpen = false;
                            //Redraw
                            Redraw();
                        }
                    }

                    if (isOpen)
                    {
                        if (Time.time > lastRedraw)
                        {
                            Redraw();
                        }
                    }
                }
            }

            public override void Disable()
            {
                //Disable use of scoreboard
                canUseScoreboard = false;
                //Force scoreboard to close
                isOpen = false;
                //Redraw
                Redraw();
            }

            public override void Enable()
            {
                //Enable use of scoreboard
                canUseScoreboard = true;
            }

            void Redraw()
            {
                //Set time
                lastRedraw = Time.time + redrawFrequency;

                //Set root based on state
                if (isOpen)
                {
                    scoreboardRoot.SetActive(true);
                }
                else
                {
                    scoreboardRoot.SetActive(false);
                }

                //Reset entries
                for (int o = 0; o < rt_ScoreboardEntries.Count; o++)
                {
                    rt_ScoreboardEntries[o].used = false;
                }

                //Convert Photon.Realtime.Player to Scoreboard ready entries
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if entry is available
                    if (rt_ScoreboardEntries.Count > i)
                    {
                        //Update
                        //Set name
                        rt_ScoreboardEntries[i].name = PhotonNetwork.PlayerList[i].NickName;

                        //Check in which team that player is
                        if (PhotonNetwork.PlayerList[i].CustomProperties["team"] != null)
                        {
                            rt_ScoreboardEntries[i].team = (int)PhotonNetwork.PlayerList[i].CustomProperties["team"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].team = 2;
                        }

                        if (PhotonNetwork.PlayerList[i].CustomProperties["kills"] != null)
                        {
                            rt_ScoreboardEntries[i].kills = (int)PhotonNetwork.PlayerList[i].CustomProperties["kills"];

                        }
                        else
                        {
                            rt_ScoreboardEntries[i].kills = 0;
                        }

                        //Check if he has deaths
                        if (PhotonNetwork.PlayerList[i].CustomProperties["deaths"] != null)
                        {
                            rt_ScoreboardEntries[i].deaths = (int)PhotonNetwork.PlayerList[i].CustomProperties["deaths"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].deaths = 0;
                        }

                        //Check if he has ping
                        if (PhotonNetwork.PlayerList[i].CustomProperties["ping"] != null)
                        {
                            rt_ScoreboardEntries[i].ping = (int)PhotonNetwork.PlayerList[i].CustomProperties["ping"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].ping = 0;
                        }

                        rt_ScoreboardEntries[i].used = true;
                    }
                    else
                    {
                        //Create new
                        Kit_ScoreboardHelper entry = new Kit_ScoreboardHelper();

                        //Set name
                        entry.name = PhotonNetwork.PlayerList[i].NickName;

                        //Check in which team that player is
                        if (PhotonNetwork.PlayerList[i].CustomProperties["team"] != null)
                        {
                            entry.team = (int)PhotonNetwork.PlayerList[i].CustomProperties["team"];
                        }
                        else
                        {
                            entry.team = 2;
                        }

                        if (PhotonNetwork.PlayerList[i].CustomProperties["kills"] != null)
                        {
                            entry.kills = (int)PhotonNetwork.PlayerList[i].CustomProperties["kills"];

                        }
                        else
                        {
                            entry.kills = 0;
                        }

                        //Check if he has deaths
                        if (PhotonNetwork.PlayerList[i].CustomProperties["deaths"] != null)
                        {
                            entry.deaths = (int)PhotonNetwork.PlayerList[i].CustomProperties["deaths"];
                        }
                        else
                        {
                            entry.deaths = 0;
                        }

                        //Check if he has ping
                        if (PhotonNetwork.PlayerList[i].CustomProperties["ping"] != null)
                        {
                            entry.ping = (int)PhotonNetwork.PlayerList[i].CustomProperties["ping"];
                        }
                        else
                        {
                            entry.ping = 0;
                        }

                        //Set to used
                        entry.used = true;

                        //Add
                        rt_ScoreboardEntries.Add(entry);
                    }
                }

                //Convert Bots to List
                if (main.currentBotManager)
                {
                    for (int i = 0; i < main.currentBotManager.bots.Count; i++)
                    {
                        //Check if entry is available
                        if (rt_ScoreboardEntries.Count > i + PhotonNetwork.PlayerList.Length)
                        {
                            //Update
                            //Set name
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].name = main.currentBotManager.bots[i].name;
                            //Copy team
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].team = main.currentBotManager.bots[i].team;
                            //Copy kills
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].kills = main.currentBotManager.bots[i].kills;
                            //Copy Deaths
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].deaths = main.currentBotManager.bots[i].deaths;
                            //Set ping to 0
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].ping = 0;
                            //Set to used
                            rt_ScoreboardEntries[i + PhotonNetwork.PlayerList.Length].used = true;
                        }
                        else
                        {
                            //Create new
                            Kit_ScoreboardHelper entry = new Kit_ScoreboardHelper();

                            //Set name
                            entry.name = main.currentBotManager.bots[i].name;
                            //Copy team
                            entry.team = main.currentBotManager.bots[i].team;
                            //Copy kills
                            entry.kills = main.currentBotManager.bots[i].kills;
                            //Copy deaths
                            entry.deaths = main.currentBotManager.bots[i].deaths;
                            //Set ping to 0
                            entry.ping = 0;

                            //Set to used
                            entry.used = true;

                            //Add
                            rt_ScoreboardEntries.Add(entry);
                        }
                    }
                }

                //Sort List
               rt_ScoreboardEntries =  rt_ScoreboardEntries.OrderBy(x => x.kills).Reverse().ToList();

                //Different Scoreboard for team and non team game modes
                //Team Game Mode
                if (main.currentGameModeBehaviour.isTeamGameMode)
                {
                 
                }
                //Non Team Game Mode
                else
                {
                    //Use correct scoreaboard
                    if (teamGameModeRoot.activeSelf) teamGameModeRoot.SetActive(false);
                    if (!nonTeamGameModeRoot.activeSelf) nonTeamGameModeRoot.SetActive(true);

                    //Set all to unused
                    for (int o = 0; o < activeEntries.Count; o++)
                    {
                        activeEntries[o].used = false;
                    }

                    int activeIndex = 0;

                    //Redraw
                    for (int i = 0; i < rt_ScoreboardEntries.Count; i++)
                    {
                        if (rt_ScoreboardEntries[i].used)
                        {
                            //Check if we have an active entry for this player
                            if (activeEntries.Count <= activeIndex)
                            {
                                //We don't have one, create one
                                GameObject go = Instantiate(entryPrefab, entriesGo, false);
                                //Reset scale
                                go.transform.localScale = Vector3.one;
                                //Add to list
                                activeEntries.Add(go.GetComponent<Kit_ScoreboardUIEntry>());
                            }

                            //Calculate total score
                            int totalScore = 0;

                            //Redraw
                            activeEntries[i].used = true; //Set to true
                            if (!activeEntries[i].gameObject.activeSelf) activeEntries[i].gameObject.SetActive(true);
                            activeEntries[i].nameText.text = rt_ScoreboardEntries[i].name; //Update nickname

                            //Kills
                            activeEntries[i].kills.text = rt_ScoreboardEntries[i].kills.ToString();
                            //Add to score
                            totalScore += rt_ScoreboardEntries[i].kills * main.gameInformation.pointsPerKill;

                            //Check if he has deaths
                            activeEntries[i].deaths.text = rt_ScoreboardEntries[i].deaths.ToString();

                            //Check if he has ping
                            activeEntries[i].ping.text = rt_ScoreboardEntries[i].ping.ToString();

                            //Update score
                            activeEntries[i].score.text = totalScore.ToString();

                            activeIndex++;
                        }
                    }

                    //Disable unused ones
                    for (int p = 0; p < activeEntries.Count; p++)
                    {
                        if (!activeEntries[p].used)
                        {
                            activeEntries[p].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
