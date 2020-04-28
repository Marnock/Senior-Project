using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ImmixKit
{

    public abstract class Kit_ChatBase : MonoBehaviour
    {

        public abstract void DisplayChatMessage(Photon.Realtime.Player sender, string message, int type);


        public abstract void PlayerLeft(Photon.Realtime.Player player);

        public abstract void PlayerJoined(Photon.Realtime.Player player);

 
        public abstract void BotLeft(string botName);

      
        public abstract void BotJoined(string botName);

     
        public abstract void MasterClientSwitched(Photon.Realtime.Player player);

        public virtual void PauseMenuOpened()
        {

        }

        public virtual void PauseMenuClosed()
        {

        }


        public void SendChatMessage(string content, int targets)
        {
            //Check if we have a master player
            if (PhotonNetwork.MasterClient != null)
            {
                //Create message content
                Hashtable messageContent = new Hashtable(3);
                //Set type
                messageContent[(byte)0] = 0;
                //Set our message (content)
                messageContent[(byte)1] = content;
                //Set who we want the message to see
                messageContent[(byte)2] = targets;
                //Send it to the master client only. He decides who will get the actual message.
                PhotonNetwork.RaiseEvent(1, messageContent, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.MasterClient.ActorNumber } }, SendOptions.SendUnreliable);
            }
        }

        public void SendBotMessage(string botSender, int content)
        {
            //Create message content
            Hashtable messageContent = new Hashtable(3);
            //Set type
            messageContent[(byte)0] = 1;
            //Set sender
            messageContent[(byte)1] = botSender;
            //Set our message (content)
            messageContent[(byte)2] = content;

            //Send it to everyone
            PhotonNetwork.RaiseEvent(1, messageContent, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);
        }
    }
}
