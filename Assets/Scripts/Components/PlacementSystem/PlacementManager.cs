using Core.Utilities;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class PlacementManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private float reachDistance = 10f;

        private GameObject _currentGhostObject;
        private MouseGrabbable _currentGrabbable;
        private PlaceableItem _currentPlaceableItem;
        
        private bool _isPlacingMode;

        private void Update()
        {
            if (!_isPlacingMode || _currentGhostObject == null) return;

            HandleGhostMovement();
            HandleInput();
        }
        
        public void StartPlacement(FurnitureMold mold)
        {
            if (mold == null) return;
            
            AssetUtils.TryLoadAsset(mold.PrefabPoolInfo.ObjectPath, out _currentGhostObject);
            
            _currentPlaceableItem = _currentGhostObject.GetComponent<PlaceableItem>();
            _currentGrabbable = _currentGhostObject.GetComponent<MouseGrabbable>();

            if (_currentPlaceableItem == null || _currentGrabbable == null)
            {
                Debug.LogError("Prefab missing PlaceableItem or MouseGrabbable component!");
                CancelPlacement();
                return;
            }
            
            SetGhostMaterial(_currentGhostObject, true);
            
            _currentGrabbable.Grab(); 
            
            EditModeController.Instance.SetEditMode(true);
            
            _isPlacingMode = true;
        }
        
        private void HandleGhostMovement()
        {
            if (Camera.main == null) return;
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, placementLayer))
            {
                var rb = _currentGhostObject.GetComponent<Rigidbody>();
                if (rb)
                    rb.MovePosition(hit.point);
                else
                    _currentGhostObject.transform.position = hit.point;
            }
        }
        
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
                TryPlaceItem();
            
            if (Input.GetKeyDown(KeyCode.Escape))
                CancelPlacement();
        }

        private void TryPlaceItem()
        {
            _currentGrabbable.Release();
            
            if (_currentPlaceableItem.Placed)
                FinalizePlacement();
            else
                Debug.Log("Not in socket range!");
        }

        private void FinalizePlacement()
        {
            SetGhostMaterial(_currentGhostObject, false);
            
            _currentGhostObject = null;
            _currentGrabbable = null;
            _isPlacingMode = false;
            
            EditModeController.Instance.SetEditMode(false);
            
            // Remove an item from inventory, spend money
        }

        private void CancelPlacement()
        {
            if (_currentGhostObject != null) Destroy(_currentGhostObject);
            _isPlacingMode = false;
            EditModeController.Instance.SetEditMode(false);
        }

        private void SetGhostMaterial(GameObject obj, bool isGhost)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (isGhost)
                    r.material = ghostMaterial;
                else
                    r.material = r.sharedMaterial;
            }
        }
    }
}