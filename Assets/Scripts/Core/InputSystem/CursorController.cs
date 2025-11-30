using UnityEngine;

namespace Core.InputSystem
{
    public static class CursorController
    {
        public static void ToggleCursor(bool show)
        {
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}