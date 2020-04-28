using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_AttachmentOverrideSounds : Kit_AttachmentBehaviour
        {
     
            public AudioClip fireSound;
  
            public AudioClip fireSoundThirdPerson;
      
            public float fireSoundThirdPersonMaxRange = 300f;
             public AnimationCurve fireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);
   
            public bool silencesWeapon = true;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}
