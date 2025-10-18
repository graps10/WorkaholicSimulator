using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.InputManager
{
    public enum InputDeviceType { Keyboard, Gamepad }

    public static class InputManager
    {
        private static GameInput inputActions;

		private static InputAction horizontalAxis, verticalAxis, leftMouseButton,
			stopAlternativeUse, useStop, pauseCancel;
		
        public static InputDeviceType CurrentInputDevice { get; private set; }

        public delegate void ButtonDelegate();
        public delegate void AxisDelegate(float axis);

        public static event ButtonDelegate OnStopOrAlternativeUse, OnUseStop;
        public static event AxisDelegate OnHorizontalAxis, OnVerticalAxis;
		
		private static float mouseSensitivity = 1;
		private static bool isControledByMouse;
		
		public static event Action<InputDeviceType> OnInputDeviceChanged;
		
		internal static void Initialize()
        {
            inputActions = new();
            inputActions.Enable();

			Player.Instance.OnUpdateEvent += OnUpdate;
        }

        private static void CheckDevice(InputAction.CallbackContext context)
        {
	        var currentDevice = context.action.activeControl.device;
	        var newDevice = currentDevice switch
            {
	            Keyboard or Mouse => InputDeviceType.Keyboard,
	            Touchscreen => InputDeviceType.Gamepad,//ToDo: work with touch screens
	            _ => InputDeviceType.Gamepad
            };

            if (newDevice == CurrentInputDevice) 
	            return;

            CurrentInputDevice = newDevice;
            OnInputDeviceChanged?.Invoke(newDevice);

            Cursor.visible = newDevice == InputDeviceType.Keyboard;
        }

        private static void OnUpdate()
        {
	        if(Player.Instance == null || SceneManager.IsChangingPlaymode || !Application.isPlaying)
		        Dispose();
	        
	        if(Player.Instance.PlayerEntityGameObject == null) 
		        return;

            VerticalAxis();
            HorizontalAxis();

			if (stopAlternativeUse.IsPressed())
				OnStopOrAlternativeUse?.Invoke();

			if (useStop.IsPressed())
				OnUseStop?.Invoke();
		}
        
		private static void HorizontalAxis()
		{
			float horizontalInput = 0f;
			var mouseDeltaX = Mouse.current.delta.x.ReadValue();
			if (Mathf.Abs(mouseDeltaX) > 0.1f)
			{
				// We scale down the delta to fit into range of -1 to 1
				horizontalInput = (2f / Mathf.PI) * Mathf.Atan(mouseDeltaX) * mouseSensitivity;

				// Extra check for better safety
				horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
			}
			
            OnHorizontalAxis?.Invoke(horizontalInput);
        }
		private static void VerticalAxis()
		{
			float verticalInput = 0f;

			if (verticalAxis.ReadValue<float>() != 0)
				verticalInput = verticalAxis.ReadValue<float>();

			verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
			OnVerticalAxis?.Invoke(verticalInput);
        }

		private static void Dispose()
        {
	        if(Player.Instance != null)
		        Player.Instance.OnUpdateEvent -= OnUpdate;
	        
	        if (inputActions != null)
	        {
		        inputActions.Disable();
		        inputActions.Dispose();
		        inputActions = null;
	        }

	        if (horizontalAxis != null)
            {
	            horizontalAxis.Dispose();
	            horizontalAxis = null;
            }
	        
	        if (verticalAxis != null)
	        {
		        verticalAxis.Dispose();
		        verticalAxis = null;
	        }
        }
	}
}