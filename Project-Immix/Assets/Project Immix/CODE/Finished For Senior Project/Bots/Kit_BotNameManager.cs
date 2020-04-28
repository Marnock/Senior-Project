using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_BotNameManager : ScriptableObject
    {
   
        public abstract string GetRandomName(Kit_BotManager bm);
    }
}
