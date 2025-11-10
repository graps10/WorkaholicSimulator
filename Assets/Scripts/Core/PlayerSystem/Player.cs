using System;
using System.Collections.Generic;
using Components;
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
        public static Player Instance { get; private set; }
        
        private static bool isPlayerLoaded;
        
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
        
        public event Action OnUpdateEvent;
        public event Action OnFixedUpdateEvent;
        
        [SerializeField] private Mold playerMold;
        private PlayerEntity _playerEntityGameObject;
        
        private readonly List<IUpdatable> _updatableList = new();
        private readonly List<IFixedUpdatable> _fixedUpdatableList = new();

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

            SpawnCamera();
            TransitionManager.Initialize();

            SceneManager.OnBeforeNewSceneLoaded_ActionList += RemoveAllRegions;
            SceneManager.AlwaysOnBeforeNewSceneLoaded_ActionList += VisibleEntitiesManager.ClearMovingEntitiesDictionary;
            SceneManager.AlwaysOnAfterNewSceneLoaded_ActionList += SpawnCamera;
            SceneManager.OnAfterEnterAnimationEnded_ActionList += LoadOnPlayerPosition;

            InputManager.Initialize();

            OnUpdateEvent += RegionCoordinator.FindCurrentPlayerLocation;
            OnUpdateEvent += RegionManager.UpdatePlayerPositionWithRepositionDelay;
        }
        
        private void OnApplicationQuit()
        {
            if (this != null)
            {
                StopAllCoroutines();
                
                _updatableList.Clear();
                _fixedUpdatableList.Clear();
                
                SceneManager.UninitializeCameraAndCanvas();
                SceneManager.UnsubscribeAsyncEntityConstructor();
            }
        }

        private static void SpawnCamera() 
            => CameraManager.SetCameraBySceneIndex(GameObject.Find("Cameras").transform);
        
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
        
        public void ApplyLoadedProgress()
        {
            SaveManager.LoadProgress();

            if (EntityGameObjectIsNull)
            {
                Debug.LogWarning("PlayerEntityGameObject is null, delaying ApplyLoadedProgress until entity is created.");
                return; 
            }

            var progress = SaveManager.Progress;
            var playerEntity = PlayerEntityGameObject.GetComponent<PlayerEntity>();
            /*if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Progress upgrades before applying: {JsonUtility.ToJson(progress.UpgradeData.UpgradeLevels)}");
            
            playerEntity.GetLevelUpgrades().
                Copy(progress.UpgradeData.UpgradeLevels.ToUpgradeLevelContainer());
            
            playerEntity.UpdateUpgrades();
            if (SaveManager.EnableSaveLoadDebugLogs) 
                Debug.Log($"PlayerActor upgrades after applying: {JsonUtility.ToJson(playerEntity.GetLevelUpgrades())}");*/
        }
        
        [ContextMenu("Try find player")]
        private void TryFindPlayerEntity()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.IsChangingPlaymode)
                return;

            var playerPosition = GameObject.Find("PlayerEntityPosition");
            if (playerPosition == null)
            {
                Debug.LogError("PlayerEntityPosition not found in scene!");
                return;
            }

            EntityConstructor.Instance.LoadImmediately(playerMold, playerPosition.transform, out _playerEntityGameObject);
            ApplyLoadedProgress();
        }
        
        private void LoadOnPlayerPosition()
        {
            if (!EntityGameObjectIsNull) 
                RegionManager.LoadLocationOnPosition(Instance.PlayerEntityGameObject.transform.position);
        }
        
        private static void RemoveAllRegions() => RegionManager.Regions.Clear();

        #region Update and FixedUpdate

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

        public void UnregisterUpdatable(IUpdatable updatable) 
            => _updatableList.Remove(updatable);
        
        public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable) 
            => _fixedUpdatableList.Remove(fixedUpdatable);

        
        #endregion
    }
}
