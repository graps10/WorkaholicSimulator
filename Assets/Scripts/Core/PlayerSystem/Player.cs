using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Components.CameraSystem;
using Core.Enums;
using Core.InputSystem;
using Core.Interfaces;
using Core.ObjectPool;
using Core.SaveSystem;
using Entities;
using Entities.Constructors;
using Entities.Molds;
using Region;
using Transition;
using UnityEngine;

namespace Core.PlayerSystem
{
    public sealed class Player : MonoBehaviour
    {
        private const string PlayerEntityPosition = "PlayerEntityPosition";
        
        public static Player Instance { get; private set; }
        
        private static bool isPlayerLoaded;
        
        [SerializeField] private Mold playerMold;
        
        public PlayerEntity PlayerEntityGameObject
        {
            get
            {
                if (_playerEntityGameObject == null)
                    TryFindPlayerEntity();

                return _playerEntityGameObject;
            }
            private set => _playerEntityGameObject = value;
        }
        
        public bool EntityGameObjectIsNull => _playerEntityGameObject == null;
        
        public event Action OnUpdateEvent, OnFixedUpdateEvent, OnLateUpdateEvent;
        
        private PlayerEntity _playerEntityGameObject;
        
        private readonly List<IUpdatable> _updatableList = new();
        private readonly List<IFixedUpdatable> _fixedUpdatableList = new();
        private readonly List<ILateUpdatable> _lateUpdatableList = new();
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (Instance == null || EntityGameObjectIsNull) return;
            
            var style = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.yellow
                }
            };

            var r = RegionManager.CurrentRegion;
            var s = RegionManager.CurrentSectors;
            var l = RegionManager.CurrentLocations;
            
            var dist = -1f;
            if (s != null && s.Count > 0 && s[0] != null)
            {
                dist = Vector3.Distance(
                    PlayerEntityGameObject.transform.position, 
                    s[0].transform.position
                );
            }
            
            var regionName = r != null ? r.name : "NULL";
            var sectorInfo = s != null && s.Count > 0 ? $"{s[0].name}" : "NULL";
            var locNames = l != null ? string.Join(", ", l.Select(x => x.name)) : "None";
            
            GUILayout.BeginArea(new Rect(20, 20, 600, 200));
            GUILayout.Box("--- REGION SYSTEM DEBUG ---");
            GUILayout.Label($"Region: {regionName}", style);
            GUILayout.Label($"Sector: {sectorInfo}", style);
            GUILayout.Label($"Locations: {locNames}", style);
            
            style.normal.textColor = dist > 60f ? Color.red : Color.green; 
            GUILayout.Label($"Distance to Sector Center: {dist:F2}m", style);
            
            GUILayout.EndArea();
        }
#endif

        private void Awake()
        {
            if (!isPlayerLoaded)
            {
                SceneManager.LoadScene((int)UnityScenes.mainMenu, TransitionManager.LoadMode.None);
                isPlayerLoaded = true;
                return;
            }
            
            if (!InitializeGameObject())
                return;

            CameraManager.SpawnCamera();
            TransitionManager.Initialize();

            SceneManager.OnBeforeNewSceneLoaded_ActionList += RemoveAllRegions;
            SceneManager.AlwaysOnBeforeNewSceneLoaded_ActionList += VisibleEntitiesManager.ClearMovingEntitiesDictionary;
            SceneManager.AlwaysOnAfterNewSceneLoaded_ActionList += CameraManager.SpawnCamera;
            SceneManager.OnAfterEnterAnimationEnded_ActionList += LoadOnPlayerPosition;

            InputManager.Initialize();

            OnUpdateEvent += RegionManager.UpdatePlayerPositionWithRepositionDelay;
            OnUpdateEvent += RegionCoordinator.FindCurrentPlayerLocation;
        }
        
        private void OnApplicationQuit()
        {
            if (this != null)
            {
                StopAllCoroutines();
                
                _updatableList.Clear();
                _fixedUpdatableList.Clear();
                _lateUpdatableList.Clear();
                
                SceneManager.UninitializeCameraAndCanvas();
                SceneManager.UnsubscribeAsyncEntityConstructor();
            }
        }
        
        private bool InitializeGameObject()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                ObjectPooler.ClearPooler();
                return false;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }
        
        [ContextMenu("Save Progress")]
        private void SaveProgress() => SaveManager.SaveProgress();
        
        public void ApplyLoadedProgress()
        {
            SaveManager.LoadProgress();

            if (EntityGameObjectIsNull)
                Debug.LogWarning("PlayerEntityGameObject is null, delaying ApplyLoadedProgress until entity is created.");
        }
        
        [ContextMenu("Try find player")]
        private void TryFindPlayerEntity()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.IsChangingPlaymode)
                return;

            var playerPosition = GameObject.Find(PlayerEntityPosition);
            if (playerPosition == null)
            {
                Debug.LogError("PlayerEntityPosition not found in scene!");
                return;
            }

            EntityConstructor.Instance.LoadImmediately(playerMold, playerPosition.transform, out _playerEntityGameObject);
            ApplyLoadedProgress();
        }
        
        private void LoadOnPlayerPosition() => StartCoroutine(WaitForPlayerAndLoadRegion());

        private IEnumerator WaitForPlayerAndLoadRegion()
        {
            yield return new WaitUntil(() => !EntityGameObjectIsNull);
            RegionManager.LoadLocationOnPosition(PlayerEntityGameObject.transform.position);
        }
        
        private static void RemoveAllRegions() => RegionManager.Regions.Clear();

        #region Update FixedUpdate LateUpdate

        private void Update()
        {
            OnUpdateEvent?.Invoke();
            
            foreach (var updatable in _updatableList)
                updatable.OnUpdate();
        }

        private void FixedUpdate()
        {
            OnFixedUpdateEvent?.Invoke();
            
            foreach (var fixedUpdatable in _fixedUpdatableList)
                fixedUpdatable.OnFixedUpdate();
        }

        private void LateUpdate()
        {
            OnLateUpdateEvent?.Invoke();
            
            foreach (var lateUpdatable in _lateUpdatableList)
                lateUpdatable.OnLateUpdate();
        }

        public void RegisterUpdatable(IUpdatable updatable)
        {
            if (!_updatableList.Contains(updatable))
                _updatableList.Add(updatable);
        }
        
        public void RegisterFixedUpdatable(IFixedUpdatable fixedUpdatable)
        {
            if (!_fixedUpdatableList.Contains(fixedUpdatable))
                _fixedUpdatableList.Add(fixedUpdatable);
        }
        
        public void RegisterLateUpdatable(ILateUpdatable lateUpdatable)
        {
            if (!_lateUpdatableList.Contains(lateUpdatable))
                _lateUpdatableList.Add(lateUpdatable);
        }

        public void UnregisterUpdatable(IUpdatable updatable) 
            => _updatableList.Remove(updatable);
        
        public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable) 
            => _fixedUpdatableList.Remove(fixedUpdatable);

        public void UnregisterLateUpdatable(ILateUpdatable lateUpdatable) 
            => _lateUpdatableList.Remove(lateUpdatable);
        
        #endregion
    }
}
