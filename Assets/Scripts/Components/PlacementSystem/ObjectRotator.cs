using Core.InputSystem;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class ObjectRotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float minInputThreshold = 0.5f;

        private Transform _targetTransform;
        private Quaternion _initialRotation;
        
        private bool _isActive;

        private void OnEnable() => InputManager.OnLookInput += HandleLookInput;

        private void OnDisable() => InputManager.OnLookInput -= HandleLookInput;

        public void BeginRotation(Transform target)
        {
            if (target == null) return;

            _targetTransform = target;
            _initialRotation = target.localRotation;
            _isActive = true;
        }

        public void ConfirmRotation() => StopRotation();

        public void CancelRotation()
        {
            if (_targetTransform != null)
                _targetTransform.localRotation = _initialRotation;

            StopRotation();
        }

        private void StopRotation()
        {
            _isActive = false;
            _targetTransform = null;
        }

        private void HandleLookInput(Vector2 delta)
        {
            if (!_isActive || _targetTransform == null) return;

            if (Mathf.Abs(delta.x) > minInputThreshold)
                _targetTransform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime);
        }
    }
}