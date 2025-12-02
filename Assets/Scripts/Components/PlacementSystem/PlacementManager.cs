using Core.InputSystem;
using Core.Interfaces;
using Core.ObjectPool;
using Core.PlayerSystem;
using Entities;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class PlacementManager : MonoBehaviour, IUpdatable, IFixedUpdatable
    {
        private const string Ghost_Placement_Layer_Name = "GhostPlacement";
        public static PlacementManager Instance {get; private set; }
        
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

        private RigidbodyEntity _currentGhostEntity;
        private Rigidbody _currentRigidbody;
        private Renderer[] _currentObjectRenderers;
        
        private MouseGrabbable _currentGrabbable;
        private PlaceableItem _currentPlaceableItem;
        
        private bool _isPlacingMode;
        private Vector3 _targetPosition;
        
        private int _originalLayer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

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

        public void OnUpdate()
        {
            if (!_isPlacingMode || _currentGhostEntity == null) return;

            CalculateTargetPosition();
        }
        
        public void OnFixedUpdate()
        {
            if (!_isPlacingMode || _currentGhostEntity == null) return;
            
            ApplyMovement();
        }
        
        public void StartPlacement(FurnitureMold mold)
        {
            if (mold == null) return;
            
            _currentGhostEntity = ObjectPooler.TakePooledGameObject(mold.PrefabPoolInfo, transform) as RigidbodyEntity;
            if (_currentGhostEntity == null) return;

            _currentRigidbody =_currentGhostEntity.GetRigidbody();
            _currentObjectRenderers = _currentGhostEntity.GetRenderers();
            
            _currentPlaceableItem = _currentGhostEntity.GetComponentInChildren<PlaceableItem>();
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
            EditModeController.Instance.SetEditMode(true);
        }
        
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
        
        private void HandlePlaceInput()
        {
            if (!_isPlacingMode) return;
            
            var targetSocket = _currentPlaceableItem.ClosestSocket;

            if (targetSocket != null)
            {
                if (targetSocket.CanPlace(_currentPlaceableItem))
                {
                    _isPlacingMode = false;
                    _currentGrabbable.Release();
                }
                else
                    Debug.LogWarning($"Cannot place here! Socket '{targetSocket.name}' rejected the item (Occupied?).");
            }
            else
                Debug.Log("Cannot place: No socket nearby!");
        }

        private void HandleCancelInput()
        {
            if (!_isPlacingMode) return;
            CancelPlacement();
        }

        private void OnItemSuccessfullyPlaced(Socket socket, PlaceableItem placeableItem)
        {
            Debug.Log($"Placed successfully {placeableItem.transform.parent.name} in socket: {socket.name}");
            FinalizePlacementLogic(true);
        }
        
        private void FinalizePlacementLogic(bool success)
        {
            if (_currentPlaceableItem != null)
                _currentPlaceableItem.OnPlaced -= OnItemSuccessfullyPlaced;

            if (success)
            {
                SetLayer(_currentGhostEntity.gameObject, _originalLayer);
                SetGhostMaterial(false);
            }
            else
            {
                if(_currentGhostEntity != null)
                    _currentGhostEntity.ReturnToPool();
            }
            
            _currentGhostEntity = null;
            _currentRigidbody = null;
            _currentObjectRenderers = null;
            
            _currentGrabbable = null;
            _currentPlaceableItem = null;
            
            _isPlacingMode = false;
            EditModeController.Instance.SetEditMode(false);
            
            // TODO: Remove money / Remove from inventory data logic here
        }

        private void CancelPlacement() => FinalizePlacementLogic(false);

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
    }
}