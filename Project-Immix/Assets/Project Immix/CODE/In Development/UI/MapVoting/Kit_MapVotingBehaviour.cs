using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace ImmixKit
{

    public class MapGameModeCombo
    {
        public int map;
    
        public int gameMode;
    }

    public class Kit_MapVotingBehaviour : MonoBehaviourPunCallbacks, IPunObservable
    {

        Kit_IngameMain main;
    
        public List<MapGameModeCombo> combos = new List<MapGameModeCombo>();

        public List<int> currentVotes = new List<int>();

        void Start()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Assign
            main.currentMapVoting = this;
            //Callback
            main.MapVotingOpened();
            //Get data
            object[] data = photonView.InstantiationData;
            combos = new List<MapGameModeCombo>();

            //Loop through it and turn it back into combos
            for (int i = 0; i < data.Length; i++)
            {
                //Since they are in linear order (gameMode, map, new gameMode, next map, etc) we only do a new one every two steps
                if (i % 2 == 0)
                {
                    //Create new combo
                    MapGameModeCombo newCombo = new MapGameModeCombo();
                    //Read from the network
                    newCombo.gameMode = (int)data[i];
                    newCombo.map = (int)data[i + 1];
                    //Add to the list
                    combos.Add(newCombo);
                }
            }

            //Setup votes
            while (currentVotes.Count < combos.Count) currentVotes.Add(0);

            //Setup the UI
            main.mapVotingUI.SetupVotes(combos);
        }

        void OnDestroy()
        {
            if (main)
            {
                main.mapVotingUI.Hide();
            }
        }

        #region Photon Calls
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //First send length
                stream.SendNext(currentVotes.Count);
                //Then send all votes in correct order
                for (int i = 0; i < currentVotes.Count; i++)
                {
                    stream.SendNext(currentVotes[i]);
                }
            }
            else
            {
                //Get count
                int count = (int)stream.ReceiveNext();
                //Check if we have enough count
                while (currentVotes.Count < count) currentVotes.Add(0);
                //Then receive all votes in correct order
                for (int i = 0; i < count; i++)
                {
                    currentVotes[i] = (int)stream.ReceiveNext();
                }
                if (main && main.mapVotingUI)
                {
                    //Then proceed to redraw
                    main.mapVotingUI.RedrawVotes(this);
                }
            }
        }
        #endregion

        #region Custom Calls
        public void RecalculateVotes()
        {
            //Only the master client calculates votes
            if (PhotonNetwork.IsMasterClient)
            {
                //Reset votes
                for (int i = 0; i < currentVotes.Count; i++)
                {
                    currentVotes[i] = 0;
                }

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    Hashtable table = PhotonNetwork.PlayerList[i].CustomProperties;
                    //Check who this player has voted for
                    if (table["vote"] != null)
                    {
                        int vote = (int)table["vote"];
                        if (vote >= 0 && vote < currentVotes.Count)
                        {
                            if (vote < currentVotes.Count)
                            {
                                //Add that vote
                                currentVotes[vote]++;
                            }
                        }
                    }
                }

                //Redraw on the master client
                main.mapVotingUI.RedrawVotes(this);
            }
        }

        public MapGameModeCombo GetComboWithMostVotes()
        {
            MapGameModeCombo toReturn = combos[0];
            int mostVotes = 0;
            int mostVotesIndex = 0;

            //Check which one has the most votes
            for (int i = 0; i < currentVotes.Count; i++)
            {
                if (currentVotes[i] > mostVotes)
                {
                    mostVotes = currentVotes[i];
                    mostVotesIndex = i;
                }
            }

            //Set
            toReturn = combos[mostVotesIndex];

            //Return it
            return toReturn;
        }
        #endregion

        #region Static functions
        public static MapGameModeCombo GetMapGameModeCombo(Kit_GameInformation game, List<MapGameModeCombo> used)
        {
            //First select a random game mode and map
            int gameMode = Random.Range(0, game.allGameModes.Length);
            int map = 0;

            if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
            {
                map = Random.Range(0, game.allGameModes[gameMode].traditionalMaps.Length);
            }
            else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
            {
                map = Random.Range(0, game.allGameModes[gameMode].lobbyMaps.Length);
            }

            //To prevent an infite loop if all game modes are already used
            int tries = 0;
            while (IsGameModeUsed(gameMode, used) && tries < 10)
            {
                gameMode = Random.Range(0, game.allGameModes.Length);
                tries++;
            }

            //Reset tries
            tries = 0;
            while (IsMapUsed(map, used) && tries < 10)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    map = Random.Range(0, game.allGameModes[gameMode].traditionalMaps.Length);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    map = Random.Range(0, game.allGameModes[gameMode].lobbyMaps.Length);
                }
                tries++;
            }

            //Create class and return it
            return new MapGameModeCombo { map = map, gameMode = gameMode };
        }

        static bool IsGameModeUsed(int gameMode, List<MapGameModeCombo> used)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].gameMode == gameMode)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsMapUsed(int map, List<MapGameModeCombo> used)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].map == map)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
