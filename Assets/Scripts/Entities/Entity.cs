using System;
using Entities.Molds;
using Core.ObjectPool;
using UnityEngine;
using UnityEngine.Events;

namespace Entities
{
    public class Entity : PooledGameObject
    {
        protected bool LogicIsEnabled { get; private set; }
        
        [SerializeField] protected UnityEvent OnEnableLogic;
        [SerializeField] protected UnityEvent OnDisableLogic;
        [SerializeField] protected Renderer[] renderers;

        public UnityEvent OnExternalActivation;
        public UnityEvent OnExternalDeactivation;
        
        public event Action OnDispose;

        public virtual void LoadEntity(Mold entityMold) { }

        public virtual void ExternalActivation(bool value)
        {
            if (value)
            {
                OnExternalActivation?.Invoke();
            }
            else
                OnExternalDeactivation?.Invoke();
        }
        
        public virtual void ToggleLogic(bool stateToSet)
        {
            LogicIsEnabled = stateToSet;
            
            if(stateToSet)
                OnEnableLogic?.Invoke();
            else
                OnDisableLogic?.Invoke();
        }

        public void SwitchGraphics(bool stateToSet)
        {
            if (renderers == null)
                return;

            foreach (var rendererComponent in renderers)
                rendererComponent.enabled = stateToSet;
        }
        
        public Renderer[] GetRenderers() => renderers;
        
        public override void ReturnToPool() // Unload entity into basic assets
        {
            SwitchGraphics(false);
            ToggleLogic(false);
            
            OnDispose?.Invoke();
            OnDispose = null;

            base.ReturnToPool();
        }
    }
}
