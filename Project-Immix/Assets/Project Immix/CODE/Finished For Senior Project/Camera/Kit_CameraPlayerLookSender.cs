using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    ///  finds if we are looking at someone and notifies that player when we are looking at them
    /// </summary>
    public class Kit_CameraPlayerLookSender : MonoBehaviour
    {
   
        public float rayLength = 100f;
    
        public LayerMask rayMask;
    
        public float rayTime = 0f;

   
        private float lastCheck;

        public RaycastHit hit;
        

        void Update()
        {
            if (rayTime <= 0 || Time.time >= lastCheck + rayTime)
            {
                //Set time
                lastCheck = Time.time;
                //Fire Ray
                if (Physics.Raycast(transform.position, transform.forward, out hit, rayLength, rayMask.value))
                {
                    //Check if we hit something that belongs to a player
                    Kit_PlayerBehaviour pb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                    if (pb && (!pb.photonView.IsMine || pb.isBot    ))
                    {
                        //Check if the player has a name system assigned
                        if (pb.nameManager)
                        {
                            //Tell the system we hit him
                            pb.nameManager.PlayerSpotted(pb, rayTime);
                        }
                    }
                }
            }
        }
    }
}
