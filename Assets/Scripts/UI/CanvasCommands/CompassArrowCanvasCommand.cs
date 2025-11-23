using UI.CanvasReceivers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CanvasCommands
{
    public class CompassArrowCanvasCommand : UpdatableCanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/CompassArrow";
        public override string CanvasCommandPath => Path;
        
        private const float MaxAngleForFullResponse = 30f;
        
        private static Vector2 springStiffness = new(37f, 75f); // Strength of the spring force
        private static Vector2 springDamping = new(1f, 5f); // Resistance to motion (higher = less wobble)
        
        private static Vector2 screenPositionPadding = new(-150, -150);
        
        public bool IsEnabled => _arrowImage != null && _arrowImage.gameObject.activeSelf;
        
        private Transform _originTransform;
        private Transform _targetTransform;

        private Transform _arrowTransform;
        private Image _arrowImage;

        private float _currentVelocity;  
        private float _currentAngle;

        public override void Initialize(CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            
            ((RectTransform)transform).anchoredPosition = 
                new Vector2(screenPositionPadding.x, screenPositionPadding.y);

            _arrowTransform = transform;
            _arrowImage = _arrowTransform.GetComponentInChildren<Image>();
        }
        
        public void SetOrigin(Transform origin) => _originTransform = origin;
        public void SetTarget(Transform target) => _targetTransform = target;

        public override void OnUpdate()
        {
            if (_originTransform == null) 
                return; 
            
            float targetRotation = CalculateTargetAngle();
            float currentRotation = _arrowTransform.localRotation.eulerAngles.z;

            float angleDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
            float absoluteDifference = Mathf.Abs(angleDifference);

            // Adaptive Stiffness & Damping (higher force when far, higher damping when close)
            float t = absoluteDifference / MaxAngleForFullResponse;
            float stiffness = Mathf.Lerp(springStiffness.x, springStiffness.y, t);
            float damping = Mathf.Lerp(springDamping.x, springDamping.y, t);

            // Apply Hooke's Law: Force = -stiffness * displacement
            float force = -stiffness * angleDifference;

            // Apply damping: Opposes velocity
            float dampingForce = -damping * _currentVelocity;

            // Compute acceleration (simplified physics: F = ma, assume m = 1)
            float acceleration = force + dampingForce;

            // Integrate velocity
            _currentVelocity += acceleration * Time.deltaTime;

            // Integrate position (apply movement)
            _currentAngle += _currentVelocity * Time.deltaTime;

            // Apply final rotation
            _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, -_currentAngle);
        }

        private float CalculateTargetAngle()
        {
            if (_originTransform == null || _targetTransform == null) 
                return _currentAngle;
            
            var direction = _targetTransform.position - _originTransform.position;
            return Mathf.Atan2(-direction.x, direction.z) * Mathf.Rad2Deg;
        }

        public void EnableImage(bool enable) => _arrowImage.gameObject.SetActive(enable);
    }
}