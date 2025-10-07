using System.Collections;
using UnityEngine;

namespace Transition
{
    [CreateAssetMenu(fileName = "Scale", menuName = "Scene Transition/Scale")]
    public class ScaleTransitionScriptableObject : AbstractSceneTransitionScriptableObject
    {
        [SerializeField] protected Sprite scaleSprite;
        [SerializeField] protected Color scaleColor;

        public override IEnumerator Enter(bool expectSceneLoad = true)
        {
            animatedObject.color = scaleColor;
            animatedObject.sprite = scaleSprite;

            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime * animationSpeedMultiplier;
                float transitionValue = lerpCurve.Evaluate(time);
                
                animatedObject.transform.localScale = Vector3.one * transitionValue;
                
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
                
                animatedObject.transform.localScale = Vector3.one * (1 - transitionValue); 
                
                yield return null;
            }
        }
    }
}