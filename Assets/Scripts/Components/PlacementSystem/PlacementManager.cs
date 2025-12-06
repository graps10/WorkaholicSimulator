using System;
using Core.InputSystem;
using Core.Interfaces;
using Core.PlayerSystem;
using Entities;
using Entities.Constructors;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;

namespace Components.PlacementSystem
{
    [RequireComponent(typeof(PlacementInventoryHandler))]
    [RequireComponent(typeof(ObjectRotator))]
    public class PlacementManager : MonoBehaviour, IUpdatable, IFixedUpdatable
    {
        #region Constants
        
        private const string Ghost_Placement_Layer_Name = "GhostPlacement";
        private const string Not_Holding_Ignore_Criteria = "NotHoldingItem";
        
        #endregion
        
        #region Singleton
        
        public static PlacementManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _objectRotator = GetComponent<ObjectRotator>();
        }
        
        #endregion
        
        #region Editor Settings
        
        [Header("Settings")]
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private LayerMask ghostLayer;
        [SerializeField] private Material initialMaterial;
        [SerializeField] private Material ghostMaterial;
        
        [Header("Movement Settings")]
        [SerializeField] private float maxReachDistance = 10f;
        [SerializeField] private float minDetectionDistance = 2f;
        [SerializeField] private float yHeightClampMin = -5f;
        [SerializeField] private float yHeightClampMax = 5f;

        #endregion
        
        #region Internal State
        
        // Current Object References
        private ObjectRotator _objectRotator;
        private RigidbodyEntity _currentGhostEntity;
        private Rigidbody _currentRigidbody;
        private Renderer[] _currentObjectRenderers;
        private FurnitureMold _currentMold;
        
        // USS References
        private MouseGrabbable _currentGrabbable;
        private PlaceableItem _currentPlaceableItem;
        
        // Logic Flags & Cache
        private bool _isPlacingMode;
        private bool _isRotationMode;
        
        private Vector3 _targetPosition;
        private int _originalLayer;
        
        #endregion

        #region  Events
        
        public event Action<FurnitureMold> OnPlacementSuccess;
        public event Action<FurnitureMold> OnPlacementCancelled;
        public event Action<FurnitureMold> OnFurnitureRemoved;
        public event Action OnExitedRotationMode;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnEnable()
        {
            if (Player.Instance != null)
            {
                Player.Instance.RegisterUpdatable(this);
                Player.Instance.RegisterFixedUpdatable(this);
            }
            
            InputManager.OnLMBPerformed += HandleLMB;
            InputManager.OnRMBPerformed += HandleRMB;
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
            {
                Player.Instance.UnregisterUpdatable(this);
                Player.Instance.UnregisterFixedUpdatable(this);
            }
            
            InputManager.OnLMBPerformed -= HandleLMB;
            InputManager.OnRMBPerformed -= HandleRMB;
        }

        #endregion
        
        #region Update Loops (Interfaces)
        
        public void OnUpdate()
        {
            if (_isPlacingMode && _currentGhostEntity != null && !_isRotationMode)
                CalculateTargetPosition();
        }
        
        public void OnFixedUpdate()
        {
            if (_isPlacingMode && _currentGhostEntity != null && !_isRotationMode)
                ApplyGhostMovement();
        }
        
        #endregion
        
        #region Public API (Interaction Entry Points)
        
        public void StartPlacement(FurnitureMold mold)
        {
            if (mold == null) return;
            _currentMold = mold;
            
            EntityConstructor.Instance.LoadImmediately(mold, transform, out _currentGhostEntity);
            if (_currentGhostEntity == null) return;

            // Cache Components
            _currentRigidbody = _currentGhostEntity.GetRigidbody();
            _currentObjectRenderers = _currentGhostEntity.GetRenderers();
            _currentPlaceableItem = _currentGhostEntity.GetComponent<PlaceableItem>();
            _currentGrabbable = _currentGhostEntity.GetComponent<MouseGrabbable>();

            if (_currentPlaceableItem == null || _currentGrabbable == null)
            {
                Debug.LogError("Prefab missing PlaceableItem or MouseGrabbable component!");
                CancelPlacement();
                return;
            }
            
            _currentPlaceableItem.OnPlaced += OnItemSuccessfullyPlaced;
            
            _originalLayer = _currentGhostEntity.gameObject.layer;
            SetLayerRecursively(_currentGhostEntity.gameObject, LayerMask.NameToLayer(Ghost_Placement_Layer_Name));
            SetGhostMaterial(true);
            
            _currentGrabbable.Grab(); 
            
            _isPlacingMode = true;
            ApartmentController.Instance.SetDecorationMode(true);
        }
        
