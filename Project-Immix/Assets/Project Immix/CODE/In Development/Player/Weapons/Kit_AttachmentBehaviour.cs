using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
    
        [System.Serializable]
        public class AttachmentSlot
        {
     
            public string name;

      
            public Transform uiPosition;

    
            public Attachment[] attachments;
        }

 
        [System.Serializable]
        public class Attachment
        {

            public string name;

            public Kit_AttachmentBehaviour[] attachmentBehaviours;
        }

  
        public enum AttachmentUseCase { FirstPerson, ThirdPerson, Drop }

        public abstract class Kit_AttachmentBehaviour : MonoBehaviour
        {
    
            public Kit_AttachmentBehaviour thirdPersonEquivalent;

       
            public virtual bool RequiresSyncing()
            {
                return false;
            }

            /// <summary>
            /// Call for sync (FP is sender, TP is receiver)
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="info"></param>
            public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int index)
            {

            }

            /// <summary>
            /// Call for sync (FP is sender, TP is receiver)
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="info"></param>
            public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerOthersRuntimeData data, int index)
            {

            }

            public virtual bool RequiresInteraction()
            {
                return false;
            }

 
            public virtual void Interaction(Kit_PlayerBehaviour pb)
            {

            }

            /// <summary>
            /// For local sync in third person (and bots as master client), this is called from first person to sync.
            /// </summary>
            /// <param name="obj"></param>
            public virtual void SyncFromFirstPerson(object obj)
            {

            }

            public abstract void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc);

            public abstract void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc);
        }
    }
}