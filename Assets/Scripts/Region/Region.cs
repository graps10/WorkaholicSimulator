using System.Collections.Generic;
using Core.PlayerSystem;
using Core.Utilities;
using Region.BoundsInEditor;
using UnityEngine;

namespace Region
{
    public class Region : Sector
    {
        [HideInInspector] [SerializeField] private bool showRegion;
        [HideInInspector] [SerializeField] private List <Sector> sectors;
        
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
                locationBounds = BoundsSceneElement.Create<SectorBounds>(this, boundsMaterial, transform);
                locationBounds.CreateMeshBounds();
            }

            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);

            foreach (var sector in sectors)
            {
                sector.SetRegion(this);
                sector.Initialize();
            }
        }

        public override void Exit()
        {
            foreach (var sector in sectors)
                sector.Dispose();
            
            onExit?.Invoke();
        }
        
        public List<Sector> GetSectors() => sectors;
        public void AddSector(Sector sector) => sectors.Add(sector);

        public void InitializeNewSectorsList() => sectors ??= new List<Sector>();

        public override List<Bounds> GetAllBounds()
        {
            List <Bounds> collectedLocationBounds = new();
            
            if (sectors == null) return collectedLocationBounds;
            
            foreach (var item in sectors)
                collectedLocationBounds.Add(item.Bounds);

            return collectedLocationBounds;
        }

        public override void CalculateBounds(bool displayBounds = true)
        {
            if (sectors != null)
                foreach (Sector sector in sectors)
                    sector.CalculateBounds(displayBounds);

#if UNITY_EDITOR
            AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
            if (locationBounds == null)
                locationBounds = BoundsSceneElement.Create<RegionBounds>(this, boundsMaterial, transform);
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
            InitializeNewSectorsList();
            CalculateBounds();
        }
#endif
    }
}