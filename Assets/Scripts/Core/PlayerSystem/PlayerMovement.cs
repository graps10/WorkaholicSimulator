using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement: MonoBehaviour, IUpdatable
    {
        [Header("References")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CharacterController characterController;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        
        [Header("Rotation Settings")]
        [Tooltip("Sensitivity for body rotation (Yaw/Horizontal)")]
        [SerializeField] private float lookSensitivity = 0.1f;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
        }
        
        private void OnDisable()
        {
            if (Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
        }

        public void OnUpdate()
        {
            HandleRotation();
            HandleMovement();
        }
        
        private void HandleRotation()
        {
            float mouseX = inputHandler.LookInput.x;
            
            if (Mathf.Abs(mouseX) == 0f) 
                return;
            
            float rotationAmount = mouseX * lookSensitivity;
            transform.Rotate(Vector3.up * rotationAmount);
        }

        private void HandleMovement()
        {
            Vector2 input = inputHandler.MoveInput;
            Vector3 moveDirection = (transform.right * input.x) + (transform.forward * input.y);
            characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
        }
    }
}