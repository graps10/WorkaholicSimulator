using Core.PlayerSystem;
using UnityEngine;

namespace Components.JobSystem.Tools
{
    public abstract class JobTool : MonoBehaviour
    {
        [Header("Base Tool Settings")]
        [SerializeField] protected Vector2 swayAmountRange = new(0.02f, 0.06f);
        [SerializeField] protected float swaySmooth = 4.0f;
        [SerializeField] protected float toolMoveThreshold = 0.1f;

        protected PlayerInputHandler inputHandler;
        
        private Vector3 _initialLocalPosition;

        public virtual void Initialize()
        {
            _initialLocalPosition = transform.localPosition;
            
            if (Player.Instance != null)
                inputHandler = Player.Instance.InputHandler;
        }
        
        public virtual void OnToolUpdate()
        {
            HandleSway();
            
            bool isUsing = Input.GetMouseButton(0); // TODO: Add tool usage button
            OnToolUsage(isUsing);
        }
        
        protected abstract void OnToolUsage(bool isInputActive);

        public virtual void OnEquip() => gameObject.SetActive(true);

        public virtual void OnUnequip() => gameObject.SetActive(false);

        private void HandleSway()
        {
            if (inputHandler == null) return;

            var minSwayAmount = swayAmountRange.x;
            var maxSwayAmount = swayAmountRange.y;
            
            float moveX = -inputHandler.LookInput.x * minSwayAmount;
            float moveY = -inputHandler.LookInput.y * minSwayAmount;

            moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
            moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);

            Vector3 finalPosition = new Vector3(moveX, moveY, 0) + _initialLocalPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * swaySmooth);
        }

        protected bool IsMoving() => inputHandler.MoveInput.sqrMagnitude > toolMoveThreshold;
    }
}