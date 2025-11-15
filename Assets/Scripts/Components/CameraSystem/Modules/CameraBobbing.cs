using System;
using UnityEngine;

namespace Components.CameraSystem.Modules
{
    [Serializable]
    public class CameraBobbingSettings : CameraSettings
    {
        [Header("Bob Frequency")]
        [Tooltip("Frequency (speed) of the bob when at max speed")]
        public float Frequency = 1.0f;

        [Header("Positional Bob")]
        [Tooltip("Horizontal (X) bobbing animation shape")]
        public AnimationCurve BobCurveX;
        [Tooltip("Vertical (Y) bobbing animation shape")]
        public AnimationCurve BobCurveY;

        [Space(10)]
        [Tooltip("Amplitude (strength) of horizontal bob")]
        public float AmplitudeX = 0.1f;
        [Tooltip("Amplitude (strength) of vertical bob")]
        public float AmplitudeY = 0.2f;

        [Header("Rotational Bob")]
        [Tooltip("Rotation (X-axis, Pitch) bobbing shape")]
        public AnimationCurve BobCurveRotX;
        [Tooltip("Rotation (Z-axis, Roll) bobbing shape")]
        public AnimationCurve BobCurveRotZ;

        [Space(10)]
        [Tooltip("Amplitude (strength) of rotation bob (X-axis)")]
        public float RotAmplitudeX = 1.0f;
        [Tooltip("Amplitude (strength) of rotation bob (Z-axis)")]
        public float RotAmplitudeZ = 0.5f;

        [Tooltip("Speed threshold to start bobbing")]
        public float SpeedThreshold = 0.1f;
        
        [Tooltip("How fast the bob effect blends in/out")]
        public float BlendSpeed = 5.0f;
    }
    
    public class CameraBobbing : CameraModule<CameraBobbingSettings>
    {
        private const float Blend_Target_On = 1.0f;
        private const float Blend_Target_Off = 0.0f;
        private const float Blend_Stop_Threshold = 0.01f;
        private const float Bob_Timer_Max = 1.0f;
        private const float Bob_Ttimer_Reset = 0.0f;
        
        private Func<float> _speedGetter;
    
        private float _bobTime; 
        private float _currentBlend; 
        
        private Vector3 _startPos;
        private Quaternion _startRot;

        public override void Initialize(Transform controlledTransform, CameraBobbingSettings settings)
        {
            base.Initialize(controlledTransform, settings);
            
            _startPos = controlledTransform.localPosition;
            _startRot = controlledTransform.localRotation;
        }

        public void SetSpeedGetter(Func<float> speedGetter) => _speedGetter = speedGetter;

        public override void OnLateUpdate()
        {
            if (!isEnabled || _speedGetter == null) return;
            
            float speed = _speedGetter();
            
            float targetBlend = (speed > cameraSettings.SpeedThreshold) ? Blend_Target_On : Blend_Target_Off;
            
            _currentBlend = Mathf.Lerp(_currentBlend, targetBlend, Time.deltaTime * cameraSettings.BlendSpeed);

            if (_currentBlend < Blend_Stop_Threshold)
            {
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, _startPos, Time.deltaTime * cameraSettings.BlendSpeed);
                cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, _startRot, Time.deltaTime * cameraSettings.BlendSpeed);
                
                _bobTime = Bob_Ttimer_Reset;
                return;
            }
            
            _bobTime += Time.deltaTime * cameraSettings.Frequency * speed * _currentBlend;
            _bobTime %= Bob_Timer_Max;
            
            float posX = cameraSettings.BobCurveX.Evaluate(_bobTime) * cameraSettings.AmplitudeX;
            float posY = cameraSettings.BobCurveY.Evaluate(_bobTime) * cameraSettings.AmplitudeY;
            
            float rotX = cameraSettings.BobCurveRotX.Evaluate(_bobTime) * cameraSettings.RotAmplitudeX;
            float rotZ = cameraSettings.BobCurveRotZ.Evaluate(_bobTime) * cameraSettings.RotAmplitudeZ;
            
            cameraTransform.localPosition = _startPos + new Vector3(posX, posY, 0) * _currentBlend;
            cameraTransform.localRotation = _startRot * Quaternion.Euler(rotX, 0, rotZ * _currentBlend);
        }
    }
}
