using UnityEngine;
using UnityEngine.UI;

namespace ImmixKit
{
    public class Kit_PointsPopup : Kit_PointsUIBase
    {
    
        public AnimationCurve fontSizeCurve;
        public float fontSizeCurveLength;
     
        public float fontSizeDelta = 1f;
      
        public Text fontAnimation;
   
        public float timeToStackPoints = 2f;
       
        private float lastPointAdd;

        public int currentPoints;

        public override void DisplayPoints(int points, PointType type)
        {
            //Set size
            fontSizeDelta = 0f;

            //Check if we can still stack ppoints
            if (lastPointAdd + timeToStackPoints > Time.time)
            {
                //Add
                lastPointAdd = Time.time;
                currentPoints += points;
            }
            else
            {
                //We can't, set points
                lastPointAdd = Time.time;
                currentPoints = points;
            }

            //Set text
            fontAnimation.text = "+" + currentPoints.ToString();
        }

        void Update()
        {
            fontAnimation.fontSize = Mathf.RoundToInt(fontSizeCurve.Evaluate(fontSizeDelta));

            if (fontSizeDelta < 1f)
            {
                fontSizeDelta += Time.deltaTime / fontSizeCurveLength;
                fontSizeDelta = Mathf.Clamp(fontSizeDelta, 0, 1f);
            }

            if (lastPointAdd + timeToStackPoints > Time.time)
            {
                fontAnimation.enabled = true;
            }
            else
            {
                fontAnimation.enabled = false;
            }
        }
    }
}
