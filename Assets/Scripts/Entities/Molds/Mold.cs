using Core.ObjectPool;
using UnityEngine;

namespace Entities.Molds
{
    public abstract class Mold : ScriptableObject
    {
        public abstract PrefabPoolInfo PrefabPoolInfo { get; }
    }
}