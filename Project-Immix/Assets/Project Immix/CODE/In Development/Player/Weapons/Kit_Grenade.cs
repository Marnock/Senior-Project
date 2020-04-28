using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This script is the actual grenade
    /// </summary>
    public class Kit_Grenade : Photon.Pun.MonoBehaviourPun
    {

        public float explosionTime = 5f;

 
        public Rigidbody rb;

 
        public GameObject explosionPrefab;

        [HideInInspector]
        public int gunId;

        private int ownerId;
 
        private bool ownerBot;

        IEnumerator Start()
        {
            if (photonView.IsMine)
            {
                //Should be at start, but just to make sure!
                rb.isKinematic = false;
                //Wait
                yield return new WaitForSeconds(explosionTime);
                //Then just destroy
                PhotonNetwork.Destroy(gameObject);
                //cached owner id
                ownerId = photonView.OwnerActorNr;
            }
            else
            {
                rb.isKinematic = true;
            }

            ownerBot = (bool)photonView.InstantiationData[0];
            ownerId = (int)photonView.InstantiationData[1];
        }

        void OnDestroy()
        {
            if (explosionPrefab)
            {
                GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation);
                if (go.GetComponent<Kit_Explosion>())
                {
                    go.GetComponent<Kit_Explosion>().Explode(photonView.IsMine, ownerBot, ownerId, gunId);
                }
                if (go.GetComponent<Kit_FlashbangExplosion>())
                {
                    go.GetComponent<Kit_FlashbangExplosion>().Explode(photonView.IsMine, ownerBot, ownerId, gunId);
                }
            }
        }
    }
}