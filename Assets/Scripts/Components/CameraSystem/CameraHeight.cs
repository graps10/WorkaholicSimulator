using System;
using UnityEngine;

namespace Components.CameraSystem
{
    [Serializable]
    public class CameraHeightSettings : CameraSettings
    {
        [Tooltip("Basic Y offset from target")]
        public float BaseYOffset;
    }

    public class CameraHeight : CameraModule<CameraHeightSettings>
    {

        public override void SetCameraPosition()
        {
            if (!isEnabled) return;
            
            ApplyHeight(cameraSettings.BaseYOffset);
        }

        private void ApplyHeight(float height)
        {
            var localPos = cameraTransform.localPosition;
            localPos.y = height;
            cameraTransform.localPosition = localPos;
        }
    }
}