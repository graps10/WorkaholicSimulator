using Core.ObjectPool;
using UnityEngine;

namespace Entities.Molds
{
    [CreateAssetMenu(fileName = "Mold", menuName = "Entities/Molds/Mold")]
    public class SimpleEntityMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfo => prefabPoolInfo;

        [SerializeField] private PrefabPoolInfo prefabPoolInfo;
    }
}