using Core.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.ObjectPool
{
    public static class ObjectPooler
    {
        private const string Pooled_Objects_Parent = "PooledObjects";
        
        private static readonly Dictionary<string, ObjectPool<GameObject>> activeObjectPools = new();
        private static readonly Dictionary<string, PrefabPoolInfo> activeObjectPoolsInfos = new();
        private static readonly Dictionary<string, Transform> poolParents = new();

        private static Transform GetPooledGameObjectsParent
        {
            get
            {
                if (pooledGameObjectsParent == null)
                {
                    pooledGameObjectsParent = GameObject.Find(Pooled_Objects_Parent).transform;

                    if (pooledGameObjectsParent == null)
                    {
                        Debug.LogWarning("Cant find PooledObjectsParent");
                        return null;
                    }
                }

                return pooledGameObjectsParent;
            }
        }
        
        private static Transform pooledGameObjectsParent;

        private static bool PoolExist(string poolName) => activeObjectPoolsInfos.ContainsKey(poolName);
        private static void CreateParentForPool(string poolName)
        {
            if (poolParents.ContainsKey(poolName))
                poolParents[poolName] = new GameObject(poolName).transform;
            else
                poolParents.Add(poolName, new GameObject(poolName).transform);

            if (!poolParents.TryGetValue(poolName, out Transform createdParent))
            {
                Debug.LogError("Couldn't get a parent for pool");
                return;
            }

            createdParent.transform.SetParent(GetPooledGameObjectsParent, false);
            createdParent.transform.localScale = Vector3.one;
        }

        private static void CreatePool(PrefabPoolInfo PrefabPoolInfo)
        {
            if (PoolExist(PrefabPoolInfo.PoolName))
                return;

            if (!poolParents.ContainsKey(PrefabPoolInfo.PoolName))
                CreateParentForPool(PrefabPoolInfo.PoolName);

            var pool = new ObjectPool<GameObject>
            (
                createFunc: () => CreatePooledObject(PrefabPoolInfo),

                actionOnGet: obj => obj.SetActive(true),

                actionOnRelease: obj => obj.SetActive(false),

                actionOnDestroy: RemoveObject,

                defaultCapacity: PrefabPoolInfo.DefaultAmount,

                maxSize: Mathf.CeilToInt(PrefabPoolInfo.DefaultAmount * PrefabPoolInfo.MaxAmountMultiplier)
            );

            void RemoveObject(GameObject gameObject)
            {
                if (Application.isPlaying)
                    Object.Destroy(gameObject);
                else
                    Object.DestroyImmediate(gameObject);
            }

            activeObjectPoolsInfos.Add(PrefabPoolInfo.PoolName, PrefabPoolInfo);
            activeObjectPools.Add(PrefabPoolInfo.PoolName, pool);
        }
        
        public static void ClearAllPools()
        {
            foreach (var activeObjectPool in activeObjectPools.Values)
                activeObjectPool.Clear();
        }

        private static GameObject CreatePooledObject(PrefabPoolInfo prefabPoolInfo)
        {
            if (!AssetUtils.TryLoadAsset(prefabPoolInfo.ObjectPath, out GameObject loadedObjectAsset))
                return null;

            poolParents.TryGetValue(prefabPoolInfo.PoolName, out var parent);

            if (parent == null)
            {
                CreateParentForPool(prefabPoolInfo.PoolName);
                if (!poolParents.TryGetValue(prefabPoolInfo.PoolName, out parent))
                    return null;
            }

            var instance = Object.Instantiate(loadedObjectAsset, parent.transform, true);
            if (!instance.TryGetComponent(out PooledGameObject pooledObject))
            {
                Debug.LogError("Created object does not have the PooledGameObject component or it couldn't be found. Destroying the created GameObject");
                Object.Destroy(instance);
                return null;
            }

            pooledObject.SetPoolName(prefabPoolInfo.PoolName);
            instance.SetActive(false);

            instance.transform.localScale = Vector3.one;

            return instance;
        }

        public static PooledGameObject TakePooledGameObject(PrefabPoolInfo prefabPoolInfo, Transform transformToSet = null)
        {
            GameObject gameObject = GetPooledGameObject(prefabPoolInfo, transformToSet);
            PooledGameObject pooledGameObject = null;

            if (gameObject != null)
            {
                pooledGameObject = gameObject.GetComponent<PooledGameObject>();
            }

            return pooledGameObject;
        }

        private static GameObject GetPooledGameObject(PrefabPoolInfo prefabPoolInfo, Transform transformToSet = null)
        {
            if (prefabPoolInfo == null)
            {
                Debug.LogWarning("Pool is null.");
                return null;
            }

            if (!PoolExist(prefabPoolInfo.PoolName)) CreatePool(prefabPoolInfo);

            if (!activeObjectPools.TryGetValue(prefabPoolInfo.PoolName, out var pool))
            {
                Debug.LogWarning($"No active pool found with name: {prefabPoolInfo.PoolName}");
                return null;
            }

            var pooledGameObject = pool.Get();

            if (pooledGameObject == null)
            {
                Debug.LogWarning($"No pooled object available in pool: {prefabPoolInfo.PoolName}");
                return null;
            }
            
            if (transformToSet != null)
            {
                pooledGameObject.transform.position = transformToSet.position;
                pooledGameObject.transform.rotation = transformToSet.rotation;
                pooledGameObject.transform.localScale = transformToSet.localScale;
            }

            if (!poolParents.ContainsKey(prefabPoolInfo.PoolName))
            {
                poolParents.Add(prefabPoolInfo.PoolName, new GameObject(prefabPoolInfo.PoolName).transform);
                poolParents.TryGetValue(prefabPoolInfo.PoolName, out Transform createdParent);
                if (createdParent != null)
                {
                    createdParent.transform.SetParent(pooledGameObjectsParent);
                    createdParent.transform.localScale = Vector3.one;
                }
            }

            if (poolParents.TryGetValue(prefabPoolInfo.PoolName, out Transform parent))
            {
                pooledGameObject.transform.SetParent(parent.transform);
                pooledGameObject.transform.localScale = Vector3.one;
            }

            return pooledGameObject;
        }

        internal static void ReturnPooledObject(PooledGameObject pooledObjectToReturn)
        {
            if (pooledObjectToReturn == null || pooledObjectToReturn.IsBeingDestroyed)
                return;

            if (!activeObjectPools.TryGetValue(pooledObjectToReturn.GetPoolName(), out var pool))
            {
                Debug.LogWarning($"No existing pool {pooledObjectToReturn.gameObject.name} has been destroyed");
                Object.Destroy(pooledObjectToReturn.gameObject);
                return;
            }

            poolParents.TryGetValue(pooledObjectToReturn.GetPoolName(), out Transform parent);
            
            if (parent == null)
            {
                Debug.LogError($"Couldn't find parent for PooledGameObject while returning it. GameObject name: {pooledObjectToReturn.gameObject.name}");
                return;
            }

            if (parent == pooledObjectToReturn.transform.parent) 
                return;
            
            pooledObjectToReturn.transform.SetParent(parent.transform);
            pool.Release(pooledObjectToReturn.gameObject);
        }

        public static void ClearPooler()
        {
            poolParents.Clear();
            activeObjectPools.Clear();
            activeObjectPoolsInfos.Clear();
        }
    }
}
