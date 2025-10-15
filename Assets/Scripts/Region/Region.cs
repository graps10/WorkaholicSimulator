using System.Collections.Generic;
using Core;
using Core.Utilities;
using UnityEngine;

namespace Region
{
    public class Region : Sector
    {
        [HideInInspector] [SerializeField] private bool showRegion;
        [field: SerializeField] public int Level { get; private set; }

        public bool ShowRegion
        { 
            get => showRegion;
            set
            {
                showRegion = value;
#if UNITY_EDITOR
                ToggleDisplayBounds(value); 
#endif
            }
        }
        
        private List <Sector> _sectors;

        private void AddToRegionManager() => RegionManager.AddToRegions(this);

        protected override string MaterialPath => "Assets/Materials/EditorLocationBounds/RegionMaterial.mat";

        private void Awake()
        {
            if (Player.Instance != null)
                Initialize();
        }
        
        private new void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Location already initialized");
                return;
            }

            isInitialized = true;

            AddToRegionManager();
            
            if (locationBounds == null)
            {
                //LocationBounds = BoundsSceneElement.Create<SectorBounds>(gameObject.name, this, boundsMaterial, transform);
                locationBounds.CreateMeshBounds();
            }

            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);

            foreach (var sector in _sectors)
            {
                sector.SetRegion(this);
                sector.Initialize();
            }
        }

        public override void Exit()
        {
            foreach (var sector in _sectors)
                sector.Dispose();
            
            onExit?.Invoke();
        }
        
        public List<Sector> GetSectors() => _sectors;

        public override List<Bounds> GetAllBounds()
        {
            List <Bounds> collectedLocationBounds = new();
            
            if (_sectors == null) return collectedLocationBounds;
            
            foreach (var item in _sectors)
                collectedLocationBounds.Add(item.Bounds);

            return collectedLocationBounds;
        }

        public override void CalculateBounds(bool displayBounds = true)
        {
            if (_sectors != null)
                foreach (Sector sector in _sectors)
                    sector.CalculateBounds(displayBounds);

#if UNITY_EDITOR
            AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
            if (locationBounds == null)
                //LocationBounds = BoundsSceneElement.Create<RegionBounds>(gameObject.name, this, boundsMaterial, transform);
            if (locationBounds == null) return;
            
            locationBounds.CreateMeshBounds();

            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);
#if UNITY_EDITOR
            ToggleDisplayBounds(displayBounds);
#endif
        }
        
#if UNITY_EDITOR
        
        protected override void ReloadForEditor()
        {
            CreateEnvironmentParent();
            
            _sectors ??= new List <Sector>();
            CalculateBounds();
        }
#endif
    }
}