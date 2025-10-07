using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Transition
{
    public abstract class AbstractSceneTransitionScriptableObject : ScriptableObject
    {
        protected const float Transition_Completion_Threshold = 0.98f;
        
        [SerializeField] protected AnimationCurve lerpCurve;
        [SerializeField] protected float animationSpeedMultiplier = 0.25f;
        [SerializeField] protected Image animatedObject;
        
        public static event Action OnEnterCompleted;

        public abstract IEnumerator Enter(bool expectSceneLoad = true);
        public abstract IEnumerator Exit();

        public void InitializeAnimatedObject(Image image) 
            => animatedObject = image;
        
        protected static void OnEnterFinished() 
            => OnEnterCompleted?.Invoke();
    }
}