using Core.Interfaces;
using UnityEngine;

namespace Entities.Interactables
{
    public class TestInteractableBox : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => "Open Box";

        public void Interact() => Debug.Log("Box Opened! (Interaction Logic works)");
    }
}