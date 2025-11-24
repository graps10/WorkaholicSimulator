using System.Collections;
using System.Collections.Generic;
using Core.Utilities;
using Region.BoundsInEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Region
{
    public class Sector : Location
    {
        [HideInInspector][SerializeField] internal UnityEvent onEnter;
        [HideInInspector][SerializeField] internal UnityEvent onExit;
        [HideInInspector] [SerializeField] private bool showSector;
        [HideInInspector] [SerializeField] protected List<Location> locations;
        
        public bool ShowSector 
        { 
            get { return showSector; } 
            set 
            { 
                showSector = value; 
#if UNITY_EDITOR
                ToggleDisplayBounds(value); 
#endif
            } 
        }

        public Region MyRegion { get; private set; }
        
        protected override string MaterialPath => "Assets/Materials/EditorLocationBounds/SectorMaterial.mat";
        
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Sector already initialized");
                return;
            }

            isInitialized = true;

            SwitchGraphics(false);

            if (locationBounds == null)
            {
                locationBounds = BoundsSceneElement.Create<SectorBounds>(this, boundsMaterial, transform);
                locationBounds.CreateMeshBounds();
            }
            
            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);

            foreach (var location in locations)
                location.Initialize(this);
        }

        public void SetRegion(Region region)
        {
            if (MyRegion == null)
                MyRegion = region;
        }
        
        public List<Location> GetLocations() => locations;
        public void AddLocation(Location location) => locations.Add(location);
        public void ClearLocations() => locations.Clear();
        public void InitializeNewLocationsList() => locations ??= new List<Location>();

        protected internal override void Load()
        {
            if (IsLoaded)
                return;

            IsLoaded = true;

            foreach (var location in locations)
                location.Load();
        }

        public override void SwitchGraphics(bool stateToSet)
        {
            base.SwitchGraphics(stateToSet);
            
            if (stateToSet) return;
            
            foreach (var location in locations)
                location.SwitchGraphics(stateToSet);
        }

        public override void Dispose()
        {
            onEnter.RemoveAllListeners();
            onExit.RemoveAllListeners();

            foreach (var location in locations)
                location.Dispose();

            base.Dispose();
        }

        public override void Enter() => onEnter?.Invoke();

        public override void Exit() => onExit?.Invoke();

        public override List<Bounds> GetAllBounds()
        {
            List <Bounds> collectedLocationBounds = new List <Bounds>(base.GetAllBounds());

            if(locations == null) return collectedLocationBounds;
            
            foreach (var item in locations)
            {
                var mesh = item.GetComponentInChildren<LocationBounds>().GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    collectedLocationBounds.Add(MeshUtils.TransformBounds(mesh.bounds, item.transform));
                    Debug.Log("location added");
                }
            }

            return collectedLocationBounds;
        }

        public override void CalculateBounds(bool displayBounds = true)
        {
            if (locations != null)
                foreach (Location location in locations)
                    location.CalculateBounds(displayBounds);
#if UNITY_EDITOR
            AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
            if (locationBounds == null)
                locationBounds = BoundsSceneElement.Create<SectorBounds>(this, boundsMaterial, transform);
            
            if (locations != null && 
                (locationBounds == null
                 || environmentParent.childCount == 0 
                 && locations.Count == 0)) return;
            
            locationBounds.CreateMeshBounds();

            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);
#if UNITY_EDITOR     
            ToggleDisplayBounds(displayBounds);
#endif
        }
        
        protected override IEnumerator DelayedHideGraphicsRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            SwitchGraphics(false);
            Dispose();
        }
        
#if UNITY_EDITOR
        protected override void ReloadForEditor()
        {
            CreateEnvironmentParent();
            InitializeNewLocationsList();
            CalculateBounds();
        }
#endif
    }
}