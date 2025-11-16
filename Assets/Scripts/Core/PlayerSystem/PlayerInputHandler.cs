using System;
using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.PlayerSystem
{
    public sealed class PlayerInputHandler : MonoBehaviour, IUpdatable
    {
        private const float Input_Deadzone = 0.01f;
        private const float Input_Deadzone_Sqr = Input_Deadzone * Input_Deadzone;
        
        private const float Max_Length_Magnitude = 1.0f;
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        
        private float _horizontal;
        private float _vertical;

        private void OnEnable()
        {
            Player.Instance.RegisterUpdatable(this);
            
            InputManager.OnHorizontalAxis += HandleHorizontalAxis;
            InputManager.OnVerticalAxis += HandleVerticalAxis;
            InputManager.OnJumpPerformed += OnJump;
        }

        private void OnDisable()
        {
            Player.Instance.UnregisterUpdatable(this);
            
            InputManager.OnHorizontalAxis -= HandleHorizontalAxis;
            InputManager.OnVerticalAxis -= HandleVerticalAxis;
            InputManager.OnJumpPerformed -= OnJump;
        }
        
        public void OnUpdate()
        {
            Vector2 rawMoveInput = new Vector2(_horizontal, _vertical);
            Vector2 rawMouseInput = Mouse.current.delta.ReadValue();
            
            MoveInput = rawMoveInput.sqrMagnitude < Input_Deadzone_Sqr
                ? Vector2.zero
                : Vector2.ClampMagnitude(rawMoveInput, Max_Length_Magnitude);
            
            LookInput = rawMouseInput.sqrMagnitude < Input_Deadzone_Sqr
                ? Vector2.zero
                : rawMouseInput;
        }

        private void LateUpdate()
        {
            if (JumpInput)
                JumpInput = false;
        }

        private void HandleHorizontalAxis(float axisValue) => _horizontal = axisValue;

        private void HandleVerticalAxis(float axisValue) => _vertical = axisValue;
        
        private void OnJump() => JumpInput = true;
    }
}
