using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_DropBehaviour : MonoBehaviourPunCallbacks
        {
            public Rigidbody body;

            public GameObject rendererRoot;
            /// <summary>
            /// How long is this weapon drop going to be live?
            /// </summary>
            public float lifeTime = 30f;

            [HideInInspector]
            public int weaponID;
            [HideInInspector]
            public int bulletsLeft;
            [HideInInspector]
            public int bulletsLeftToReload;
            [HideInInspector]
            public int[] attachments;
  
            [HideInInspector]
            public bool isSceneOwned;

            void Start()
            {
                //Find main
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();

                //Get data
                object[] instData = photonView.InstantiationData;
                //Get weapon
                int weaponId = (int)instData[0];
                weaponID = weaponId;
                //Read bullets
                bulletsLeft = (int)instData[1];
                bulletsLeftToReload = (int)instData[2];
                //Read amount of attachments
                int attachmentLength = (int)instData[3];
                int[] newAttachments = new int[attachmentLength];
                //Read attachments
                for (int i = 0; i < newAttachments.Length; i++)
                {
                    newAttachments[i] = (int)instData[4 + i];
                }

                attachments = newAttachments;
                //Instantiate renderer
                GameObject pr = Instantiate(main.gameInformation.allWeapons[weaponId].dropPrefab, rendererRoot.transform);
                //Set scale
                pr.transform.localScale = Vector3.one;
                //Get Renderer
                Kit_DropRenderer render = pr.GetComponent<Kit_DropRenderer>();
                //Setup Attachments
                render.SetAttachments(attachments);

                //Setup rigidbody
                if (photonView.IsMine)
                {
                    body.isKinematic = false;

                    if (!isSceneOwned)
                        StartCoroutine(DestroyTimed());
                }
                else
                {
                    body.isKinematic = true;
                }
            }

            void Update()
            {
                if (isSceneOwned)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        body.isKinematic = false;
                    }
                    else
                    {
                        body.isKinematic = true;
                    }
                }
            }

            IEnumerator DestroyTimed()
            {
                yield return new WaitForSeconds(lifeTime);
                PhotonNetwork.Destroy(photonView);
            }

            [PunRPC]
            public void PickedUp()
            {
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(photonView);
                }
            }
        }
    }
}
