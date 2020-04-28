using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_SimpleChatEntry : MonoBehaviour
    {
 
        public Text txt;

        public void Setup(string content)
        {
            //Set it up
            txt.text = content; //Text
        }
    }
}
