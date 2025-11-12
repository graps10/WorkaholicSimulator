using Core.PlayerSystem;
using UnityEngine;

namespace Components.CameraSystem
{
    public class GameplayCameraController : MonoBehaviour
    {
        [Header("Global Position")]
        [SerializeField] private Transform cameraPositionParent;
        [SerializeField] private CameraPositionSettings positionSettings;

        [Header("Height")]
        [SerializeField] private Transform heightParent;
        [SerializeField] private CameraHeightSettings heightSettings;

        [Header("Tilt")]
        [SerializeField] private Transform xRotationParent;
        [SerializeField] private Transform yRotationParent;
        [SerializeField] private Transform zRotationParent;
        [SerializeField] private CameraTiltSettings tiltSettings;

        [Space]
        [Header("Camera Reference")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [SerializeField] private Transform targetEntity;
        
        private CameraPosition _position;
        private CameraHeight _height;
        private CameraTilt _tilt;

        private void Start()
        {
            _position = new CameraPosition();
            _height = new CameraHeight();
            _tilt = new CameraTilt();
            
            _position.Initialize(cameraPositionParent, positionSettings);
            _height.Initialize(heightParent, heightSettings);
            _tilt.Initialize(xRotationParent, yRotationParent, zRotationParent, tiltSettings);

            Core.Utilities.UtilsProvider.WaitAndRun(() =>
            {
                SetTarget(Player.Instance.PlayerEntityGameObject.transform);
            }, true, 0.1f);
        }

        private void LateUpdate()
        {
            if (targetEntity == null) return;

            UpdatePosition();
        }

        public void SetTarget(Transform targetToSet)
        {
            targetEntity = targetToSet;
            _position.SetTarget(targetToSet);
        }
        
        private void UpdatePosition()
        {
            _position.SetCameraPosition();
            _height.SetCameraPosition();
            _tilt.SetCameraPosition();
        }
    }
}
