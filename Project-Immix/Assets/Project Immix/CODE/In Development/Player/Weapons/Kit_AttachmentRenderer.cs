using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        public class Kit_AttachmentRenderer : Kit_AttachmentBehaviour
        {
            public Renderer[] renderersToActivate;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                for (int i = 0; i < renderersToActivate.Length; i++)
                {
                    if (renderersToActivate[i])
                    {
                        renderersToActivate[i].enabled = true;
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + ": Renderer at " + i + " is not assigned.");
                    }
                }
            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                for (int i = 0; i < renderersToActivate.Length; i++)
                {
                    if (renderersToActivate[i])
                    {
                        renderersToActivate[i].enabled = false;
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + ": Renderer at " + i + " is not assigned.");
                    }
                }
            }
        }
    }
}
