using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_AttachmentAimOverride : Kit_AttachmentBehaviour
        {
         
            public Vector3 aimPos;
        
            public Vector3 aimRot;
        
            public float aimFov = 40f;
      
            public bool useFullscreenScope;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}
