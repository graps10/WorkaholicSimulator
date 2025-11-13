using Core.PlayerSystem;
using UnityEngine;

namespace Components.CameraSystem
{
    public class GameplayCameraController : MonoBehaviour
    {
        private const float Set_Target_Delay = 1f;
            
        [Header("Position")]
        [SerializeField] private Transform cameraPositionParent;
        [SerializeField] private CameraPositionSettings positionSettings;

        [Header("Rotation")]
        [SerializeField] private Transform cameraRotationParent;
        [SerializeField] private CameraRotationSettings rotationSettings;

        [Space]
        [Header("Camera Reference")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform targetEntity;
        
        private CameraPosition _position;
        private CameraRotation _rotation;

        private void Start()
        {
            _position = new CameraPosition();
            _rotation = new CameraRotation();
            
            _position.Initialize(cameraPositionParent, positionSettings);
            _rotation.Initialize(cameraRotationParent, rotationSettings);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            Core.Utilities.UtilsProvider.WaitAndRun(() =>
            {
                SetTarget(Player.Instance.PlayerEntityGameObject.transform);
            }, true, Set_Target_Delay);
        }

        private void LateUpdate()
        {
            if (targetEntity == null) return;

            _position.OnLateUpdate();
            _rotation.OnLateUpdate();
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
        }
    }
}
