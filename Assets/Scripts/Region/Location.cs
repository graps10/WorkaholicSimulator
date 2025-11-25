using System;
using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.SaveSystem;
using Core.Utilities;
using Entities;
using Entities.Constructors;
using Region.BoundsInEditor;
using UnityEngine;

namespace Region
{
    public class Location : MonoBehaviour, IDisposable
    {
        private const int Location_Path_Depth = 2;
        
        private const string Entities_Parent_Name = "Entities";
        private const string Environment_Parent_Name = "Environment";
        
        /// Serialized in editor
        [SerializeField] protected Transform environmentParent;
        
        [SerializeField] private List<EntitySpawnPreset> entityPresets = new();
        [SerializeField] protected Material boundsMaterial;
        [SerializeField] protected BoundsSceneElement locationBounds;
        [SerializeField] private Color boundsColor = new(0.5f, 0.5f, 0.5f, 0.5f);
        
        private readonly List<EntitySpawnPreset> _lastEntityPresets = new();
        private Transform _lastEnvironment;

        /// Bounds of the location. Defines both size and position
        [HideInInspector] [SerializeField] private bool showLocation;
        internal Bounds Bounds { get; set; }

        internal Dictionary<Transform, Entity> _entitiesDictionary = new();
        internal Dictionary<string, Pose> _savedEntityPoses;
        
        internal string _locationPath;
        
        public Sector MySector { get; private set; }
        public bool IsLoaded { get; protected set; }
        public bool IsWaitingToHide => delayedHidingCoroutine != null;
        
        public bool ShowLocation
        {
            get => showLocation;
            set
            {
                showLocation = value;
#if UNITY_EDITOR
                ToggleDisplayBounds(value);
#endif
            }
        }
        
        protected Coroutine delayedHidingCoroutine;
        
        protected bool isInitialized;
        
        private List<Renderer> _renderersList = new();
        
        private Color _lastBoundsColor;
        private Material _lastMaterial;
        
        #region events

        protected internal event Action OnLoad;
        protected internal event Action OnUnload;
        protected internal event Action<Location> OnLocationDestroy;

        protected internal event Action OnEnter;
        protected internal event Action OnExit;

        internal event Action<bool> OnSwitchLogic;
        internal event Action<bool> OnSwitchGraphics;

        #endregion
        
        private void OnEnable() => CopyLastEntitiesTransformsAndMolds();

        private void OnValidate()
        {
            if (environmentParent == null) return;
            
            if (_lastEnvironment != environmentParent)
                FindAllEnvironmentMeshes();
        }

        private void FindAllEnvironmentMeshes()
        {
            if (environmentParent == null) return;

            foreach (var item in environmentParent.GetComponentsInChildren<Renderer>())
            {
                if (!_renderersList.Contains(item))
                    _renderersList.Add(item);
            }
        }
        
        private void CopyLastEntitiesTransformsAndMolds()
        {
            _lastEntityPresets.Clear();
            
            foreach (var item in entityPresets)
            {
                var copiedEntityPreset = new EntitySpawnPreset
                {
                    mold = item.mold,
                    transforms = new Transform[item.transforms.Length]
                };

                for (int i = 0; i < item.transforms.Length; i++)
                    copiedEntityPreset.transforms[i] = item.transforms[i];

                _lastEntityPresets.Add(copiedEntityPreset);
            }
        }

        protected virtual string MaterialPath => "Assets/Materials/EditorLocationBounds/LocationMaterial.mat";

        public void Initialize(Sector sector)
        {
            if (isInitialized)
            {
                Debug.LogWarning("Location already initialized");
                return;
            }
            
            isInitialized = true;

            Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);

            SwitchGraphics(false);
            SwitchLogic(false);

            MySector = sector;
            _locationPath = UtilsProvider.GetGameObjectPath(gameObject, Location_Path_Depth);

            SaveManager.Progress.TryGetLocationObjectPoses(_locationPath, out _savedEntityPoses);
        }

#if UNITY_EDITOR
        
        private void Reset()
        {
            ReloadForEditor();
        }
        
        protected virtual void ReloadForEditor()
        {
            CreateEntitiesParent();
            CreateEnvironmentParent();
            CalculateBounds();
        }

        private void CreateEntitiesParent()
        {
            foreach (var item in GetComponentsInChildren<Transform>())
                if (item.name == Entities_Parent_Name)
                    return;

            GameObject entities = new GameObject(Entities_Parent_Name);
            entities.transform.SetParent(transform);
        }

        protected void CreateEnvironmentParent()
        {
            foreach (var item in GetComponentsInChildren<Transform>())
                if (item.name == Environment_Parent_Name)
                    return;

            GameObject environment = new GameObject(Environment_Parent_Name);
            environment.transform.SetParent(transform);
            environmentParent = environment.transform;
        }

        public void RefreshEditorEntities()
        {
            bool isNeedRefresh = false;

            if (_lastEntityPresets.Count != entityPresets.Count)
                isNeedRefresh = true;
            else
                for (int i = 0; i < _lastEntityPresets.Count; i++)
                {
                    if (_lastEntityPresets[i].Equals(entityPresets[i]))
                        continue;

                    isNeedRefresh = true;
                    break;
                }

            if (!isNeedRefresh)
                return;

            EditorEntityConstructor.Instance.RefreshLocation(this, entityPresets);

            CopyLastEntitiesTransformsAndMolds();
        }

