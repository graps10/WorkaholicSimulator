namespace Core.Interfaces
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact();
    }
}