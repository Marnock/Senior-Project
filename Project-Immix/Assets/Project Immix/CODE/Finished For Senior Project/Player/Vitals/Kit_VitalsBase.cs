using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_VitalsBase : ScriptableObject
    {
 
        public abstract void Setup(Kit_PlayerBehaviour pb);

 
        public abstract void ApplyFallDamage(Kit_PlayerBehaviour pb, float dmg);


        public abstract void ApplyEnvironmentalDamage(Kit_PlayerBehaviour pb, float dmg, int deathSoundCategory);


        public abstract void Suicide(Kit_PlayerBehaviour pb);

 
        public abstract void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotFrom);

   
        public abstract void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, string deathCause, Vector3 shotFrom);

    
        public abstract void ApplyHeal(Kit_PlayerBehaviour pb, float heal);

        
        public abstract void CustomUpdate(Kit_PlayerBehaviour pb);
    }
}
