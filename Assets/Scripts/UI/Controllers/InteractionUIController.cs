using Core.PlayerSystem;
using UI.CanvasCommands;
using UnityEngine;

namespace UI.Controllers
{
    public class InteractionUIController: MonoBehaviour
    {
        private InteractionCanvasCommand _activeCommand;

        private void OnEnable()
        {
            PlayerInteractionSensor.OnInteractionAvailabilityChanged += HandleInteractionChange;
        }

        private void OnDisable()
        {
            PlayerInteractionSensor.OnInteractionAvailabilityChanged -= HandleInteractionChange;
            HideInteractionPrompt();
        }

        private void HandleInteractionChange(bool isInteractable)
        {
            if (isInteractable)
                ShowInteractionPrompt();
            else
                HideInteractionPrompt();
        }

        private void ShowInteractionPrompt()
        {
            if (_activeCommand != null && !_activeCommand.IsDisposed) return;
            
            _activeCommand = CanvasCommandConstructor.Instance.Load<InteractionCanvasCommand>();
        }

        private void HideInteractionPrompt()
        {
            if (_activeCommand != null && !_activeCommand.IsDisposed)
            {
                _activeCommand.Dispose();
                _activeCommand = null;
            }
        }
    }
}