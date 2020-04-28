using UnityEngine;

namespace ImmixKit
{

    public enum PointType { Kill }

   
    public abstract class Kit_PointsUIBase : MonoBehaviour
    {
        public abstract void DisplayPoints(int points, PointType type);
    }
}
