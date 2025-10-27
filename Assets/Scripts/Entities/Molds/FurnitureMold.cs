using Core.ObjectPool;
using UnityEngine;

namespace Entities.Molds
{
    public class FurnitureMold : SimpleEntityMold
    {
        [SerializeField] private PrefabPoolInfo particleOfSocketPlacingPool;

        public PrefabPoolInfo ParticleOfSocketPlacingPool => particleOfSocketPlacingPool;
        
        public bool CanBeRotatedBeforePlaced;
    }
}
