using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;

namespace Hypertonic.Modules.UltimateSockets.XR
{
    public delegate void IGrabbableItemEvent();

    public interface IGrabbableItem
    {
        public event IGrabbableItemEvent OnGrabbed;
        public event IGrabbableItemEvent OnReleased;
        public bool CanBeRotated { get => false; }

        public void Enable() { }
        public void Disable() { }
        public bool IsGrabbing();
        public void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem) { }
        public void HandlePlacedInSocket(Socket socket, PlaceableItem placeableItem) { }
        public void HandleEnteredPlaceableZone(Socket socket, PlaceableItem placeableItem) { }
        public void HandleExitedPlaceableZone(Socket socket, PlaceableItem placeableItem) { }

        public void Grab();
        public void Release();
    }

}