        public void RemoveFromSocket(PlaceableItem item, FurnitureMold mold)
        {
            if (item == null) return;
            var socket = item.ClosestSocket;
            if (socket == null) return;
            
            var entity = item.GetComponent<RigidbodyEntity>();
            if (entity == null) return;
            
            entity.SwitchGraphics(false);
            entity.SwitchColliders(false);
            
            if(item.ClosestSocket.PlacedItem != null)
                item.RemoveFromSocket();
            
            Core.Utilities.UtilsProvider.WaitAndRun(() => 
            {
                if(entity != null)
                    entity.ReturnToPool();
            }, true);
            
            OnFurnitureRemoved?.Invoke(mold);
            //Debug.Log($"Removed {mold.name} and returned to inventory.");
        }

        public void StartRotationMode(PlaceableItem item)
        {
            if (item == null) return;
            
            _isRotationMode = true;
            _objectRotator.BeginRotation(item.transform);
            
            Player.Instance.InputHandler.SetInputActive(false);
            //Debug.Log("Rotation Mode Started. Move mouse left/right.");
        }
        
        #endregion
        
        #region Core Placement Logic (Movement & Physics)
        
        private void CalculateTargetPosition()
        {
            if (Camera.main == null) return;
            
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance, placementLayer))
            {
                if (hit.distance < minDetectionDistance)
                    _targetPosition = ray.GetPoint(minDetectionDistance);
                else
                    _targetPosition = hit.point;
            }
            else
            {
                Vector3 defaultPos = ray.GetPoint(maxReachDistance);
                
                float clampedY = Mathf.Clamp(defaultPos.y, 
                    transform.position.y + yHeightClampMin, 
                    transform.position.y + yHeightClampMax);
                
                _targetPosition = new Vector3(defaultPos.x, clampedY, defaultPos.z);
            }
        }
        
        private void ApplyGhostMovement()
        {
            if (_currentRigidbody)
                _currentRigidbody.MovePosition(_targetPosition);
            else
                _currentGhostEntity.transform.position = _targetPosition;
        }
        
        #endregion
        
        #region Input Handlers (Placement)
        
        private void HandleLMB()
        {
            if (_isRotationMode)
            {
                ConfirmRotation();
                return;
            }

            if (_isPlacingMode)
                HandlePlaceInput();
        }

        private void HandleRMB()
        {
            if (_isRotationMode)
            {
                CancelRotation();
                return;
            }

            if (_isPlacingMode)
                CancelPlacement();
        }

        private void HandlePlaceInput()
        {
            if (!_isPlacingMode) return;

            var targetSocket = _currentPlaceableItem.ClosestSocket;

            if (targetSocket != null)
            {
                if (targetSocket.PlacedItem != null)
                    return;
                
                _isPlacingMode = false;
                _currentGrabbable.Release();
                targetSocket.PlaceItem(_currentPlaceableItem);
            }
            /*else
                Debug.Log("Cannot place: No socket nearby!");*/
        }
        
        #endregion
        
        #region Rotation Logic

        private void ConfirmRotation()
        {
            _objectRotator.ConfirmRotation();
            ExitRotationMode();
        }

        private void CancelRotation()
        {
            _objectRotator.CancelRotation();
            ExitRotationMode();
        }

        private void ExitRotationMode()
        {
            _isRotationMode = false;
            Player.Instance.InputHandler.SetInputActive(true);
            
            OnExitedRotationMode?.Invoke();
        }
        
        #endregion
        
        #region Completion & Cleanup
        
        private void OnItemSuccessfullyPlaced(Socket socket, PlaceableItem placeableItem)
        {
            //Debug.Log($"Placed successfully {placeableItem.transform.name} in socket: {socket.name}");
            FinalizePlacementLogic(true);
        }
        
        private void FinalizePlacementLogic(bool success)
        {
            if (_currentPlaceableItem != null)
                _currentPlaceableItem.OnPlaced -= OnItemSuccessfullyPlaced;

            if (success)
                OnPlacementSuccess?.Invoke(_currentMold);
            else
            {
                RemoveFromSocket(_currentPlaceableItem, _currentMold);
                OnPlacementCancelled?.Invoke(_currentMold);
            }
            
            SetLayerRecursively(_currentGhostEntity.gameObject, _originalLayer);
            SetGhostMaterial(false);
            
            // Reset
            _currentGhostEntity = null;
            _currentRigidbody = null;
            _currentObjectRenderers = null;
            _currentPlaceableItem = null;
            _currentMold = null;
            _currentGrabbable = null;
            
            _isPlacingMode = false;
            
            ApartmentController.Instance.SetDecorationMode(false);
        }

        private void CancelPlacement() => FinalizePlacementLogic(false);

        #endregion
        
        #region Utilities (Visuals & Layers)
        
        private void SetGhostMaterial(bool isGhost)
        {
            if(_currentObjectRenderers == null) return;
            
            foreach (var r in _currentObjectRenderers)
            {
                if (r == null) continue;
                
                if (isGhost && ghostMaterial != null)
                    r.material = ghostMaterial;
                else if (!isGhost && initialMaterial != null)
                    r.material = initialMaterial;
            }
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, newLayer);
        }
        
        #endregion
    }
}