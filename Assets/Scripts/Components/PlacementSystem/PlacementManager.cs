using Core.InputSystem;
using Core.Interfaces;
using Core.PlayerSystem;
using Core.SaveSystem;
using Entities;
using Entities.Constructors;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Components.PlacementSystem
{
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
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 10f;

        #endregion
        
        #region Internal State
        
        // Current Object References
        private RigidbodyEntity _currentGhostEntity;
        private Rigidbody _currentRigidbody;
        private Renderer[] _currentObjectRenderers;
        private FurnitureMold _currentMold;
        
        // USS References
        private MouseGrabbable _currentGrabbable;
        private PlaceableItem _currentPlaceableItem;
        
        // Rotation Mode State
        private PlaceableItem _itemToRotate;
        private Quaternion _initialRotation;
        
        // Logic Flags & Cache
        private bool _isPlacingMode;
        private bool _isRotationMode;
        private Vector3 _targetPosition;
        private int _originalLayer;
        
        #endregion

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            if (Player.Instance != null)
            {
                Player.Instance.RegisterUpdatable(this);
                Player.Instance.RegisterFixedUpdatable(this);
            }
            
            InputManager.OnLMBPerformed += HandlePlaceInput;
            InputManager.OnRMBPerformed += HandleCancelInput;
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
            {
                Player.Instance.UnregisterUpdatable(this);
                Player.Instance.UnregisterFixedUpdatable(this);
            }
            
            InputManager.OnLMBPerformed -= HandlePlaceInput;
            InputManager.OnRMBPerformed -= HandleCancelInput;
        }

        #endregion
        
        #region Update Loops (Interfaces)
        
        public void OnUpdate()
        {
            if (_isRotationMode)
            {
                HandleRotationLogic();
                return;
            }
            
            if (!_isPlacingMode || _currentGhostEntity == null) return;

            CalculateTargetPosition();
        }
        
        public void OnFixedUpdate()
        {
            if (!_isPlacingMode || _currentGhostEntity == null) return;
            
            ApplyMovement();
        }
        
        #endregion
        
        #region Public API (Interaction Entry Points)
        
        public void StartPlacement(FurnitureMold mold)
        {
            if (mold == null) return;

            if (SaveManager.Progress.Inventory.GetItemCount(mold.ID) <= 0)
            {
                Debug.LogError("Not enough items!");
                return;
            }
            
            _currentMold = mold;
            
            EntityConstructor.Instance.LoadImmediately(mold, transform, out _currentGhostEntity);
            if (_currentGhostEntity == null) return;

            _currentRigidbody =_currentGhostEntity.GetRigidbody();
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
            SetLayer(_currentGhostEntity.gameObject, LayerMask.NameToLayer(Ghost_Placement_Layer_Name));
            SetGhostMaterial(true);
            
            _currentGrabbable.Grab(); 
            
            _isPlacingMode = true;
            ApartmentController.Instance.SetDecorationMode(true);
        }
        
        public void RemoveFromSocket(PlaceableItem item, FurnitureMold mold)
        {
            if (item == null) return;
            
            var entity = item.GetComponent<RigidbodyEntity>();
            if (entity == null) return;
            
            entity.SwitchGraphics(false);
            entity.SwitchColliders(false);
                
            Core.Utilities.UtilsProvider.WaitAndRun(() => 
            {
                if(item.ClosestSocket.PlacedItem != null)
                    item.RemoveFromSocket();
                    
                entity.ReturnToPool();
            }, true);
            
            Debug.Log($"Removed {mold.name} and returned to inventory.");
        }

        public void StartRotationMode(PlaceableItem item)
        {
            if (item == null) return;

            _itemToRotate = item;
            _initialRotation = item.transform.rotation;
            _isRotationMode = true;
            
            Player.Instance.InputHandler.SetInputActive(false);
            
            Debug.Log("Rotation Mode Started. Move mouse left/right.");
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
        
        private void ApplyMovement()
        {
            if (_currentRigidbody)
                _currentRigidbody.MovePosition(_targetPosition);
            else
                _currentGhostEntity.transform.position = _targetPosition;
        }
        
        #endregion
        
        #region Input Handlers (Placement)

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
            }
            else
                Debug.Log("Cannot place: No socket nearby!");
        }

        private void HandleCancelInput()
        {
            if (!_isPlacingMode) return;
            CancelPlacement();
        }
        
        #endregion
        
        #region Rotation Logic /// Temporary

        private float GetRotationInput() => Mouse.current.delta.x.ReadValue(); 
        
        private void HandleRotationLogic()
        {
            float mouseX = GetRotationInput();

            const float mouseThreshold = 0.1f;
            if (Mathf.Abs(mouseX) > mouseThreshold)
                _itemToRotate.transform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime);
            
            if (Mouse.current.leftButton.wasPressedThisFrame)
                ConfirmRotation();
            
            if (Mouse.current.rightButton.wasPressedThisFrame)
                CancelRotation();
        }

        private void ConfirmRotation()
        {
            ExitRotationMode();
            ApartmentController.Instance.RequestSave();
            Debug.Log("Rotation Confirmed.");
        }

        private void CancelRotation()
        {
            if (_itemToRotate != null)
                _itemToRotate.transform.rotation = _initialRotation;
            
            ExitRotationMode();
            Debug.Log("Rotation Cancelled.");
        }

        private void ExitRotationMode()
        {
            _isRotationMode = false;
            _itemToRotate = null;
            
            Player.Instance.InputHandler.SetInputActive(true);
        }
        
        #endregion
        
        #region Completion & Cleanup
        
        private void OnItemSuccessfullyPlaced(Socket socket, PlaceableItem placeableItem)
        {
            Debug.Log($"Placed successfully {placeableItem.transform.name} in socket: {socket.name}");
            FinalizePlacementLogic(true);
        }
        
        private void FinalizePlacementLogic(bool success)
        {
            if (_currentPlaceableItem != null)
                _currentPlaceableItem.OnPlaced -= OnItemSuccessfullyPlaced;

            if (success)
            {
                //SaveManager.Progress.Inventory.TryRemoveItem(_currentMold.ID);
            }
            else
            {
                //SaveManager.Progress.Inventory.AddItem(mold.ID);
                RemoveFromSocket(_currentPlaceableItem, _currentMold);
            }
            
            SetLayer(_currentGhostEntity.gameObject, _originalLayer);
            SetGhostMaterial(false);
            
            _currentGhostEntity = null;
            _currentRigidbody = null;
            _currentObjectRenderers = null;
            _currentPlaceableItem = null;
            
            _currentMold = null;
            _currentGrabbable = null;
            
            _isPlacingMode = false;
            
            ApartmentController.Instance.SetDecorationMode(false);
            ApartmentController.Instance.RequestSave();
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
        
        private void SetLayer(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
        }
        
        #endregion
    }
}