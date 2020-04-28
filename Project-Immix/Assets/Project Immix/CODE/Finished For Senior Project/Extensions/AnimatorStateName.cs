using UnityEngine;
using System.Linq;

namespace ImmixKit
{
    public class AnimatorStateName : ScriptableObject
    {
        public string[] animatorStates;
        public string GetAnimatorState(AnimatorStateInfo info, string prefix)
        {
            for (int i = 0; i < animatorStates.Length; i++)
            {
                if (prefix != "")
                {
                    if (info.IsName(prefix + "." + animatorStates[i]))
                    {
                        return animatorStates[i];
                    }
                }
                else
                {
                    if (info.IsName(animatorStates[i]))
                    {
                        return animatorStates[i];
                    }
                }
            }

            return "";
        }
    }
}