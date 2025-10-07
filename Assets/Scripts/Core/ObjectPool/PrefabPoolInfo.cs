using System;
using UnityEngine;

namespace Core.ObjectPool
{
    [CreateAssetMenu(fileName = "New PrefabPoolInfo ", menuName = "ObjectPool/_PrefabPoolInfo")]
    [Serializable]
    public class PrefabPoolInfo : ScriptableObject
    {
        [field: SerializeField] public string PoolName { get; private set; }
        [field: SerializeField] public string ObjectPath { get; private set; }
        [field: SerializeField] public int DefaultAmount { get; private set; }
        [field: SerializeField] public float MaxAmountMultiplier { get; private set; }

        public void Initialize(string poolName, string objectPath, int defaultAmount, float maxAmountMultiplier)
        {
            PoolName = poolName;
            ObjectPath = objectPath;
            DefaultAmount = defaultAmount;
            MaxAmountMultiplier = maxAmountMultiplier;
        }

    }
}