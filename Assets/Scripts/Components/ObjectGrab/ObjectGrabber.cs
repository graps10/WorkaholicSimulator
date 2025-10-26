using System;
using Core;
using Core.InputManager;
using Core.Interfaces;
using Core.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Components.ObjectGrab
{
    public class ObjectGrabber : MonoBehaviour, IUpdatable
    {
        private const float Ray_Length = 100;
        
        private static readonly Vector3 rotationStep = new(0f, 0f, 90f);
        private static readonly float[] allowedRotationAngles = { -180f, -90f, 0, 90f, 180f };

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float offsetLength;
        [SerializeField] private float smoothTime;
        [Space] [SerializeField] private float grabbedObjectRotationTime = 0.1f;

        public Action<Transform> OnGrab;
        public Action<Transform> OnRelease;

        private Ray _ray;
        private RaycastHit _raycastHit;
        
        private IGrabbable _grabbed;
        private Transform _grabbedTransform;

        private Vector3 _smoothVelocity;
        private Camera _camera;

        private void OnEnable()
        {
            if (Player.Instance != null)
                Player.Instance.RegisterUpdatable(this);
            
            SceneManager.OnAfterNewSceneLoaded_ActionList += HandleNewSceneLoaded;
        }

        private void OnDisable()
        {
            if (Player.Instance != null)
                Player.Instance.UnregisterUpdatable(this);
            
            SceneManager.OnAfterNewSceneLoaded_ActionList -= HandleNewSceneLoaded;
        }

        public void OnUpdate()
        {
            if (_camera == null) return;

            if (InputManager.CurrentInputDevice == InputDeviceType.Keyboard)
            {
                var screenPoint = Mouse.current.position.ReadValue();
                _ray = _camera.ScreenPointToRay(screenPoint);
            }

            if (_grabbed == null)
            {
                /*if (InputManager.leftMouseButton.WasPressedThisFrame())
                {
                    if (!Physics.Raycast(_ray, out _raycastHit, Ray_Length, layerMask))
                        return;
                    GrabObject();
                }*/
            }
            else
            {
                if (_grabbed.CanBeRotated && Input.GetMouseButtonDown(1))
                    _grabbedTransform.DOLocalRotate(_grabbedTransform.eulerAngles +
                                                    rotationStep, grabbedObjectRotationTime);

                /*if (InputManager.leftMouseButton.WasReleasedThisFrame())
                {
                    ReleaseObject();
                    return;
                }*/

                if (!Physics.Raycast(_ray, out _raycastHit, Ray_Length)) return;

                MoveObject();
            }
        }

        private void GrabObject()
        {
            var hitObject = _raycastHit.collider.gameObject;
            if (!hitObject.TryGetComponent(out IGrabbable item))
                return;

            _grabbed = item;
            _grabbed.Grab();
            _grabbedTransform = _raycastHit.transform;

            RotateToAllowedAngle(_grabbedTransform);
            OnGrab?.Invoke(_grabbedTransform);
        }

        private void MoveObject()
        {
            if (_grabbed == null) return;

            var offset = -_ray.direction * offsetLength;
            var targetPosition = _raycastHit.point + offset;

            _grabbedTransform.position = Vector3.SmoothDamp(
                _grabbedTransform.position,
                targetPosition,
                ref _smoothVelocity,
                smoothTime
            );
        }

        private void ReleaseObject()
        {
            _grabbed.Release();
            _grabbed = null;

            OnRelease?.Invoke(_grabbedTransform);
            _grabbedTransform = null;
        }

        public void EnableGrabbing(bool enable)
        {
            enabled = enable;

            if (!enable && _grabbedTransform != null)
                ReleaseObject();
        }

        private void RotateToAllowedAngle(Transform transform)
        {
            var euler = transform.localEulerAngles;
            euler.z = GetClosestRightAngleZ(transform);
            transform.localEulerAngles = euler;
        }

        private float GetClosestRightAngleZ(Transform transform)
        {
            float zRotation = MathUtils.NormalizeAngle(transform.localEulerAngles.z);

            float closest = allowedRotationAngles[0];
            float minDiff = Mathf.Abs(zRotation - allowedRotationAngles[0]);

            foreach (float angle in allowedRotationAngles)
            {
                float diff = Mathf.Abs(zRotation - angle);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = angle;
                }
            }

            return closest;
        }
        
        private void HandleNewSceneLoaded() => _camera = Camera.main;
    }
}