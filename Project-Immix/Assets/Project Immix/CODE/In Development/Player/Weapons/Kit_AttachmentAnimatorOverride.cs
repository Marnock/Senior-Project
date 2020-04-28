using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {

        public class Kit_AttachmentAnimatorOverride : Kit_AttachmentBehaviour
        {
  
            public RuntimeAnimatorController animatorOverride;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}