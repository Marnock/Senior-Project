using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// Class used for storing runtime data
    /// </summary>
    public class PlayerNameRuntimeData
    {
     
        public int myId;
     
        public float lastTimeSeen;
    }

    public class Kit_PlayerNameDefault : Kit_PlayerNameUIBase
    {
      
        public float visibleTimeThreshold = 0.4f;

        
        public float nameDistance = 15f;

        public override void OnDestroyRelay(Kit_PlayerBehaviour pb)
        {
            //Get Data
            PlayerNameRuntimeData pnrd = (PlayerNameRuntimeData)pb.customNameData;
            if (pnrd != null)
            {
                //Check if we have an id assigned
                if (pnrd.myId >= 0)
                {
                    pb.main.hud.ReleasePlayerMarker(pnrd.myId);
                }
            }
        }

        public override void PlayerSpotted(Kit_PlayerBehaviour pb, float validFor)
        {
            //Get Data
            PlayerNameRuntimeData pnrd = (PlayerNameRuntimeData)pb.customNameData;
            //Set data
            pnrd.lastTimeSeen = Time.time + validFor + visibleTimeThreshold;
        }

        public override void StartRelay(Kit_PlayerBehaviour pb)
        {
            PlayerNameRuntimeData prnd = new PlayerNameRuntimeData();
            //Assign data
            pb.customNameData = prnd;
            //Get id
            prnd.myId = pb.main.hud.GetUnusedPlayerMarker();
        }

        public override void UpdateEnemy(Kit_PlayerBehaviour pb)
        {
            //Get Data
            PlayerNameRuntimeData pnrd = (PlayerNameRuntimeData)pb.customNameData;
            if (pb.isBot)
            {
                Kit_Bot bot = pb.main.currentBotManager.GetBotWithID(pb.botId);
                if (bot == null)
                    Debug.Log(pb.botId);
                //Check if we are visible
                if (Time.time <= pnrd.lastTimeSeen)
                {
                    //Display us
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.enemy, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, bot.name);
                }
                else
                {
                    //Do not display us
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.none, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, bot.name);
                }
            }
            else
            {
                //Check if we are visible
                if (Time.time <= pnrd.lastTimeSeen)
                {
                    //Display us
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.enemy, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, pb.photonView.Owner.NickName);
                }
                else
                {
                    //Do not display us
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.none, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, pb.photonView.Owner.NickName);
                }
            }
        }

        public override void UpdateFriendly(Kit_PlayerBehaviour pb)
        {
            //Get Data
            PlayerNameRuntimeData pnrd = (PlayerNameRuntimeData)pb.customNameData;
            //Check distance
            float distance = Vector3.Distance(pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, pb.main.mainCamera.transform.position);
            if (pb.isBot)
            {
                Kit_Bot bot = pb.main.currentBotManager.GetBotWithID(pb.botId);
                if (distance < nameDistance)
                {
                    //Display us as name
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.friendlyClose, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, bot.name);
                }
                else
                {
                    //Display us as marker
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.friendlyFar, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, bot.name);
                }
            }
            else
            {
                if (distance < nameDistance)
                {
                    //Display us as name
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.friendlyClose, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, pb.photonView.Owner.NickName);
                }
                else
                {
                    //Display us as marker
                    pb.main.hud.UpdatePlayerMarker(pnrd.myId, PlayerNameState.friendlyFar, pb.thirdPersonPlayerModel.enemyNameAboveHeadPos.position, pb.photonView.Owner.NickName);
                }
            }
        }
    }
}