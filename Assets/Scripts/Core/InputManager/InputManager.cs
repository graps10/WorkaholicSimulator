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

        public static event ButtonDelegate OnStopOrAlternativeUse, OnUseStop, OnPauseCancel;

        public static event AxisDelegate OnHorizontalAxis, OnVerticalAxis;
        
		public static bool IsControldByMouse
		{
			get => isControledByMouse;
			set
			{
				isControledByMouse = value;
				/*if (isContolsByMouse) ChangeControlsToMouse();
				else ChangeControlsToMain();*/
			}
		}
		
		private static float mouseSensitivity = 1;
		private static bool isControledByMouse;
		
		public static event Action<InputDeviceType> OnInputDeviceChanged;
		
		internal static void Initialize()
        {
            inputActions = new();
            inputActions.Enable();
			IsControldByMouse = false;

			Player.Instance.OnUpdateEvent += OnUpdate;

            horizontalAxis.performed += CheckDevice;
            verticalAxis.performed += CheckDevice;
            //inputActions.MouseControls.MovementHorizontal.performed += CheckDevice;
            //inputActions.MouseControls.MovementVertical.performed += CheckDevice;
            //inputActions.GamepadCursorControls.MovementVertical.performed += CheckDevice;
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
	        
	        if (pauseCancel.WasPressedThisFrame()) // temporary solution
		        OnPauseCancel?.Invoke();
	        
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
			if (IsControldByMouse)
			{
				var mouseDeltaX = Mouse.current.delta.x.ReadValue();
				if (Mathf.Abs(mouseDeltaX) > 0.1f)
				{
					// We scale down the delta to fit into range of -1 to 1
					horizontalInput = (2f / Mathf.PI) * Mathf.Atan(mouseDeltaX) * mouseSensitivity;

					// Extra check for better safety
					horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
				}
			}
			else if(horizontalAxis.ReadValue<float>()!=0)
				horizontalInput = horizontalAxis.ReadValue<float>();
			
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

		public static void ToggleInputActions(bool state)
		{
			if (state)
				inputActions.Enable();
			else
				inputActions.Disable();
		}
		
		/*private static void ChangeControlsToMain()
		{
			horizontalAxis = inputActions.MainControls.MovementHorizontal;
			verticalAxis = inputActions.MainControls.MovementVertical;
			stopAlternativeUse = inputActions.MainControls.StopAlternativeUse;
			useStop = inputActions.MainControls.UseStop;
			pauseCancel = inputActions.MainControls.PauseCancel;
            leftMouseButton = inputActions.MainControls.LeftMouseButton;
		}
		
		private static void ChangeControlsToMouse()
		{
			horizontalAxis = inputActions.MouseControls.MovementHorizontal;
			verticalAxis = inputActions.MouseControls.MovementVertical;
			stopAlternativeUse = inputActions.MouseControls.StopAlternativeUse;
			useStop = inputActions.MouseControls.UseStop;
			pauseCancel = inputActions.MouseControls.PauseCancel;
            leftMouseButton = inputActions.MainControls.LeftMouseButton;
        }*/

		private static void Dispose()
        {
	        if(Player.Instance != null)
		        Player.Instance.OnUpdateEvent -= OnUpdate;
	        
	        if (inputActions != null)
	        {
		        /*inputActions.MouseControls.MovementHorizontal.performed -= CheckDevice;
		        inputActions.MouseControls.MovementVertical.performed -= CheckDevice;
		        inputActions.GamepadCursorControls.MovementVertical.performed -= CheckDevice;*/
		        
		        inputActions.Disable();
		        inputActions.Dispose();
		        inputActions = null;
	        }

	        if (horizontalAxis != null)
            {
	            horizontalAxis.performed -= CheckDevice;
	            horizontalAxis.Dispose();
	            horizontalAxis = null;
            }
	        
	        if (verticalAxis != null)
	        {
		        verticalAxis.performed -= CheckDevice;
		        verticalAxis.Dispose();
		        verticalAxis = null;
	        }
        }
	}
}