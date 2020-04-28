using UnityEngine;

namespace ImmixKit
{
    public class Kit_LoadoutIKHelper : MonoBehaviour
    {
    
        public Animator anim;
   
        public Transform leftHandGoal;

           public bool applyIk;

        void OnAnimatorIK()
        {
            if (applyIk)
            {
                anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGoal.position);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandGoal.rotation);
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            }
            else
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            }
        }
    }
}