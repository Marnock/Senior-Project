using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace ImmixKit
{
    /// <summary>
    /// This script is the actual smoke grenade
    /// </summary>
    public class Kit_GrenadeSmoke : Photon.Pun.MonoBehaviourPun, IPunObservable
    {
   
        public float timeUntilSmoke = 5f;

        public float timeUntilDestroy = 20f;


        public Rigidbody rb;

     
        public ParticleSystem smoke;
     
        private bool smokeFired;
      
        private bool wasSmokeFired;

        IEnumerator Start()
        {
            if (photonView.IsMine)
            {
                rb.isKinematic = false;
                //Wait
                yield return new WaitForSeconds(timeUntilSmoke);
                smokeFired = true;
                yield return new WaitForSeconds(timeUntilDestroy);
                //Then just destroy
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                rb.isKinematic = true;
            }
        }

        void Update()
        {
            if (smokeFired && smokeFired != wasSmokeFired)
            {
                smoke.transform.up = Vector3.up;
                wasSmokeFired = smokeFired;
                smoke.Play(true);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(smokeFired);
            }
            else
            {
                smokeFired = (bool)stream.ReceiveNext();
            }
        }
    }
}