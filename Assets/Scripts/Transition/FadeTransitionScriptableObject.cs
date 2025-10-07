using System.Collections;
using UnityEngine;

namespace Transition
{
    [CreateAssetMenu(fileName = "Fade", menuName = "Scene Transition/Fade")]
    public class FadeTransitionScriptableObject : AbstractSceneTransitionScriptableObject
    {
        [SerializeField] private Color fadeColor;

        public override IEnumerator Enter(bool expectSceneLoad = true)
        {
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime * animationSpeedMultiplier;
                float transitionValue = lerpCurve.Evaluate(time);
                
                animatedObject.color = new Color(0, 0, 0, transitionValue);
                
                if (transitionValue >= Transition_Completion_Threshold && expectSceneLoad)
                {
                    // call event
                }
                yield return null;
            }
            
            OnEnterFinished();
        }

        public override IEnumerator Exit()
        {
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime * animationSpeedMultiplier;
                float transitionValue = lerpCurve.Evaluate(time);
                
                animatedObject.color = new Color(0, 0, 0, 1 - transitionValue); 
                yield return null;
            }
        }
    }
}