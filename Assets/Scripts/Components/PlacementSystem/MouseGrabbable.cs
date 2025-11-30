using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using Hypertonic.Modules.UltimateSockets.XR;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class MouseGrabbable : MonoBehaviour, IGrabbableItem
    {
        public event IGrabbableItemEvent OnGrabbed;
        public event IGrabbableItemEvent OnReleased;

        private bool _isGrabbed;
        
        public void Grab()
        {
            _isGrabbed = true;
            OnGrabbed?.Invoke();
        }
        
        public void Release()
        {
            _isGrabbed = false;
            OnReleased?.Invoke();
        }

        public void Enable() 
        {
            // disable colliders or smth
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public bool IsGrabbing() => _isGrabbed;

        public void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem) { }
    }
}

