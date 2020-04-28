using System;
using UnityEngine;

namespace ImmixKit
{
    namespace Weapons
    {
        [System.Serializable]
        public class MaterialChange
        {
            /// <summary>
            /// Renderer that materials will be applied to
            /// </summary>
            public Renderer rendererToApplyTo;
            public Material[] materialsToApply;
        }

        public class Kit_AttachmentChangeMaterial : Kit_AttachmentBehaviour
        {
            public MaterialChange[] materialsToChange;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                //Loop through
                for (int i = 0; i < materialsToChange.Length; i++)
                {
                    materialsToChange[i].rendererToApplyTo.sharedMaterials = materialsToChange[i].materialsToApply;
                }
            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}