using UnityEngine;

namespace Components.CameraSystem
{
    public abstract class CameraModule<TCamSettings> where TCamSettings : CameraSettings
    {
        protected Transform cameraTransform;
        protected TCamSettings cameraSettings;
        
        protected bool isEnabled;

        public virtual void Initialize(Transform controlledTransform, TCamSettings settings)
        {
            cameraTransform = controlledTransform;
            cameraSettings = settings;

            isEnabled = true;
        }

        public abstract void SetCameraPosition();
    }
}
