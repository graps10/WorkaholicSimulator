using Core.InputSystem;
using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    public sealed class PlayerInputHandler : MonoBehaviour, IUpdatable
    {
        public Vector2 MoveInput { get; private set; }
        
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
            MoveInput = Vector2.ClampMagnitude(rawInput, 1.0f);
        }

        private void HandleHorizontalAxis(float axisValue) => _horizontal = axisValue;

        private void HandleVerticalAxis(float axisValue) => _vertical = axisValue;
    }
}
