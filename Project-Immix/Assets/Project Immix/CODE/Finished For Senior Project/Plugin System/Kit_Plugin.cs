using ExitGames.Client.Photon;
using ImmixKit.UI;
using ImmixKit.Weapons;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ImmixKit
{
    public class Kit_Plugin : Kit_WeaponInjection
    {

        public virtual void OnPreSetup(Kit_IngameMain main)
        {

        }

        public virtual void OnSetupDone(Kit_IngameMain main)
        {

        }

        public virtual void PluginUpdate(Kit_IngameMain main)
        {

        }

        public virtual void PluginLateUpdate(Kit_IngameMain main)
        {

        }

        public virtual void LocalPlayerChangedTeam(Kit_IngameMain main, int newTeam)
        {

        }

        public virtual void PlayerLeftRoom(Kit_IngameMain main, Player player)
        {

        }
        public virtual void PlayerJoinedRoom(Kit_IngameMain main, Player player)
        {

        }
        public virtual void MasterClientSwitched(Kit_IngameMain main, Player newMasterClient)
        {

        }


        public virtual void OnPlayerPropertiesChanged(Kit_IngameMain main, Player player, Hashtable changedProperties)
        {

        }

        public virtual void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {

        }


        public virtual void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {

        }

        public virtual void LocalPlayerSpawned(Kit_PlayerBehaviour player)
        {

        }

        public virtual void LocalPlayerDied(Kit_PlayerBehaviour player)
        {

        }

        public virtual void PlayerSpawned(Kit_PlayerBehaviour player)
        {

        }

        public virtual void PlayerDied(Kit_PlayerBehaviour player)
        {

        }


        public virtual void LocalPlayerUpdate(Kit_PlayerBehaviour player)
        {

        }

    
        public virtual void PlayerUpdate(Kit_PlayerBehaviour player)
        {

        }


        public virtual void PlayerOnPhotonSerializeView(Kit_PlayerBehaviour player, PhotonStream stream, PhotonMessageInfo info)
        {

        }

   
        public virtual void Reset(Kit_MainMenu main)
        {

        }

        public virtual void BotWasKilled(Kit_IngameMain main, Kit_Bot bot)
        {

        }

        public virtual void BotScoredKill(Kit_IngameMain main, Kit_Bot bot, Hashtable deathInformation)
        {

        }

        public virtual void LocalPlayerWasKilled(Kit_IngameMain main)
        {

        }

        public virtual void LocalPlayerScoredKill(Kit_IngameMain main, Hashtable deathInformation)
        {

        }

    
        public virtual void BotManagerOnPhotonSerializeView(Kit_BotManager manager, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        public virtual void BotWasCreated(Kit_BotManager manager, Kit_Bot bot)
        {

        }
    }
}