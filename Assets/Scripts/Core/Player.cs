using System;
using System.Collections.Generic;
using Components;
using Core.Enums;
using Core.Interfaces;
using Core.ObjectPool;
using Entities;
using Region;
using Transition;
using UnityEngine;

namespace Core
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
                {
                    // try to find player
                }

                return _playerEntityGameObject;
            }
            private set => _playerEntityGameObject = value;
        }
        
        public event Action OnUpdateEvent;
        public event Action OnFixedUpdateEvent;
        
        private readonly List<IUpdatable> _updatableList = new();
        private readonly List<IFixedUpdatable> _fixedUpdatableList = new();
        
        private PlayerEntity _playerEntityGameObject;

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

            InputManager.InputManager.Initialize();

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

        private void SpawnCamera() 
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
        
        private void RemoveAllRegions()
        {
            RegionManager.Regions.Clear();
        }

        private void LoadOnPlayerPosition()
        {
            if (PlayerEntityGameObject != null) 
                RegionManager.LoadLocationOnPosition(Instance.PlayerEntityGameObject.transform.position);
        }

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
