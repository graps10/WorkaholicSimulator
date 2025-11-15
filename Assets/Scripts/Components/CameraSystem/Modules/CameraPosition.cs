using System;
using UnityEngine;

namespace Components.CameraSystem.Modules
{
    [Serializable]
    public class CameraPositionSettings : CameraSettings
    {
        [Tooltip("Basic position offset from target")]
        public Vector3 Offset;
    }
    
    public class CameraPosition : CameraModule<CameraPositionSettings>
    {
        public override void OnLateUpdate()
        {
            if(!isEnabled || targetTransform == null)
                return;
            
            cameraTransform.position = targetTransform.position + cameraSettings.Offset;
        }
    }
}