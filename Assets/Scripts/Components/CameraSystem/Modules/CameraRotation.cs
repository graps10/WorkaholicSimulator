using System;
using UnityEngine;

namespace Components.CameraSystem.Modules
{
    [Serializable]
    public class CameraRotationSettings : CameraSettings
    {
        [Tooltip("Initial camera tilt angle")]
        public Vector3 InitialTiltAngle;
        
        [Header("Control Settings")]
        [Tooltip("Mouse Sensitivity: X = Horizontal (Yaw), Y = Vertical (Pitch)")]
        public Vector2 Sensitivity = new(0.1f, 0.1f);

        [Tooltip("Limits for vertical rotation (X axis). X = Min (Up), Y = Max (Down)")]
        public Vector2 PitchLimits = new(-65f, 65f);
    }

    public class CameraRotation : CameraModule<CameraRotationSettings>
    {
        private float _xRotation;
        private float _yRotation;
        private float _zRotation;
        
        private Func<Vector2> _inputGetter;

        public override void Initialize(Transform controlledTransform, CameraRotationSettings settings)
        {
            base.Initialize(controlledTransform, settings);
            
            _xRotation = settings.InitialTiltAngle.x;
            _yRotation = settings.InitialTiltAngle.y;
            _zRotation = settings.InitialTiltAngle.z;
        }
        
        public override void SetTarget(Transform transform)
        {
            base.SetTarget(transform);
            
            if (transform != null)
                _yRotation = transform.eulerAngles.y;
            
            ApplyRotation();
        }
        
        public void SetInputGetter(Func<Vector2> inputGetter) => _inputGetter = inputGetter;

        public override void OnLateUpdate()
        {
            if (!isEnabled || _inputGetter == null) 
                return;
            
            HandleRotation(_inputGetter());
        }

        private void HandleRotation(Vector2 input)
        {
            _xRotation -= input.y * cameraSettings.Sensitivity.y;
            _xRotation = Mathf.Clamp(_xRotation, cameraSettings.PitchLimits.x, cameraSettings.PitchLimits.y);
            
            if (targetTransform != null)
                _yRotation = targetTransform.eulerAngles.y;
            
            ApplyRotation();
        }

        private void ApplyRotation() 
            => cameraTransform.localRotation = Quaternion.Euler(_xRotation, _yRotation, _zRotation);
    }
}