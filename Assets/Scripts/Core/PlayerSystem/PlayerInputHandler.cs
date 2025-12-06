using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    public sealed class PlayerInputHandler : MonoBehaviour, IUpdatable, ILateUpdatable
    {
        private const float Move_Deadzone = 0.01f;
        private const float Move_Deadzone_Sqr = Move_Deadzone * Move_Deadzone;
        
        private const float Look_Deadzone = 0.01f;
        private const float Look_Deadzone_Sqr = Look_Deadzone * Look_Deadzone;
        
        private const float Max_Length_Magnitude = 1.0f;
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool SprintInput { get; private set; }
        
        private float _horizontal;
        private float _vertical;
        
        private Vector2 _rawLookAccumulator;
        
        private bool _isInputActive = true;

        private void OnEnable()
        {
            Player.Instance.RegisterUpdatable(this);
            Player.Instance.RegisterLateUpdatable(this);
            
            InputManager.OnHorizontalAxis += HandleHorizontalAxis;
            InputManager.OnVerticalAxis += HandleVerticalAxis;
            InputManager.OnLookInput += HandleLookInput;
            
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
            InputManager.OnLookInput -= HandleLookInput;
            
            InputManager.OnJumpPerformed -= OnJump;
            InputManager.OnSprintStarted -= OnSprintStarted;
            InputManager.OnSprintCanceled -= OnSprintCanceled;
        }
        
        public void OnUpdate()
        {
            if (!_isInputActive)
                return;
            
            Vector2 rawMoveInput = new Vector2(_horizontal, _vertical);
            MoveInput = rawMoveInput.sqrMagnitude < Move_Deadzone_Sqr
                ? Vector2.zero
                : Vector2.ClampMagnitude(rawMoveInput, Max_Length_Magnitude);
            
            LookInput = _rawLookAccumulator.sqrMagnitude < Look_Deadzone_Sqr
                ? Vector2.zero
                : _rawLookAccumulator;
        }

        public void OnLateUpdate()
        {
            if (JumpInput)
                JumpInput = false;
            
            _rawLookAccumulator = Vector2.zero;
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
        
        private void HandleLookInput(Vector2 delta)
        {
            if (!_isInputActive) return;
            _rawLookAccumulator += delta;
        }
        
        private void OnJump() => JumpInput = true;
        
        private void OnSprintStarted() => SprintInput = true;
        private void OnSprintCanceled() => SprintInput = false;
    }
}
