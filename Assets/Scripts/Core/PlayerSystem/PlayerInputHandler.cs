using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.PlayerSystem
{
    public sealed class PlayerInputHandler : MonoBehaviour, IUpdatable, ILateUpdatable
    {
        private const float Input_Deadzone = 0.01f;
        private const float Input_Deadzone_Sqr = Input_Deadzone * Input_Deadzone;
        
        private const float Max_Length_Magnitude = 1.0f;
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool SprintInput { get; private set; }
        
        private float _horizontal;
        private float _vertical;
        
        private bool _isInputActive = true;

        private void OnEnable()
        {
            Player.Instance.RegisterUpdatable(this);
            Player.Instance.RegisterLateUpdatable(this);
            
            InputManager.OnHorizontalAxis += HandleHorizontalAxis;
            InputManager.OnVerticalAxis += HandleVerticalAxis;
            InputManager.OnJumpPerformed += OnJump;
            InputManager.OnSprintStarted += OnSprintStarted;
            InputManager.OnSprintCanceled += OnSprintCanceled;
        }

        private void OnDisable()
        {
            Player.Instance.UnregisterUpdatable(this);
            Player.Instance.UnregisterLateUpdatable(this);
            
            InputManager.OnHorizontalAxis -= HandleHorizontalAxis;
            InputManager.OnVerticalAxis -= HandleVerticalAxis;
            InputManager.OnJumpPerformed -= OnJump;
            InputManager.OnSprintStarted -= OnSprintStarted;
            InputManager.OnSprintCanceled -= OnSprintCanceled;
        }
        
        public void OnUpdate()
        {
            if (!_isInputActive)
                return;
            
            Vector2 rawMoveInput = new Vector2(_horizontal, _vertical);
            Vector2 rawMouseInput = Mouse.current.delta.ReadValue();
            
            MoveInput = rawMoveInput.sqrMagnitude < Input_Deadzone_Sqr
                ? Vector2.zero
                : Vector2.ClampMagnitude(rawMoveInput, Max_Length_Magnitude);
            
            LookInput = rawMouseInput.sqrMagnitude < Input_Deadzone_Sqr
                ? Vector2.zero
                : rawMouseInput;
        }

        public void OnLateUpdate()
        {
            if (JumpInput)
                JumpInput = false;
        }
        
        public void SetInputActive(bool isActive)
        {
            _isInputActive = isActive;

            if (!isActive)
            {
                MoveInput = Vector2.zero;
                LookInput = Vector2.zero;
                JumpInput = false;
                SprintInput = false;
                _horizontal = 0;
                _vertical = 0;
            }
        }

        private void HandleHorizontalAxis(float axisValue) => _horizontal = axisValue;

        private void HandleVerticalAxis(float axisValue) => _vertical = axisValue;
        
        private void OnJump() => JumpInput = true;
        
        private void OnSprintStarted() => SprintInput = true;
        private void OnSprintCanceled() => SprintInput = false;
    }
}
