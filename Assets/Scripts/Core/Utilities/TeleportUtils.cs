using System;
using System.Collections.Generic;
using Transition;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Utilities
{
    public static class TeleportUtils
    {
        public static bool IsTeleportingWithAnimation { get; private set; }

        public static void TeleportAnimation(Transform teleportObject, Vector3 position, Quaternion rotation,
            Action afterTeleport = null, Action afterEndTeleportAnimation = null, TransitionManager.LoadMode loadMode = TransitionManager.LoadMode.Fade)
        {
            if (IsTeleportingWithAnimation || teleportObject == null) 
                return;
            
            IsTeleportingWithAnimation = true;

            Action teleportAction = () =>
            {
                Teleport(teleportObject, position, rotation, null);
                IsTeleportingWithAnimation = false;
                afterTeleport?.Invoke();
            };

            List<Action> postTransitionActions = new List<Action>();
            if (afterEndTeleportAnimation != null)
            {
                postTransitionActions.Add(afterEndTeleportAnimation);
            }

            TransitionManager.StartTransition(loadMode, teleportAction, null, postTransitionActions, expectSceneLoad: false);
        }

        private static void Teleport(Transform teleportObject, Vector3 position, Quaternion rotation,
            UnityEvent afterTeleport)
        {
            teleportObject.position = position;
            teleportObject.rotation = rotation;

            Physics.SyncTransforms();

            if (teleportObject.TryGetComponent(out Rigidbody rigidbody))
                UtilsProvider.WaitAndRun(() => rigidbody.velocity = Vector3.zero, true);

            afterTeleport?.Invoke();
        }
    }
}