using System;
using UnityEngine;

namespace Components.CameraSystem
{
    [Serializable]
    public class CameraTiltSettings : CameraSettings
    {
        [Tooltip("Initial camera tilt angle")]
        public Vector3 InitialTiltAngle;
    }

    public class CameraTilt : CameraModule<CameraTiltSettings>
    {
        private Transform _xRotationParent;
        private Transform _yRotationParent;
        private Transform _zRotationParent;


        public void Initialize(Transform xRotationParent, Transform yRotationParent, Transform zRotationParent,
            CameraTiltSettings settings)
        {
            base.Initialize(xRotationParent, settings);

            _xRotationParent = xRotationParent;
            _yRotationParent = yRotationParent;
            _zRotationParent = zRotationParent;

            _xRotationParent.localRotation = Quaternion.Euler(settings.InitialTiltAngle.x, 0, 0);
            _yRotationParent.localRotation = Quaternion.Euler(0, settings.InitialTiltAngle.y, 0);
            _zRotationParent.localRotation = Quaternion.Euler(0, 0, settings.InitialTiltAngle.z);
        }

        public override void SetCameraPosition()
        {
            if (!isEnabled) return;

            // runtime future logic
        }
    }
}