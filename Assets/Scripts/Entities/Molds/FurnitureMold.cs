using Core.ObjectPool;
using UnityEditor;
using UnityEngine;

namespace Entities.Molds
{
    [CreateAssetMenu(fileName = "FurnitureMold", menuName = "Entities/Molds/Furniture Mold")]
    public class FurnitureMold : SimpleEntityMold
    {
        [Header("Identity")]
        [SerializeField] private string id;
        
        [Header("Shop Data")]
        [SerializeField] private int price;
        [SerializeField] private Sprite icon;

        [Header("Placement Settings")]
        [SerializeField] private PrefabPoolInfo particleOfSocketPlacingPool;
        public bool CanBeRotatedBeforePlaced;

        // Public Getters
        public string ID => id;
        public int Price => price;
        public Sprite Icon => icon;
        public PrefabPoolInfo ParticleOfSocketPlacingPool => particleOfSocketPlacingPool;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }
        
        [ContextMenu("Generate New ID")]
        private void GenerateId()
        {
            id = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
