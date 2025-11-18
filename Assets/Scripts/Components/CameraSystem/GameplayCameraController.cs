using System;
using Components.CameraSystem.Modules;
using Core.Interfaces;
using Core.PlayerSystem;
using UnityEngine;

namespace Components.CameraSystem
{
    public class GameplayCameraController : MonoBehaviour, ILateUpdatable
    {
        private const float Set_Target_Delay = 1f;
            
        [Header("Position")]
        [SerializeField] private Transform cameraPositionParent;
        [SerializeField] private CameraPositionSettings positionSettings;

        [Header("Rotation")]
        [SerializeField] private Transform cameraRotationParent;
        [SerializeField] private CameraRotationSettings rotationSettings;

        [Header("Bobbing")]
        [SerializeField] private Transform cameraBobbingParent;
        [SerializeField] private CameraBobbingSettings bobbingSettings;
        
        [Space]
        [Header("Camera Reference")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform targetEntity;
        
        private CameraPosition _position;
        private CameraRotation _rotation;
        private CameraBobbing _bobbing;

        private void OnEnable()
        {
            Player.Instance.RegisterLateUpdatable(this);
        }

        private void OnDisable()
        {
            Player.Instance.UnregisterLateUpdatable(this);
        }

        private void Start()
        {
            _position = new CameraPosition();
            _rotation = new CameraRotation();
            _bobbing = new CameraBobbing();
            
            _position.Initialize(cameraPositionParent, positionSettings);
            _rotation.Initialize(cameraRotationParent, rotationSettings);
            _bobbing.Initialize(cameraBobbingParent, bobbingSettings);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            Core.Utilities.UtilsProvider.WaitAndRun(() =>
            {
                SetTarget(Player.Instance.PlayerEntityGameObject.transform);
            }, true, Set_Target_Delay);
        }

        public void OnLateUpdate()
        {
            if (targetEntity == null) return;

            _position.OnLateUpdate();
            _rotation.OnLateUpdate();
            _bobbing.OnLateUpdate();
        }

        public void SetTarget(Transform targetToSet)
        {
            targetEntity = targetToSet;
            
            _position.SetTarget(targetToSet);
            _rotation.SetTarget(targetToSet);
            
            var inputHandler = targetToSet.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
                _rotation.SetInputGetter(() => inputHandler.LookInput);
            else
                Debug.LogWarning($"PlayerInputHandler not found on target {targetToSet.name}");
            
            var targetController = targetToSet.GetComponent<CharacterController>();
            
            if (targetController != null)
                _bobbing.SetSpeedGetter(() => targetController.velocity.magnitude);
        }
    }
}
