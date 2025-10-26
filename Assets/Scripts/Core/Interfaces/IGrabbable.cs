namespace Core.Interfaces
{
    public delegate void GrabbableObjectEvent();

    public interface IGrabbable
    {
        public event GrabbableObjectEvent OnGrabbed;
        public event GrabbableObjectEvent OnReleased;
        public bool CanBeRotated { get => false; }

        public void Enable() { }
        public void Disable() { }
        public bool IsGrabbing();

        public void Grab();
        public void Release();
    }
}