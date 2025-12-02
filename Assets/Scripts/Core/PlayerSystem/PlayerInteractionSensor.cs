using System;
using Core.Enums;
using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    public class PlayerInteractionSensor : MonoBehaviour, IUpdatable
    {
        [SerializeField] private float interactionDistance = 3.0f;
        
        private IInteractable _currentInteractable;
        
        private Camera _playerCamera;
        
        public static event Action<bool> OnInteractionAvailabilityChanged;
        public event Action<bool> OnInteractableTargetChanged;

        private void Awake() => _playerCamera = Camera.main;

        private void OnEnable()
        {
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
            
            InputManager.OnInteractPerformed += TryInteract;
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
            
            InputManager.OnInteractPerformed -= TryInteract;
            OnInteractionAvailabilityChanged?.Invoke(false);
        }

        public void OnUpdate() => CheckForInteractable();
        
        private void TryInteract()
        {
            if (_currentInteractable != null)
                _currentInteractable.Interact();
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, (int)UnityLayers.Interactable))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (_currentInteractable != interactable)
                    {
                        _currentInteractable = interactable;
                        OnInteractableTargetChanged?.Invoke(true);
                        OnInteractionAvailabilityChanged?.Invoke(true);
                        //Debug.Log($"Can Interact: {hit.collider.name}");
                    }
                }
                else
                {
                    if (_currentInteractable != null)
                        ClearInteraction();
                }
            }
            else
            {
                if (_currentInteractable != null)
                    ClearInteraction();
            }
        }
        
        private void ClearInteraction()
        {
            _currentInteractable = null;
            OnInteractableTargetChanged?.Invoke(false);
            OnInteractionAvailabilityChanged?.Invoke(false);
            //Debug.Log("Interaction Lost");
        }
    }
}