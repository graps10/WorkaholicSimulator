using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement: MonoBehaviour, IUpdatable
    {
        private const float Jump_Gravity_Multiplier = -2.0f;
        
        [Header("References")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CharacterController characterController;

        [Header("Settings")]
        [SerializeField] private float walkSpeed = 5.0f; 
        [SerializeField] private float sprintSpeed = 8.0f;
        
        [Header("Rotation Settings")]
        [Tooltip("Sensitivity for body rotation (Yaw/Horizontal)")]
        [SerializeField] private float lookSensitivity = 0.1f;

        [Header("Gravity Settings")]
        [Tooltip("The strength of gravity.")]
        [SerializeField] private float gravity = -9.81f;
        [Tooltip("Small downward force applied when grounded to keep the player stuck to the ground.")]
        [SerializeField] private float groundedResetVelocity = -2.0f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 1.0f;

        /// <summary>
        /// Stores the player's current vertical speed (affected by gravity).
        /// </summary>
        private float _verticalVelocity;
        
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
            Vector3 horizontalVelocity = GetHorizontalVelocity();
            
            HandleJump();
            HandleGravity();
            
            Vector3 finalVelocity = horizontalVelocity + (Vector3.up * _verticalVelocity);
            
            characterController.Move(finalVelocity * Time.deltaTime);
            
            HandleRotation();
        }
        
        private Vector3 GetHorizontalVelocity()
        {
            float currentSpeed = inputHandler.SprintInput ? sprintSpeed : walkSpeed;
            
            Vector2 input = inputHandler.MoveInput;
            Vector3 moveDirection = (transform.right * input.x) + (transform.forward * input.y);
            
            return moveDirection * currentSpeed;
        }
        
        private void HandleJump()
        {
            if (inputHandler.JumpInput && characterController.isGrounded)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * Jump_Gravity_Multiplier * gravity);
        }
        
        private void HandleGravity()
        {
            if (characterController.isGrounded && _verticalVelocity < 0.0f)
                _verticalVelocity = groundedResetVelocity;
            else
                _verticalVelocity += gravity * Time.deltaTime;
        }
        
        private void HandleRotation()
        {
            float mouseX = inputHandler.LookInput.x;
            
            if (Mathf.Abs(mouseX) == 0f) 
                return;
            
            float rotationAmount = mouseX * lookSensitivity;
            transform.Rotate(Vector3.up * rotationAmount);
        }
    }
}