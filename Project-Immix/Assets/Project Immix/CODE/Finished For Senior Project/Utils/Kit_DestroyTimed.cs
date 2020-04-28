using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ImmixKit
{

    public class Kit_DestroyTimed : MonoBehaviour
    {
        public float destroyAfter = 5f;

        // Use this for initialization
        void Start()
        {
            //Just destroy after set seconds
            Destroy(gameObject, destroyAfter);
        }
    }
}
