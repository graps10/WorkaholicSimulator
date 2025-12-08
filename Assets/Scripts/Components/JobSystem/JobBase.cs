using System;
using UnityEngine;

namespace Components.JobSystem
{
    public abstract class JobBase : MonoBehaviour
    {
        protected JobConfig config;
        
        protected Action OnCompleteCallback;
        protected Action OnFailCallback;
        
        protected Action<int> OnAddProgressReward;
        
        public void Initialize(JobConfig config, Action onComplete, Action onFail, Action<int> onReward)
        {
            this.config = config;
            OnCompleteCallback = onComplete;
            OnFailCallback = onFail;
            OnAddProgressReward = onReward;
            
            OnJobStarted();
        }

        protected abstract void OnJobStarted();
        
        public abstract void OnJobUpdate();
        
        public abstract void OnJobEnded();
        
        protected void FinishJob(bool success)
        {
            if (success) OnCompleteCallback?.Invoke();
            else OnFailCallback?.Invoke();
        }
    }
}