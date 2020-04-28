using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{

    /// <summary>
    /// Fills up bots to the player limit
    /// </summary>
    public class Kit_BotGameModeFillup : Kit_BotGameModeManagerBase
    {
        public override void Inizialize(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }

        public override void PlayerJoinedTeam(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                int tries = 0;
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers && tries <= 20)
                    {
                        manager.AddNewBot();
                        tries++;
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers && tries <= 20)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                        tries++;
                    }
                }
            }
        }

        public override void PlayerLeftTeam(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }
    }
}
