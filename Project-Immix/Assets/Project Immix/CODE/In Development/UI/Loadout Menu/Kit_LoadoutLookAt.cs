using UnityEngine;

namespace ImmixKit
{
  
    public class Kit_LoadoutLookAt : MonoBehaviour
    {
        public Camera camToLookAt;

        void Update()
        {
            transform.forward = Vector3.forward;

        }
    }
}