        public void RefreshBoundsColor()
        {
            if (_lastBoundsColor == boundsColor)
                return;
            
            _lastBoundsColor = boundsColor;

            if (locationBounds == null || boundsMaterial == null)
                return;

            //We cannot change LocationBounds.MyMeshRenderer.material.color in EditorMode. Need to create new Material;
            Material material = new Material(boundsMaterial);
            material.color = boundsColor;

            if (_lastMaterial != null) DestroyImmediate(_lastMaterial);
            _lastMaterial = material;

            locationBounds.GetMeshRenderer().material = material;
            ToggleDisplayBounds(true);
        }
#endif

        public virtual void Enter() // Called when we enter a location type bounds
        {
            if (!IsLoaded)
                Load();

            OnEnter?.Invoke();
        }

        public virtual void Exit() // Called when we exit any location type bounds
        {
            OnExit?.Invoke();
        }

        #region Load or Unload

        protected internal virtual void Load() // Load location assets
        {
            if (IsLoaded) return;
            
            IsLoaded = true;
            LoadEntitiesFromPresets();
            OnLoad?.Invoke();
        }
        
        public virtual void Dispose() // Unload location assets
        {
            if (!IsLoaded) 
                return;
            
            var entitiesToUnload = new LocationEntitiesList();
            
            foreach (var (spawnPoint, entity) in _entitiesDictionary)
            {
                if (entity == null) continue;

                entitiesToUnload.Add(entity);
                
                SaveObjectPose(spawnPoint.name, entity.transform.GetPose());
                Debug.Log($"Saved pos for {entity.name} at {spawnPoint.name}");
            }
            
            this.UnloadEntities(entitiesToUnload);
            
            _entitiesDictionary.Clear();
            
            RemoveSectorSwitchLogicEvent(SwitchLogic);
            CancelDelayedHiding();

            OnUnload?.Invoke();

            IsLoaded = false;
        }

        public void SwitchLogic(bool enable) => OnSwitchLogic?.Invoke(enable);

        private void OnDestroy() => OnLocationDestroy?.Invoke(this);
        
        public void HideGraphicsWithDelay(float delay)
        {
            if (this == null) 
                return;
            
            if (IsWaitingToHide)
                CancelDelayedHiding();

            delayedHidingCoroutine = StartCoroutine(DelayedHideGraphicsRoutine(delay));
        }
        
        public void CancelDelayedHiding()
        {
            if (!IsWaitingToHide || this == null)
                return;
            
            StopCoroutine(delayedHidingCoroutine);
            delayedHidingCoroutine = null;
        }

        protected virtual IEnumerator DelayedHideGraphicsRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (this != null)
            {
                SwitchGraphics(false);
                Dispose();
            }
        }

        #endregion

        #region Control location content

        public virtual void SwitchGraphics(bool stateToSet)
        {
            foreach (var rendererFromList in _renderersList)
                rendererFromList.enabled = stateToSet;

            OnSwitchGraphics?.Invoke(stateToSet);
        }
        
        private void LoadEntitiesFromPresets()
        {
            foreach (var entityPreset in entityPresets)
            {
                entityPreset.SetSpawnOptions();
                this.CreateEntitiesAsync(entityPreset);
            }
            
            AddSectorSwitchLogicEvent(SwitchLogic);
        }

        private void AddSectorSwitchLogicEvent(Action<bool> action)
            => MySector.OnSwitchLogic += action;

        private void RemoveSectorSwitchLogicEvent(Action<bool> action)
        {
            if (MySector == null)
                return;
            
            MySector.OnSwitchLogic -= action;
        }

        private void SaveObjectPose(string objectName, Pose pose)
        {
            if (_savedEntityPoses == null)
                SaveManager.Progress.AddLocationObjectPoses(_locationPath, out _savedEntityPoses);

            _savedEntityPoses[objectName] = pose;
        }

        #endregion

        #region  Bounds And Gizmos

        public bool IsInsideBounds(Vector3 position) => Bounds.Contains(position);

        public virtual void CalculateBounds(bool displayBounds = true)
        {
            if (locationBounds == null)
            {
#if UNITY_EDITOR
                AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
                Material material = new Material(boundsMaterial);
                material.color = boundsColor;
                
                locationBounds = BoundsSceneElement.Create<LocationBounds>(this, material, transform);
            }

            if (locationBounds != null && environmentParent.childCount != 0)
            {
                locationBounds.CreateMeshBounds();
                Bounds = MeshUtils.TransformBounds(locationBounds.GetMeshFilter().sharedMesh.bounds, transform);
            }

#if UNITY_EDITOR
            ToggleDisplayBounds(displayBounds);
#endif
        }

#if UNITY_EDITOR
        public void ToggleDisplayBounds(bool enable) => locationBounds.SwitchVisible(enable);
#endif

        public virtual List<Bounds> GetAllBounds()
        {
            List<Bounds> locationBounds = new();

            List<Transform> environmentTransforms = new List<Transform>();

            if (environmentParent != null) environmentTransforms.AddRange(environmentParent.GetComponentsInChildren<Transform>());
            entityPresets.ForEach(x => environmentTransforms.AddRange(x.transforms));

            foreach (Transform item in environmentTransforms)
            {
                if (item == null) continue;
                if (item == environmentParent && environmentTransforms.Count != 1) continue;

                var bounds = BoundsUtils.GetValidBounds(item.gameObject);
                if (bounds != default) locationBounds.Add(bounds);
            }

#if UNITY_EDITOR
            foreach (var gameObject in locationBounds)
                BoundsUtils.DrawBounds(gameObject, 2);
#endif
            return locationBounds;
        }

        #endregion
        
        public Color GetBoundsColor() => boundsColor;
        
        public void ClearOnSwitchLogicEvent() => OnSwitchLogic = null;
    }
}
