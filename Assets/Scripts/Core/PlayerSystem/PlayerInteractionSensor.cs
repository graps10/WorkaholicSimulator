using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    public class PlayerInteractionSensor : MonoBehaviour, IUpdatable
    {
        [SerializeField] private float interactionDistance = 3.0f;
        
        private IInteractable _currentInteractable;
        
        private Camera _playerCamera;

        private void Awake() => _playerCamera = Camera.main;

        private void OnEnable()
        {
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
        }

        public void OnUpdate() => CheckForInteractable();

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
                        Debug.Log(
                            $"Looking at: {hit.collider.name} | Press E to {_currentInteractable.InteractionPrompt}");
                    }

                    return;
                }
            }
            
            if (_currentInteractable != null)
            {
                Debug.Log("Interaction Lost");
                _currentInteractable = null;
            }
        }
    }
}