using System;
using UnityEngine;

namespace Components.CameraSystem
{
    [Serializable]
    public class CameraPositionSettings : CameraSettings
    {
        [Tooltip("Basic position offset from target")]
        public Vector3 Offset;
    }
    
    public class CameraPosition : CameraModule<CameraPositionSettings>
    {
        private Transform _targetTransform;
        
        public void SetTarget(Transform targetTransform)
        {
            _targetTransform = targetTransform;
        }

        public override void SetCameraPosition()
        {
            if (!isEnabled || _targetTransform == null) return;
            
            cameraTransform.position = _targetTransform.position + cameraSettings.Offset;
        }
    }
}