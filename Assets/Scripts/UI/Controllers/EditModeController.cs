using Core.InputSystem;
using UI.CanvasCommands;
using UnityEngine;

namespace UI.Controllers
{
    public class EditModeController : MonoBehaviour
    {
        private EditModeCanvasCommand _uiCommand;

        [ContextMenu("Show EditMode Canvas")]
        private void ShowEditUI()
        {
            CursorController.ToggleCursor(true);
            
            if (CanvasCommandConstructor.Instance != null)
                _uiCommand = CanvasCommandConstructor.Instance.Load<EditModeCanvasCommand>();

            if (CanvasCommandConstructor.Instance != null) // temporary
                CanvasCommandConstructor.Instance.Load<MoneyCanvasCommand>();

            if (CanvasCommandConstructor.Instance != null)
                CanvasCommandConstructor.Instance.Load<CrosshairCanvasCommand>();
        }

        [ContextMenu("Hide EditMode Canvas")]
        private void HideEditUI()
        {
            CursorController.ToggleCursor(false);
            
            if (_uiCommand != null && !_uiCommand.IsDisposed)
            {
                _uiCommand.Dispose();
                _uiCommand = null;
            }
        }
    }
}