using UnityEngine;

namespace Core.ObjectPool
{
    public class PooledGameObject: MonoBehaviour
    {
        private string _poolName;
        
        public string GetPoolName() => _poolName;
        public void SetPoolName(string poolName) => _poolName = poolName;

        public bool IsBeingDestroyed {  get; protected set; } 

        public virtual void ReturnToPool() => ObjectPooler.ReturnPooledObject(this);

        protected virtual void OnDestroy()
        {
            IsBeingDestroyed = true;
        }
    }
}