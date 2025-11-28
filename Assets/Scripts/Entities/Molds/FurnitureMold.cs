using Core.ObjectPool;
using UnityEngine;

namespace Entities.Molds
{
    public class FurnitureMold : SimpleEntityMold
    {
        [Header("Shop Data")]
        [SerializeField] private int price;
        [SerializeField] private Sprite icon;

        [Header("Placement Settings")]
        [SerializeField] private PrefabPoolInfo particleOfSocketPlacingPool;
        public bool CanBeRotatedBeforePlaced;

        // Public Getters
        public int Price => price;
        public Sprite Icon => icon;
        public PrefabPoolInfo ParticleOfSocketPlacingPool => particleOfSocketPlacingPool;
    }
}
