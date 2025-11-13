using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.PlayerSystem
{
    public sealed class PlayerInputHandler : MonoBehaviour, IUpdatable
    {
        private const float Max_Length_Magnitude = 1.0f;
        
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        
        private float _horizontal;
        private float _vertical;

        private void OnEnable()
        {
            Player.Instance.RegisterUpdatable(this);
            
            InputManager.OnHorizontalAxis += HandleHorizontalAxis;
            InputManager.OnVerticalAxis += HandleVerticalAxis;
        }

        private void OnDisable()
        {
            Player.Instance.UnregisterUpdatable(this);
            
            InputManager.OnHorizontalAxis -= HandleHorizontalAxis;
            InputManager.OnVerticalAxis -= HandleVerticalAxis;
        }
        
        public void OnUpdate()
        {
            Vector2 rawInput = new Vector2(_horizontal, _vertical);
            Vector2 mouseInput = Mouse.current.delta.ReadValue();
            
            MoveInput = Vector2.ClampMagnitude(rawInput, Max_Length_Magnitude);
            LookInput = mouseInput;
        }

        private void HandleHorizontalAxis(float axisValue) => _horizontal = axisValue;

        private void HandleVerticalAxis(float axisValue) => _vertical = axisValue;
    }
}
